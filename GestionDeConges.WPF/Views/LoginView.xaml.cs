using GestionDeConges.Core.Enums;
using GestionDeConges.Core.Interfaces;
using GestionDeConges.WPF.ViewModels;
using GestionDeConges.WPF.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GestionDeConges.WPF.Views;

public partial class LoginView : Window
{
    private readonly LoginViewModel _vm;
    private readonly SessionService _session;

    public LoginView(LoginViewModel vm, SessionService session)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        DataContext = _vm;
    }

    // ── Connexion Admin ───────────────────────────────────────────────────────
    private async void BtnAdmin_Click(object sender, RoutedEventArgs e)
    {
        // Passe le mot de passe manuellement (PasswordBox non bindable)
        await _vm.ConnecterCommand.ExecuteAsync(TxtPassword.Password);

        // Si connexion réussie → fermer LoginView
        if (_session.EstConnecte)
            this.Close();
    }

    // ── Mode Employé (sans mot de passe) ─────────────────────────────────────
    private void BtnEmploye_Click(object sender, RoutedEventArgs e)
    {
        // Ouvre un dialog de sélection d'employé
        var dialog = App.Services.GetRequiredService<ChoixEmployeDialog>();
        bool? result = dialog.ShowDialog();

        if (result == true && dialog.EmployeSelectionne is not null)
        {
            // Créer un utilisateur fictif "employé" pour la session
            var fakeUtil = new Core.Entities.Utilisateur
            {
                Id = -1,
                NomUtilisateur = dialog.EmployeSelectionne.NomComplet,
                Role = RoleUtilisateur.Employe,
                IdEmploye = dialog.EmployeSelectionne.Id,
                Employe = dialog.EmployeSelectionne
            };
            _session.OuvrirSession(fakeUtil);

            var employeWindow = App.Services.GetRequiredService<EmployeSpaceView>();
            employeWindow.Show();
            this.Close();
        }
    }

    private void BtnCreerAdmin_Click(object sender, RoutedEventArgs e)
    {
        var authService = App.Services.GetRequiredService<IAuthService>();
        var dialog = new RegisterAdminDialog(authService)
        {
            Owner = this
        };
        dialog.ShowDialog();
    }
}