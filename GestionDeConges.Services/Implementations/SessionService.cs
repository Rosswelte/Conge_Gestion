using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;

namespace GestionDeConges.WPF;

/// <summary>
/// Service Singleton qui conserve la session de l'utilisateur connecté
/// entre les différentes fenêtres / scopes DI.
/// 
/// POURQUOI ce service existe :
///   AuthService est Scoped (car il dépend de IUnitOfWork Scoped).
///   Mais on a besoin d'accéder à l'utilisateur connecté depuis n'importe
///   quelle fenêtre. Ce Singleton fait le pont.
/// </summary>
public class SessionService
{
    public Utilisateur? UtilisateurCourant { get; private set; }
    public bool EstConnecte => UtilisateurCourant is not null;
    public bool EstAdmin => UtilisateurCourant?.Role == RoleUtilisateur.Admin;
    public bool EstEmploye => UtilisateurCourant?.Role == RoleUtilisateur.Employe;

    /// <summary>Appelle lors d'une connexion réussie.</summary>
    public void OuvrirSession(Utilisateur utilisateur)
        => UtilisateurCourant = utilisateur;

    /// <summary>Appelle lors de la déconnexion.</summary>
    public void FermerSession()
        => UtilisateurCourant = null;

    /// <summary>Id de l'utilisateur connecté (-1 si non connecté).</summary>
    public int IdCourant => UtilisateurCourant?.Id ?? -1;

    /// <summary>Id de l'employé lié à l'utilisateur (-1 si admin pur).</summary>
    public int IdEmployeCourant => UtilisateurCourant?.IdEmploye ?? -1;
}