using BCrypt.Net;
using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;
using GestionDeConges.Core.Interfaces;

namespace GestionDeConges.Services.Implementations;

public class AuthService(IUnitOfWork uow) : IAuthService
{
    private readonly IUnitOfWork _uow = uow;
    public  Utilisateur? UtilisateurCourant { get; private set; }

    public async Task<ResultatOperation<Utilisateur>> ConnecterAsync(string nomUtil, string motDePasse)
    {
        if (string.IsNullOrWhiteSpace(nomUtil) || string.IsNullOrWhiteSpace(motDePasse))
            return ResultatOperation<Utilisateur>.Echec("Identifiants requis.");

        var util = await _uow.Utilisateurs.GetParNomAsync(nomUtil.Trim());
        if (util is null)
            return ResultatOperation<Utilisateur>.Echec("Nom d'utilisateur ou mot de passe incorrect.");

        bool mdpOk;
        try
        {
            // Essaie BCrypt d'abord, retombe sur comparaison directe (données test)
            mdpOk = BCrypt.Net.BCrypt.Verify(motDePasse, util.MotDePasse);
        }
        catch
        {
            mdpOk = util.MotDePasse == motDePasse; // fallback pour mots de passe en clair (dev)
        }

        if (!mdpOk)
            return ResultatOperation<Utilisateur>.Echec("Nom d'utilisateur ou mot de passe incorrect.");

        await _uow.Utilisateurs.MettreAJourConnexionAsync(util.Id);
        await _uow.SaveChangesAsync();

        UtilisateurCourant = util;
        return ResultatOperation<Utilisateur>.Ok(util);
    }

    public async Task<ResultatOperation> CreerAdminAsync(string nomUtil, string motDePasse)
    {
        if (string.IsNullOrWhiteSpace(nomUtil) || motDePasse.Length < 6)
            return ResultatOperation.Echec("Nom requis et mot de passe ≥ 6 caractères.");

        var existant = await _uow.Utilisateurs.GetParNomAsync(nomUtil.Trim());
        if (existant is not null)
            return ResultatOperation.Echec("Ce nom d'utilisateur est déjà pris.");

        string hash = BCrypt.Net.BCrypt.HashPassword(motDePasse, workFactor: 11);
        var admin = new Utilisateur
        {
            NomUtilisateur = nomUtil.Trim(),
            MotDePasse     = hash,
            Role           = RoleUtilisateur.Admin
        };
        await _uow.Utilisateurs.AddAsync(admin);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    public async Task<ResultatOperation> ChangerMotDePasseAsync(int idUtil, string ancien, string nouveau)
    {
        var util = await _uow.Utilisateurs.GetByIdAsync(idUtil);
        if (util is null) return ResultatOperation.Echec("Utilisateur introuvable.");

        if (!BCrypt.Net.BCrypt.Verify(ancien, util.MotDePasse))
            return ResultatOperation.Echec("Ancien mot de passe incorrect.");

        if (nouveau.Length < 6)
            return ResultatOperation.Echec("Le nouveau mot de passe doit contenir au moins 6 caractères.");

        util.MotDePasse = BCrypt.Net.BCrypt.HashPassword(nouveau, 11);
        await _uow.Utilisateurs.UpdateAsync(util);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    public void Deconnecter() => UtilisateurCourant = null;
}