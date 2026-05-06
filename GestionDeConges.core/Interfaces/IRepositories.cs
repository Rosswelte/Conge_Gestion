using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;
using System.Linq.Expressions;

namespace GestionDeConges.Core.Interfaces;

// ── Repository générique ──────────────────────────────────────────────────────
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

// ── Repositories spécialisés ──────────────────────────────────────────────────
public interface IEmployeRepository : IRepository<Employe>
{
    Task<IEnumerable<Employe>> GetActifsAsync();
    Task<IEnumerable<Employe>> GetSupprimesAsync();
    Task<Employe?> GetAvecPosteEtDeptAsync(int id);
    Task<IEnumerable<Employe>> RechercherAsync(string terme);
    Task SoftDeleteAsync(int id, int suppresseurId);
    Task RestaurerAsync(int id);
    Task<bool> EmailExisteAsync(string email, int? excludeId = null);
}

public interface IDemandeCongeRepository : IRepository<DemandeConge>
{
    Task<IEnumerable<DemandeConge>> GetParEmployeAsync(int idEmploye, bool inclureSupprimees = false);
    Task<IEnumerable<DemandeConge>> GetEnAttenteAsync();
    Task<IEnumerable<DemandeConge>> GetToutesAvecDetailsAsync();
    Task<bool> ChevauchementsExisteAsync(int idEmploye, DateOnly debut, DateOnly fin, int? excludeId = null);
    Task SoftDeleteAsync(int id, int suppresseurId);
    Task<IEnumerable<DemandeConge>> GetParPeriodeAsync(DateOnly debut, DateOnly fin);
    Task<Dictionary<StatutDemande, int>> GetStatistiquesStatutsAsync(int? annee = null);
    Task<IEnumerable<DemandeConge>> GetParTypeAsync(int idType, int? annee = null);
}

public interface ISoldeCongeRepository : IRepository<SoldeConge>
{
    Task<SoldeConge?> GetAsync(int idEmploye, int idType, int annee);
    Task<IEnumerable<SoldeConge>> GetParEmployeAsync(int idEmploye, int annee);
    Task<IEnumerable<SoldeConge>> GetTousParAnneeAsync(int annee);
    Task InitialiserSoldesAsync(int idEmploye, int annee);
    Task MettreAJourPrisAsync(int idEmploye, int idType, int annee, int delta);
}

public interface IPosteRepository : IRepository<Poste>
{
    Task<IEnumerable<Poste>> GetActifsAsync();
    Task<IEnumerable<Poste>> GetSupprimesAsync();
    Task SoftDeleteAsync(int id);
    Task RestaurerAsync(int id);
    Task<int> GetNbEmployesActifsAsync(int idPoste);
    Task<bool> NomExisteAsync(string nom, int idDept, int? excludeId = null);
}

public interface IUtilisateurRepository : IRepository<Utilisateur>
{
    Task<Utilisateur?> GetParNomAsync(string nomUtilisateur);
    Task<Utilisateur?> GetAvecEmployeAsync(int id);
    Task MettreAJourConnexionAsync(int id);
}

public interface ITypeCongeRepository : IRepository<TypeConge>
{
    Task<IEnumerable<TypeConge>> GetActifsAsync();
    Task<TypeConge?> GetParCodeAsync(string code);
}

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetNonLuesAsync(int idEmploye);
    Task<int> CountNonLuesAsync(int idEmploye);
    Task MarquerLuAsync(int id);
    Task MarquerToutesLuesAsync(int idEmploye);

}

public interface IHistoriquePosteRepository : IRepository<HistoriquePoste>
{
    Task<HistoriquePoste?> GetActuelAsync(int idEmploye);
    Task<IEnumerable<HistoriquePoste>> GetParEmployeAsync(int idEmploye);
    Task<IEnumerable<HistoriquePoste>> GetTousAsync();

}

// ── Unit of Work ──────────────────────────────────────────────────────────────
public interface IUnitOfWork : IDisposable
{
    IEmployeRepository       Employes       { get; }
    IDemandeCongeRepository  Demandes       { get; }
    ISoldeCongeRepository    Soldes         { get; }
    IPosteRepository         Postes         { get; }
    IUtilisateurRepository   Utilisateurs   { get; }
    ITypeCongeRepository     TypesConges    { get; }
    INotificationRepository  Notifications  { get; }
    IHistoriquePosteRepository HistoriquePostes { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}