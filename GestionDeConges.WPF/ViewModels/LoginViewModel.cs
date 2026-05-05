using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionDeConges.Core.Enums;
using GestionDeConges.Core.Interfaces;
using GestionDeConges.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GestionDeConges.WPF.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly SessionService _session;

    public LoginViewModel(IAuthService authService, SessionService session)
    {
        _authService = authService;
        _session = session;
    }

    [ObservableProperty] private string _nomUtilisateur = string.Empty;
    [ObservableProperty] private string _erreurMessage = string.Empty;

    /// <summary>
    /// Commande de connexion.
    /// Le mot de passe est passé en paramètre depuis le code-behind
    /// (PasswordBox ne supporte pas le binding direct par sécurité).
    /// </summary>
    [RelayCommand]
    private async Task ConnecterAsync(string? motDePasse)
    {
        ErreurMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(NomUtilisateur) || string.IsNullOrWhiteSpace(motDePasse))
        {
            ErreurMessage = "Veuillez remplir tous les champs.";
            return;
        }

        await RunSafeAsync(async () =>
        {
            var resultat = await _authService.ConnecterAsync(NomUtilisateur.Trim(), motDePasse);

            if (!resultat.Succes || resultat.Donnee is null)
            {
                ErreurMessage = resultat.Erreur ?? "Identifiants incorrects.";
                return;
            }

            // Stocker dans le Singleton de session
            _session.OuvrirSession(resultat.Donnee);

            // Ouvrir la bonne fenêtre selon le rôle
            if (resultat.Donnee.Role == RoleUtilisateur.Admin)
            {
                var mainWindow = App.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            else
            {
                var employeWindow = App.Services.GetRequiredService<EmployeSpaceView>();
                employeWindow.Show();
            }

        }, "Erreur de connexion");
    }
}