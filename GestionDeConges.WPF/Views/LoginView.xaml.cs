using GestionDeConges.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GestionDeConges.WPF.Views;

public partial class LoginView : Window
{
    private readonly IAuthService _authService;

    // Constructeur pour Dependency Injection
    public LoginView(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
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

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Veuillez remplir tous les champs", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

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
        }
    }
}