using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Interfaces;

namespace GestionDeConges.Services.Implementations;

/// <summary>
/// CORRECTION : suppression de la dépendance vers GestionDeConges.WPF.
/// Le suppresseurId est maintenant passé directement par le ViewModel
/// qui possède la session.
/// </summary>
public class EmployeService(IUnitOfWork uow) : IEmployeService
{
    private readonly IUnitOfWork _uow = uow;

    public Task<IEnumerable<Employe>> GetActifsAsync()
        => _uow.Employes.GetActifsAsync();

    public Task<IEnumerable<Employe>> GetSupprimesAsync()
        => _uow.Employes.GetSupprimesAsync();

    public Task<Employe?> GetParIdAsync(int id)
        => _uow.Employes.GetAvecPosteEtDeptAsync(id);

    public Task<IEnumerable<Employe>> RechercherAsync(string terme)
        => _uow.Employes.RechercherAsync(terme);

    public async Task<IEnumerable<HistoriquePoste>> GetTousHistoriquePostesAsync()
    => await _uow.HistoriquePostes.GetTousAsync();

    public async Task<ResultatOperation<Employe>> CreerAsync(Employe employe)
    {
        if (string.IsNullOrWhiteSpace(employe.Nom) || string.IsNullOrWhiteSpace(employe.Prenom))
            return ResultatOperation<Employe>.Echec("Nom et prénom requis.");
        if (string.IsNullOrWhiteSpace(employe.Email))
            return ResultatOperation<Employe>.Echec("Email requis.");
        if (await _uow.Employes.EmailExisteAsync(employe.Email))
            return ResultatOperation<Employe>.Echec("Cet email est déjà utilisé.");

        await _uow.Employes.AddAsync(employe);
        await _uow.SaveChangesAsync();
        await _uow.Soldes.InitialiserSoldesAsync(employe.Id, DateTime.Now.Year);
        await _uow.SaveChangesAsync();

        return ResultatOperation<Employe>.Ok(employe);
    }

    public async Task<ResultatOperation<Employe>> ModifierAsync(Employe employe)
    {
        var existant = await _uow.Employes.GetByIdAsync(employe.Id);
        if (existant is null)
            return ResultatOperation<Employe>.Echec("Employé introuvable.");
        if (await _uow.Employes.EmailExisteAsync(employe.Email, employe.Id))
            return ResultatOperation<Employe>.Echec("Cet email est déjà utilisé par un autre employé.");

        // ✅ Si le poste change, enregistrer dans l'historique
        if (existant.IdPoste != employe.IdPoste)
        {
            // Clôturer l'ancien historique
            var historiqueActuel = await _uow.HistoriquePostes.GetActuelAsync(employe.Id);
            if (historiqueActuel is not null)
            {
                historiqueActuel.DateFin = DateOnly.FromDateTime(DateTime.Today);
                historiqueActuel.EstActuel = false;
                await _uow.HistoriquePostes.UpdateAsync(historiqueActuel);
            }

            // Créer le nouveau
            await _uow.HistoriquePostes.AddAsync(new HistoriquePoste
            {
                IdEmploye = employe.Id,
                IdPoste = employe.IdPoste,
                DateDebut = DateOnly.FromDateTime(DateTime.Today),
                EstActuel = true
            });
        }

        existant.Nom = employe.Nom;
        existant.Prenom = employe.Prenom;
        existant.Email = employe.Email;
        existant.Telephone = employe.Telephone;
        existant.DateNaissance = employe.DateNaissance;
        existant.Sexe = employe.Sexe;
        existant.IdPoste = employe.IdPoste;
        existant.DateEmbauche = employe.DateEmbauche;

        await _uow.Employes.UpdateAsync(existant);
        await _uow.SaveChangesAsync();
        return ResultatOperation<Employe>.Ok(existant);
    }

    public async Task<ResultatOperation> SupprimerAsync(int id)
    {
        var employe = await _uow.Employes.GetByIdAsync(id);
        if (employe is null)
            return ResultatOperation.Echec("Employé introuvable.");

        // suppresseurId = 0 ici car le service n'a pas accès à la session.
        // Le ViewModel (qui a la session) peut appeler SupprimerAvecAuteurAsync
        // pour la traçabilité complète.
        await _uow.Employes.SoftDeleteAsync(id, 0);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    /// <summary>
    /// Surcharge avec l'id de l'auteur de la suppression pour la traçabilité.
    /// Appelée par EmployesViewModel qui possède la session.
    /// </summary>
    public async Task<ResultatOperation> SupprimerAsync(int id, int suppresseurId)
    {
        var employe = await _uow.Employes.GetByIdAsync(id);
        if (employe is null)
            return ResultatOperation.Echec("Employé introuvable.");

        await _uow.Employes.SoftDeleteAsync(id, suppresseurId);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    public async Task<ResultatOperation> RestaurerAsync(int id)
    {
        await _uow.Employes.RestaurerAsync(id);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    /// <summary>
    /// Change le poste d'un employé et enregistre l'historique.
    /// </summary>
    public async Task<ResultatOperation> ChangerPosteAsync(int idEmploye, int idNouveauPoste, DateOnly dateDebut)
    {
        var employe = await _uow.Employes.GetByIdAsync(idEmploye);
        if (employe is null)
            return ResultatOperation.Echec("Employé introuvable.");

        var nouveauPoste = await _uow.Postes.GetByIdAsync(idNouveauPoste);
        if (nouveauPoste is null)
            return ResultatOperation.Echec("Poste introuvable.");

        // Clôturer le poste actuel
        var historiqueActuel = await _uow.HistoriquePostes.GetActuelAsync(idEmploye);
        if (historiqueActuel is not null)
        {
            historiqueActuel.DateFin = dateDebut.AddDays(-1);
            historiqueActuel.EstActuel = false;
            await _uow.HistoriquePostes.UpdateAsync(historiqueActuel);
        }

        // Créer le nouvel historique
        var nouveau = new HistoriquePoste
        {
            IdEmploye = idEmploye,
            IdPoste = idNouveauPoste,
            DateDebut = dateDebut,
            EstActuel = true
        };
        await _uow.HistoriquePostes.AddAsync(nouveau);

        // Mettre à jour le poste de l'employé
        employe.IdPoste = idNouveauPoste;
        await _uow.Employes.UpdateAsync(employe);

        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    /// <summary>
    /// Récupère l'historique des postes d'un employé.
    /// </summary>
    public async Task<IEnumerable<HistoriquePoste>> GetHistoriquePostesAsync(int idEmploye)
        => await _uow.HistoriquePostes.GetParEmployeAsync(idEmploye);
}