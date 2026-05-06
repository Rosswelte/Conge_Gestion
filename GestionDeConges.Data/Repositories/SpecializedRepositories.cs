using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;
using GestionDeConges.Core.Interfaces;
using GestionDeConges.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConges.Data.Repositories;

// ── Employé ──────────────────────────────────────────────────────────────────
public class EmployeRepository(AppDbContext ctx)
    : Repository<Employe>(ctx), IEmployeRepository
{
    public async Task<IEnumerable<Employe>> GetActifsAsync()
        => await _ctx.Employes
            .Include(e => e.Poste).ThenInclude(p => p.Departement)
            .Where(e => !e.EstSupprime && e.EstActif)
            .OrderBy(e => e.Nom).ThenBy(e => e.Prenom)
            .ToListAsync();

    public async Task<IEnumerable<Employe>> GetSupprimesAsync()
        => await _ctx.Employes
            .IgnoreQueryFilters()
            .Include(e => e.Poste)
            .Where(e => e.EstSupprime)
            .OrderBy(e => e.SupprimeLe)
            .ToListAsync();

    public async Task<Employe?> GetAvecPosteEtDeptAsync(int id)
        => await _ctx.Employes
            .Include(e => e.Poste).ThenInclude(p => p.Departement)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<Employe>> RechercherAsync(string terme)
    {
        terme = terme.ToLower();
        return await _ctx.Employes
            .Include(e => e.Poste)
            .Where(e => !e.EstSupprime &&
                (e.Nom.ToLower().Contains(terme) ||
                 e.Prenom.ToLower().Contains(terme) ||
                 e.Email.ToLower().Contains(terme)))
            .ToListAsync();
    }

    public async Task SoftDeleteAsync(int id, int suppresseurId)
    {
        var e = await _ctx.Employes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Employé {id} introuvable.");
        e.EstSupprime  = true;
        e.SupprimeLe   = DateTime.UtcNow;
        e.SupprimePar  = suppresseurId;
        e.EstActif     = false;
    }

    public async Task RestaurerAsync(int id)
    {
        var e = await _ctx.Employes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Employé {id} introuvable.");
        e.EstSupprime = false;
        e.SupprimeLe  = null;
        e.EstActif    = true;
    }

    public async Task<bool> EmailExisteAsync(string email, int? excludeId = null)
        => await _ctx.Employes.IgnoreQueryFilters()
            .AnyAsync(e => e.Email == email && (excludeId == null || e.Id != excludeId));
}

// ── DemandeConge ─────────────────────────────────────────────────────────────
public class DemandeCongeRepository(AppDbContext ctx)
    : Repository<DemandeConge>(ctx), IDemandeCongeRepository
{
    private IQueryable<DemandeConge> Detailed()
        => _ctx.DemandesConges
            .Include(d => d.Employe).ThenInclude(e => e.Poste).ThenInclude(p => p.Departement)
            .Include(d => d.TypeConge)
            .Include(d => d.Admin);

    public async Task<IEnumerable<DemandeConge>> GetParEmployeAsync(int idEmploye, bool inclureSupprimees = false)
    {
        var q = inclureSupprimees
            ? _ctx.DemandesConges.IgnoreQueryFilters()
            : _ctx.DemandesConges.AsQueryable();
        return await q.Include(d => d.TypeConge)
            .Where(d => d.IdEmploye == idEmploye)
            .OrderByDescending(d => d.CreeLe)
            .ToListAsync();
    }

    public async Task<IEnumerable<DemandeConge>> GetEnAttenteAsync()
        => await Detailed()
            .Where(d => d.Statut == StatutDemande.EnAttente)
            .OrderBy(d => d.CreeLe)
            .ToListAsync();

    public async Task<IEnumerable<DemandeConge>> GetToutesAvecDetailsAsync()
        => await Detailed()
            .OrderByDescending(d => d.CreeLe)
            .ToListAsync();

    public async Task<bool> ChevauchementsExisteAsync(int idEmploye, DateOnly debut, DateOnly fin, int? excludeId = null)
        => await _ctx.DemandesConges
            .Where(d => d.IdEmploye == idEmploye
                && d.Statut != StatutDemande.Refuse
                && d.Statut != StatutDemande.Annule
                && (excludeId == null || d.Id != excludeId)
                && !(d.DateFin < debut || d.DateDebut > fin))
            .AnyAsync();

    public async Task SoftDeleteAsync(int id, int suppresseurId)
    {
        var d = await _ctx.DemandesConges.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException();
        d.EstSupprime  = true;
        d.SupprimeLe   = DateTime.UtcNow;
        d.SupprimePar  = suppresseurId;
    }

    public async Task<IEnumerable<DemandeConge>> GetParPeriodeAsync(DateOnly debut, DateOnly fin)
        => await Detailed()
            .Where(d => d.DateDebut >= debut && d.DateFin <= fin)
            .ToListAsync();

    public async Task<Dictionary<StatutDemande, int>> GetStatistiquesStatutsAsync(int? annee = null)
    {
        var q = _ctx.DemandesConges.AsQueryable();
        if (annee.HasValue)
            q = q.Where(d => d.DateDebut.Year == annee.Value);
        return await q.GroupBy(d => d.Statut)
            .Select(g => new { Statut = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Statut, x => x.Count);
    }

    public async Task<IEnumerable<DemandeConge>> GetParTypeAsync(int idType, int? annee = null)
    {
        var q = _ctx.DemandesConges.Include(d => d.Employe).Where(d => d.IdTypeConge == idType);
        if (annee.HasValue) q = q.Where(d => d.DateDebut.Year == annee.Value);
        return await q.ToListAsync();
    }
}

// ── SoldeConge ───────────────────────────────────────────────────────────────
public class SoldeCongeRepository(AppDbContext ctx)
    : Repository<SoldeConge>(ctx), ISoldeCongeRepository
{
    public async Task<SoldeConge?> GetAsync(int idEmploye, int idType, int annee)
        => await _ctx.SoldesConges
            .Include(s => s.TypeConge)
            .FirstOrDefaultAsync(s => s.IdEmploye == idEmploye
                && s.IdTypeConge == idType && s.Annee == annee);

    public async Task<IEnumerable<SoldeConge>> GetParEmployeAsync(int idEmploye, int annee)
        => await _ctx.SoldesConges
            .Include(s => s.TypeConge)
            .Where(s => s.IdEmploye == idEmploye && s.Annee == annee)
            .ToListAsync();

    public async Task<IEnumerable<SoldeConge>> GetTousParAnneeAsync(int annee)
        => await _ctx.SoldesConges
            .Include(s => s.Employe)
            .Include(s => s.TypeConge)
            .Where(s => s.Annee == annee)
            .ToListAsync();

    public async Task InitialiserSoldesAsync(int idEmploye, int annee)
    {
        var types = await _ctx.TypesConges.Where(t => t.EstActif && t.QuotaJours > 0).ToListAsync();
        foreach (var type in types)
        {
            var existe = await _ctx.SoldesConges.AnyAsync(
                s => s.IdEmploye == idEmploye && s.IdTypeConge == type.Id && s.Annee == annee);
            if (!existe)
                await _ctx.SoldesConges.AddAsync(new SoldeConge
                {
                    IdEmploye   = idEmploye,
                    IdTypeConge = type.Id,
                    Annee       = annee,
                    Quota       = type.QuotaJours
                });
        }
    }

    public async Task MettreAJourPrisAsync(int idEmploye, int idType, int annee, int delta)
    {
        var solde = await GetAsync(idEmploye, idType, annee);
        if (solde is not null) solde.Pris += delta;
    }
}

// ── Poste ────────────────────────────────────────────────────────────────────
public class PosteRepository(AppDbContext ctx)
    : Repository<Poste>(ctx), IPosteRepository
{
    public async Task<IEnumerable<Poste>> GetActifsAsync()
        => await _ctx.Postes
            .Include(p => p.Departement)
            .Where(p => !p.EstSupprime && p.EstActif)
            .OrderBy(p => p.Nom)
            .ToListAsync();

    public async Task<IEnumerable<Poste>> GetSupprimesAsync()
        => await _ctx.Postes.IgnoreQueryFilters()
            .Include(p => p.Departement)
            .Where(p => p.EstSupprime)
            .ToListAsync();

    public async Task SoftDeleteAsync(int id)
    {
        var p = await _ctx.Postes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException();
        p.EstSupprime = true;
        p.SupprimeLe  = DateTime.UtcNow;
    }

    public async Task RestaurerAsync(int id)
    {
        var p = await _ctx.Postes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException();
        p.EstSupprime = false;
        p.SupprimeLe  = null;
    }

    public async Task<int> GetNbEmployesActifsAsync(int idPoste)
        => await _ctx.Employes.CountAsync(e => e.IdPoste == idPoste && !e.EstSupprime && e.EstActif);

    public async Task<bool> NomExisteAsync(string nom, int idDept, int? excludeId = null)
        => await _ctx.Postes.IgnoreQueryFilters()
            .AnyAsync(p => p.Nom == nom && p.IdDepartement == idDept
                && (excludeId == null || p.Id != excludeId));
}

// ── Utilisateur ──────────────────────────────────────────────────────────────
public class UtilisateurRepository(AppDbContext ctx)
    : Repository<Utilisateur>(ctx), IUtilisateurRepository
{
    public async Task<Utilisateur?> GetParNomAsync(string nomUtilisateur)
        => await _ctx.Utilisateurs
            .Include(u => u.Employe).ThenInclude(e => e!.Poste)
            .FirstOrDefaultAsync(u => u.NomUtilisateur == nomUtilisateur && u.EstActif);

    public async Task<Utilisateur?> GetAvecEmployeAsync(int id)
        => await _ctx.Utilisateurs
            .Include(u => u.Employe)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task MettreAJourConnexionAsync(int id)
    {
        var u = await _ctx.Utilisateurs.FindAsync(id);
        if (u is not null) u.DerniereConnexion = DateTime.UtcNow;
    }
}

// ── TypeConge ────────────────────────────────────────────────────────────────
public class TypeCongeRepository(AppDbContext ctx)
    : Repository<TypeConge>(ctx), ITypeCongeRepository
{
    public async Task<IEnumerable<TypeConge>> GetActifsAsync()
        => await _ctx.TypesConges.Where(t => t.EstActif).OrderBy(t => t.Libelle).ToListAsync();

    public async Task<TypeConge?> GetParCodeAsync(string code)
        => await _ctx.TypesConges.FirstOrDefaultAsync(t => t.Code == code);
}

// ── Notification ─────────────────────────────────────────────────────────────
public class NotificationRepository(AppDbContext ctx)
    : Repository<Notification>(ctx), INotificationRepository
{
    public async Task<IEnumerable<Notification>> GetNonLuesAsync(int idEmploye)
        => await _ctx.Notifications
            .Where(n => n.IdEmploye == idEmploye && !n.EstLu)
            .OrderByDescending(n => n.CreeLe)
            .ToListAsync();

    public async Task<int> CountNonLuesAsync(int idEmploye)
        => await _ctx.Notifications.CountAsync(n => n.IdEmploye == idEmploye && !n.EstLu);

    public async Task MarquerLuAsync(int id)
    {
        var n = await _ctx.Notifications.FindAsync(id);
        if (n is not null) { n.EstLu = true; n.LuLe = DateTime.UtcNow; }
    }

    public async Task MarquerToutesLuesAsync(int idEmploye)
    {
        var notifs = await _ctx.Notifications
            .Where(n => n.IdEmploye == idEmploye && !n.EstLu).ToListAsync();
        foreach (var n in notifs) { n.EstLu = true; n.LuLe = DateTime.UtcNow; }
    }
   
}
// ── HistoriquePoste ──────────────────────────────────────────────────────────
public class HistoriquePosteRepository(AppDbContext ctx)
    : Repository<HistoriquePoste>(ctx), IHistoriquePosteRepository
{
    public async Task<HistoriquePoste?> GetActuelAsync(int idEmploye)
        => await _ctx.HistoriquePostes
            .Include(h => h.Poste)
            .FirstOrDefaultAsync(h => h.IdEmploye == idEmploye && h.EstActuel);

    public async Task<IEnumerable<HistoriquePoste>> GetParEmployeAsync(int idEmploye)
        => await _ctx.HistoriquePostes
            .Include(h => h.Poste)
            .Where(h => h.IdEmploye == idEmploye)
            .OrderByDescending(h => h.DateDebut)
            .ToListAsync();
    public async Task<IEnumerable<HistoriquePoste>> GetTousAsync()
    => await _ctx.HistoriquePostes
        .Include(h => h.Employe)
        .Include(h => h.Poste)
        .OrderByDescending(h => h.DateDebut)
        .ToListAsync();
}