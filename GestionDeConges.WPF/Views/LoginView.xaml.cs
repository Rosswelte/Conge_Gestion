<<<<<<< HEAD
﻿using GestionDeConges.Core.Enums;
using GestionDeConges.Core.Interfaces;
using GestionDeConges.WPF.ViewModels;
using GestionDeConges.WPF.Views.Dialogs;
=======
﻿using GestionDeConges.Core.Interfaces;
>>>>>>> 9510d52d7eb1706487be963abce4edd11b18fe49
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GestionDeConges.WPF.Views;

public partial class LoginView : Window
{
    private readonly LoginViewModel _vm;
    private readonly SessionService _session;

<<<<<<< HEAD
    public LoginView(LoginViewModel vm, SessionService session)
=======
    // Constructeur pour Dependency Injection
    public LoginView(IAuthService authService)
>>>>>>> 9510d52d7eb1706487be963abce4edd11b18fe49
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        DataContext = _vm;
    }

<<<<<<< HEAD
    // ── Connexion Admin ───────────────────────────────────────────────────────
    private async void BtnAdmin_Click(object sender, RoutedEventArgs e)
    {
        // Passe le mot de passe manuellement (PasswordBox non bindable)
        await _vm.ConnecterCommand.ExecuteAsync(TxtPassword.Password);
=======
    // Constructeur par défaut (obligatoire pour XAML)
    public LoginView()
    {
        InitializeComponent();
        // Pour éviter les erreurs si DI ne marche pas
        _authService = App.Services.GetService<IAuthService>()!;
    }

    // Constructeur par défaut (obligatoire pour XAML)
    public LoginView()
    {
        InitializeComponent();
        // Pour éviter les erreurs si DI ne marche pas
        _authService = App.Services.GetService<IAuthService>()!;
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string username = txtUsername.Text?.Trim() ?? "";
        string password = txtPassword.Password;
>>>>>>> 9510d52d7eb1706487be963abce4edd11b18fe49

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

<<<<<<< HEAD
            var employeWindow = App.Services.GetRequiredService<EmployeSpaceView>();
            employeWindow.Show();
            this.Close();
=======
        var result = await _authService.ConnecterAsync(username, password);

        if (result.Succes && result.Donnee != null)
        {
            MessageBox.Show($"Connexion réussie !\nBienvenue {result.Donnee.NomUtilisateur}",
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            // TODO : Ouvrir MainWindow plus tard
            var main = new MainWindow();
            main.Show();

            this.Close();
        }
        else
        {
            MessageBox.Show(result.Erreur ?? "Nom d'utilisateur ou mot de passe incorrect.",
                            "Échec", MessageBoxButton.OK, MessageBoxImage.Error);
>>>>>>> 9510d52d7eb1706487be963abce4edd11b18fe49
        }
    }
}