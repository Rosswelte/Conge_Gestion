using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;

namespace GestionDeConges.Core.Interfaces;

// ── Résultat standard d'une opération ────────────────────────────────────────
public record ResultatOperation<T>(bool Succes, T? Donnee, string? Erreur = null)
{
    public static ResultatOperation<T> Ok(T donnee)    => new(true,  donnee);
    public static ResultatOperation<T> Echec(string e) => new(false, default, e);
}
public record ResultatOperation(bool Succes, string? Erreur = null)
{
    public static ResultatOperation Ok()              => new(true);
    public static ResultatOperation Echec(string e)   => new(false, e);
}

// ── Service d'authentification ────────────────────────────────────────────────
public interface IAuthService
{
    Task<ResultatOperation<Utilisateur>> ConnecterAsync(string nomUtil, string motDePasse);
    Task<ResultatOperation> CreerAdminAsync(string nomUtil, string motDePasse);
    Task<ResultatOperation> ChangerMotDePasseAsync(int idUtil, string ancien, string nouveau);
    Utilisateur? UtilisateurCourant { get; }
    void Deconnecter();
}

// ── Service employés ──────────────────────────────────────────────────────────
public interface IEmployeService
{
    Task<IEnumerable<Employe>> GetActifsAsync();
    Task<IEnumerable<Employe>> GetSupprimesAsync();
    Task<Employe?> GetParIdAsync(int id);
    Task<IEnumerable<Employe>> RechercherAsync(string terme);
    Task<ResultatOperation<Employe>> CreerAsync(Employe employe);
    Task<ResultatOperation<Employe>> ModifierAsync(Employe employe);
    Task<ResultatOperation> SupprimerAsync(int id);
    Task<ResultatOperation> SupprimerAsync(int id, int suppresseurId); // ← AJOUTER
    Task<ResultatOperation> RestaurerAsync(int id);
}

// ── Service demandes de congé ─────────────────────────────────────────────────
public interface IDemandeCongeService
{
    Task<IEnumerable<DemandeConge>> GetToutesAsync();
    Task<IEnumerable<DemandeConge>> GetParEmployeAsync(int idEmploye);
    Task<IEnumerable<DemandeConge>> GetEnAttenteAsync();
    Task<ResultatOperation<DemandeConge>> SoumettreAsync(DemandeConge demande);
    Task<ResultatOperation> ApprouverAsync(int id, int idAdmin, string? commentaire = null);
    Task<ResultatOperation> RefuserAsync(int id, int idAdmin, string commentaire);
    Task<ResultatOperation> AnnulerAsync(int id, int idUtil);
    Task<ResultatOperation> SupprimerAsync(int id, int idUtil);
    Task<ResultatOperation<DemandeConge>> ModifierAsync(DemandeConge demande);
}

// ── Service soldes ────────────────────────────────────────────────────────────
public interface ISoldeCongeService
{
    Task<IEnumerable<SoldeConge>> GetParEmployeAsync(int idEmploye, int annee);
    Task<SoldeConge?> GetAsync(int idEmploye, int idType, int annee);
    Task<int> GetSoldeRestantAsync(int idEmploye, int idType, int annee);
    Task InitialiserSoldesNouvelAnAsync(int annee);
}

// ── Service postes ────────────────────────────────────────────────────────────
public interface IPosteService
{
    Task<IEnumerable<Poste>> GetActifsAsync();
    Task<IEnumerable<Poste>> GetSupprimesAsync();
    Task<ResultatOperation<Poste>> CreerAsync(Poste poste);
    Task<ResultatOperation<Poste>> ModifierAsync(Poste poste);
    Task<ResultatOperation> SupprimerAsync(int id);
    Task<ResultatOperation> RestaurerAsync(int id);
}

// ── Service rapports / exports ────────────────────────────────────────────────
public interface IRapportService
{
    Task<byte[]> ExporterCongesExcelAsync(int? annee = null);
    Task<byte[]> ExporterCongesPdfAsync(int? annee = null);
    Task<byte[]> ExporterSoldesExcelAsync(int annee);
    Task<StatistiquesGlobales> GetStatistiquesAsync(int annee);
}

// ── Service notifications ─────────────────────────────────────────────────────
public interface INotificationService
{
    Task<IEnumerable<Notification>> GetNonLuesAsync(int idEmploye);
    Task<int> CountNonLuesAsync(int idEmploye);
    Task MarquerLueAsync(int id);
    Task MarquerToutesLuesAsync(int idEmploye);
    Task EnvoyerAsync(int idEmploye, string titre, string message);
}

// ── DTO Statistiques ──────────────────────────────────────────────────────────
public class StatistiquesGlobales
{
    public int Annee                  { get; set; }
    public int TotalDemandes          { get; set; }
    public int EnAttente              { get; set; }
    public int Approuvees             { get; set; }
    public int Refusees               { get; set; }
    public int TotalEmployes          { get; set; }
    public int EmployesEnConge        { get; set; }
    public Dictionary<string, int> DemandesParType  { get; set; } = [];
    public Dictionary<string, int> DemandesParMois  { get; set; } = [];
    public List<TopEmployeConge>   TopConsommateurs { get; set; } = [];
}

public class TopEmployeConge
{
    public string Employe    { get; set; } = string.Empty;
    public int    JoursPris  { get; set; }
    public string TypeConge  { get; set; } = string.Empty;
}