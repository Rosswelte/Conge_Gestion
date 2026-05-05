using GestionDeConges.Core.Enums;

namespace GestionDeConges.Core.Entities;

// ── Département ──────────────────────────────────────────────────────────────
public class Departement
{
    public int    Id          { get; set; }
    public string Nom         { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool   EstActif    { get; set; } = true;
    public DateTime CreeLe    { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Poste> Postes { get; set; } = [];
}

// ── Poste ────────────────────────────────────────────────────────────────────
public class Poste : BaseEntity
{
    public string Nom             { get; set; } = string.Empty;
    public int    IdDepartement   { get; set; }
    public int    NbMinEmployes   { get; set; } = 1;
    public bool   EstActif        { get; set; } = true;

    // Navigation
    public Departement          Departement { get; set; } = null!;
    public ICollection<Employe> Employes    { get; set; } = [];
}

// ── Employé ──────────────────────────────────────────────────────────────────
public class Employe : BaseEntity
{
    public string   Nom            { get; set; } = string.Empty;
    public string   Prenom         { get; set; } = string.Empty;
    public string   Email          { get; set; } = string.Empty;
    public string?  Telephone      { get; set; }
    public DateOnly? DateNaissance { get; set; }
    public Sexe     Sexe           { get; set; } = Sexe.M;
    public int      IdPoste        { get; set; }
    public DateOnly DateEmbauche   { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public bool     EstActif       { get; set; } = true;
    public int?     SupprimePar    { get; set; }

    // Navigation
    public Poste               Poste          { get; set; } = null!;
    public Utilisateur?        Utilisateur    { get; set; }
    public ICollection<DemandeConge>  Demandes { get; set; } = [];
    public ICollection<SoldeConge>    Soldes   { get; set; } = [];
    public ICollection<Notification>  Notifications { get; set; } = [];

    // Propriété calculée
    public string NomComplet => $"{Nom} {Prenom}";
    public int Anciennete => DateTime.Today.Year - DateEmbauche.Year;
}

// ── Utilisateur ──────────────────────────────────────────────────────────────
public class Utilisateur
{
    public int              Id                  { get; set; }
    public string           NomUtilisateur      { get; set; } = string.Empty;
    public string           MotDePasse          { get; set; } = string.Empty; // bcrypt hash
    public RoleUtilisateur  Role                { get; set; } = RoleUtilisateur.Employe;
    public int?             IdEmploye           { get; set; }
    public bool             EstActif            { get; set; } = true;
    public DateTime?        DerniereConnexion   { get; set; }
    public DateTime         CreeLe              { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employe?                  Employe        { get; set; }
    public ICollection<DemandeConge> DemandesTraitees { get; set; } = [];
}

// ── Type de congé ─────────────────────────────────────────────────────────────
public class TypeConge
{
    public int            Id              { get; set; }
    public string         Code            { get; set; } = string.Empty;
    public string         Libelle         { get; set; } = string.Empty;
    public int            QuotaJours      { get; set; }
    public bool           EstPaye         { get; set; } = true;
    public bool           NecessitePreuve { get; set; } = false;
    public SexeRequisConge SexeRequis     { get; set; } = SexeRequisConge.Tous;
    public bool           EstActif        { get; set; } = true;

    // Navigation
    public ICollection<DemandeConge> Demandes { get; set; } = [];
    public ICollection<SoldeConge>   Soldes   { get; set; } = [];
}

// ── Solde congé ───────────────────────────────────────────────────────────────
public class SoldeConge
{
    public int    Id           { get; set; }
    public int    IdEmploye    { get; set; }
    public int    IdTypeConge  { get; set; }
    public int    Annee        { get; set; }
    public int    Quota        { get; set; }
    public int    Pris         { get; set; }
    public int    Reporte      { get; set; } = 0;
    public DateTime ModifieLe  { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employe   Employe   { get; set; } = null!;
    public TypeConge TypeConge { get; set; } = null!;

    // Propriété calculée
    public int Restant => Quota + Reporte - Pris;
}

// ── Demande de congé ──────────────────────────────────────────────────────────
public class DemandeConge : BaseEntity
{
    public int            IdEmploye       { get; set; }
    public int            IdTypeConge     { get; set; }
    public DateOnly       DateDebut       { get; set; }
    public DateOnly       DateFin         { get; set; }
    public int            NbJours         { get; set; }
    public string?        Motif           { get; set; }
    public string?        CheminPreuve    { get; set; }
    public StatutDemande  Statut          { get; set; } = StatutDemande.EnAttente;
    public int?           TraitePar       { get; set; }
    public DateTime?      TraiteLe        { get; set; }
    public string?        CommentaireAdmin { get; set; }
    public int?           SupprimePar     { get; set; }

    // Navigation
    public Employe      Employe    { get; set; } = null!;
    public TypeConge    TypeConge  { get; set; } = null!;
    public Utilisateur? Admin      { get; set; }

    // Propriétés calculées
    public bool EstModifiable  => Statut == StatutDemande.EnAttente;
    public bool EstEnAttente   => Statut == StatutDemande.EnAttente;
}

// ── Historique / Audit ────────────────────────────────────────────────────────
public class HistoriqueAction
{
    public int        Id          { get; set; }
    public string     TableCible  { get; set; } = string.Empty;
    public int        IdEnreg     { get; set; }
    public TypeAction Action      { get; set; }
    public string?    Details     { get; set; }
    public int?       IdUtil      { get; set; }
    public DateTime   RealiseLe   { get; set; } = DateTime.UtcNow;

    // Navigation
    public Utilisateur? Utilisateur { get; set; }
}

// ── Notification ──────────────────────────────────────────────────────────────
public class Notification
{
    public int      Id         { get; set; }
    public int      IdEmploye  { get; set; }
    public string   Titre      { get; set; } = string.Empty;
    public string   Message    { get; set; } = string.Empty;
    public bool     EstLu      { get; set; } = false;
    public DateTime? LuLe      { get; set; }
    public DateTime CreeLe     { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employe Employe { get; set; } = null!;
}