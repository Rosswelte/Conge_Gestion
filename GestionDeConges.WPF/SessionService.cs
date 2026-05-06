using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;

namespace GestionDeConges.WPF;

/// <summary>
/// Service Singleton de session.
/// 
/// CORRECTION : ajout de EstSessionReelle pour distinguer un vrai compte
/// admin (Id > 0) d'un accès employé sans compte (Id = -1).
/// Les opérations d'écriture admin doivent vérifier EstSessionReelle
/// avant d'agir.
/// </summary>
public class SessionService
{
    public Utilisateur? UtilisateurCourant { get; private set; }

    public bool EstConnecte => UtilisateurCourant is not null;
    public bool EstAdmin => UtilisateurCourant?.Role == RoleUtilisateur.Admin;
    public bool EstEmploye => UtilisateurCourant?.Role == RoleUtilisateur.Employe;

    /// <summary>
    /// Vrai uniquement si la session provient d'une vraie authentification
    /// (Id > 0). Faux pour le mode "Accès Employé sans mot de passe" (Id = -1).
    /// </summary>
    public bool EstSessionReelle => UtilisateurCourant?.Id > 0;

    public void OuvrirSession(Utilisateur utilisateur)
        => UtilisateurCourant = utilisateur;

    public void FermerSession()
        => UtilisateurCourant = null;

    /// <summary>
    /// Id de l'utilisateur connecté.
    /// Retourne -1 si non connecté ou si session fictive (mode employé sans compte).
    /// </summary>
    public int IdCourant => UtilisateurCourant?.Id ?? -1;

    /// <summary>
    /// Id de l'employé lié à l'utilisateur.
    /// Retourne -1 si aucun employé lié (ex : admin pur sans fiche employé).
    /// </summary>
    public int IdEmployeCourant => UtilisateurCourant?.IdEmploye ?? -1;
}
