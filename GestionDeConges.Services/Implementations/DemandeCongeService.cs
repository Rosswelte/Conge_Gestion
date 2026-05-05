using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;
using GestionDeConges.Core.Interfaces;

namespace GestionDeConges.Services.Implementations;

public class DemandeCongeService(IUnitOfWork uow, INotificationService notifService) : IDemandeCongeService
{
    private readonly IUnitOfWork        _uow  = uow;
    private readonly INotificationService _notif = notifService;

    public async Task<IEnumerable<DemandeConge>> GetToutesAsync()
        => await _uow.Demandes.GetToutesAvecDetailsAsync();

    public async Task<IEnumerable<DemandeConge>> GetParEmployeAsync(int idEmploye)
        => await _uow.Demandes.GetParEmployeAsync(idEmploye);

    public async Task<IEnumerable<DemandeConge>> GetEnAttenteAsync()
        => await _uow.Demandes.GetEnAttenteAsync();

    // ── Soumettre une demande ─────────────────────────────────────────────────
    public async Task<ResultatOperation<DemandeConge>> SoumettreAsync(DemandeConge demande)
    {
        // 1. Validation des dates
        if (demande.DateDebut > demande.DateFin)
            return ResultatOperation<DemandeConge>.Echec("La date de début doit être avant la date de fin.");

        if (demande.DateDebut < DateOnly.FromDateTime(DateTime.Today))
            return ResultatOperation<DemandeConge>.Echec("Impossible de soumettre un congé dans le passé.");

        // 2. Récupérer le type et l'employé
        var type    = await _uow.TypesConges.GetByIdAsync(demande.IdTypeConge);
        var employe = await _uow.Employes.GetByIdAsync(demande.IdEmploye);

        if (type is null || employe is null)
            return ResultatOperation<DemandeConge>.Echec("Type de congé ou employé introuvable.");

        // 3. Vérification sexe (maternité/paternité)
        if (type.SexeRequis == SexeRequisConge.F && employe.Sexe != Sexe.F)
            return ResultatOperation<DemandeConge>.Echec($"Le congé '{type.Libelle}' est réservé aux femmes.");
        if (type.SexeRequis == SexeRequisConge.M && employe.Sexe != Sexe.M)
            return ResultatOperation<DemandeConge>.Echec($"Le congé '{type.Libelle}' est réservé aux hommes.");

        // 4. Calcul jours ouvrables
        int joursOuvrables = CompterJoursOuvrables(demande.DateDebut, demande.DateFin);
        if (joursOuvrables == 0)
            return ResultatOperation<DemandeConge>.Echec("La période sélectionnée ne contient aucun jour ouvrable.");

        // 5. Vérification solde (uniquement si quota > 0)
        if (type.QuotaJours > 0)
        {
            var solde = await _uow.Soldes.GetAsync(demande.IdEmploye, demande.IdTypeConge, demande.DateDebut.Year);
            int restant = solde?.Restant ?? type.QuotaJours;
            if (joursOuvrables > restant)
                return ResultatOperation<DemandeConge>.Echec(
                    $"Solde insuffisant : {restant} jour(s) restant(s) en {type.Libelle}, vous en demandez {joursOuvrables}.");
        }

        // 6. Vérification chevauchement
        bool chevauche = await _uow.Demandes.ChevauchementsExisteAsync(
            demande.IdEmploye, demande.DateDebut, demande.DateFin);
        if (chevauche)
            return ResultatOperation<DemandeConge>.Echec("Vous avez déjà une demande sur cette période.");

        // 7. Vérification effectif minimum du poste
        var avertPoste = await VerifierEffectifPosteAsync(employe.IdPoste, demande.DateDebut, demande.DateFin, demande.IdEmploye);
        // (avertissement non bloquant pour l'employé, il soumet quand même)

        // 8. Enregistrement
        demande.NbJours = joursOuvrables;
        demande.Statut  = StatutDemande.EnAttente;
        await _uow.Demandes.AddAsync(demande);
        await _uow.SaveChangesAsync();
        return ResultatOperation<DemandeConge>.Ok(demande);
    }

    // ── Approuver ────────────────────────────────────────────────────────────
    public async Task<ResultatOperation> ApprouverAsync(int id, int idAdmin, string? commentaire = null)
    {
        var demande = await _uow.Demandes.GetByIdAsync(id);
        if (demande is null) return ResultatOperation.Echec("Demande introuvable.");
        if (demande.Statut != StatutDemande.EnAttente)
            return ResultatOperation.Echec("Seules les demandes en attente peuvent être approuvées.");

        // Vérifier effectif (bloquant pour l'admin avec confirmation)
        var employe = await _uow.Employes.GetByIdAsync(demande.IdEmploye);
        if (employe is not null)
        {
            string? avert = await VerifierEffectifPosteAsync(employe.IdPoste, demande.DateDebut, demande.DateFin, demande.IdEmploye);
            if (avert is not null)
                return ResultatOperation.Echec(avert + "\nForcez l'approbation si nécessaire.");
        }

        demande.Statut           = StatutDemande.Approuve;
        demande.TraitePar        = idAdmin;
        demande.TraiteLe         = DateTime.UtcNow;
        demande.CommentaireAdmin = commentaire;
        await _uow.Demandes.UpdateAsync(demande);

        // Mettre à jour le solde
        var type = await _uow.TypesConges.GetByIdAsync(demande.IdTypeConge);
        if (type?.QuotaJours > 0)
            await _uow.Soldes.MettreAJourPrisAsync(
                demande.IdEmploye, demande.IdTypeConge, demande.DateDebut.Year, demande.NbJours);

        await _uow.SaveChangesAsync();

        // Notification
        await _notif.EnvoyerAsync(demande.IdEmploye, "Congé approuvé ✅",
            $"Votre demande du {demande.DateDebut:dd/MM/yyyy} au {demande.DateFin:dd/MM/yyyy} a été approuvée.");

        return ResultatOperation.Ok();
    }

    // ── Refuser ──────────────────────────────────────────────────────────────
    public async Task<ResultatOperation> RefuserAsync(int id, int idAdmin, string commentaire)
    {
        if (string.IsNullOrWhiteSpace(commentaire))
            return ResultatOperation.Echec("Un motif de refus est requis.");

        var demande = await _uow.Demandes.GetByIdAsync(id);
        if (demande is null) return ResultatOperation.Echec("Demande introuvable.");

        demande.Statut           = StatutDemande.Refuse;
        demande.TraitePar        = idAdmin;
        demande.TraiteLe         = DateTime.UtcNow;
        demande.CommentaireAdmin = commentaire;
        await _uow.Demandes.UpdateAsync(demande);
        await _uow.SaveChangesAsync();

        await _notif.EnvoyerAsync(demande.IdEmploye, "Congé refusé ❌",
            $"Votre demande a été refusée. Motif : {commentaire}");

        return ResultatOperation.Ok();
    }

    // ── Annuler ──────────────────────────────────────────────────────────────
    public async Task<ResultatOperation> AnnulerAsync(int id, int idUtil)
    {
        var demande = await _uow.Demandes.GetByIdAsync(id);
        if (demande is null) return ResultatOperation.Echec("Demande introuvable.");
        if (demande.Statut != StatutDemande.EnAttente)
            return ResultatOperation.Echec("Seules les demandes en attente peuvent être annulées.");

        demande.Statut = StatutDemande.Annule;
        await _uow.Demandes.UpdateAsync(demande);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    // ── Supprimer (soft) ─────────────────────────────────────────────────────
    public async Task<ResultatOperation> SupprimerAsync(int id, int idUtil)
    {
        var demande = await _uow.Demandes.GetByIdAsync(id);
        if (demande is null) return ResultatOperation.Echec("Demande introuvable.");
        if (demande.Statut == StatutDemande.Approuve)
        {
            // Rembourser le solde
            await _uow.Soldes.MettreAJourPrisAsync(
                demande.IdEmploye, demande.IdTypeConge, demande.DateDebut.Year, -demande.NbJours);
        }
        await _uow.Demandes.SoftDeleteAsync(id, idUtil);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    // ── Modifier ─────────────────────────────────────────────────────────────
    public async Task<ResultatOperation<DemandeConge>> ModifierAsync(DemandeConge demande)
    {
        var existant = await _uow.Demandes.GetByIdAsync(demande.Id);
        if (existant is null) return ResultatOperation<DemandeConge>.Echec("Demande introuvable.");
        if (!existant.EstModifiable)
            return ResultatOperation<DemandeConge>.Echec("Cette demande ne peut plus être modifiée.");

        // Chevauchement (en excluant la demande actuelle)
        bool chevauche = await _uow.Demandes.ChevauchementsExisteAsync(
            demande.IdEmploye, demande.DateDebut, demande.DateFin, demande.Id);
        if (chevauche)
            return ResultatOperation<DemandeConge>.Echec("Chevauchement avec une demande existante.");

        existant.DateDebut    = demande.DateDebut;
        existant.DateFin      = demande.DateFin;
        existant.NbJours      = CompterJoursOuvrables(demande.DateDebut, demande.DateFin);
        existant.IdTypeConge  = demande.IdTypeConge;
        existant.Motif        = demande.Motif;
        existant.CheminPreuve = demande.CheminPreuve;

        await _uow.Demandes.UpdateAsync(existant);
        await _uow.SaveChangesAsync();
        return ResultatOperation<DemandeConge>.Ok(existant);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static int CompterJoursOuvrables(DateOnly debut, DateOnly fin)
    {
        int count = 0;
        for (var d = debut; d <= fin; d = d.AddDays(1))
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                count++;
        return count;
    }

    private async Task<string?> VerifierEffectifPosteAsync(int idPoste, DateOnly debut, DateOnly fin, int idEmployeExclus)
    {
        var poste = await _uow.Postes.GetByIdAsync(idPoste);
        if (poste is null) return null;

        // Compter combien d'employés du même poste sont déjà en congé approuvé sur la période
        var demandes = await _uow.Demandes.GetToutesAvecDetailsAsync();
        int enConge = demandes.Count(d =>
            d.IdEmploye != idEmployeExclus &&
            d.Employe.IdPoste == idPoste &&
            d.Statut == StatutDemande.Approuve &&
            !(d.DateFin < debut || d.DateDebut > fin));

        int totalPoste = await _uow.Postes.GetNbEmployesActifsAsync(idPoste);
        int restants   = totalPoste - enConge - 1;

        if (restants < poste.NbMinEmployes)
            return $"Le poste '{poste.Nom}' aurait seulement {restants} employé(s) présent(s), minimum requis : {poste.NbMinEmployes}.";

        return null;
    }
}