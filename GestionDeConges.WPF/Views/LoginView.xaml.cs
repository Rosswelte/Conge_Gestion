using GestionDeConges.Core.Interfaces;
using GestionDeConges.WPF.Views;
using System.Windows;

namespace GestionDeConges.WPF.Views;

public partial class LoginView : Window
{
    private readonly IAuthService _authService;

    public LoginView(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Veuillez remplir tous les champs", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = await _authService.ConnecterAsync(username, password);

        if (result.Succes)
        {
            MessageBox.Show($"Connexion réussie !\nBienvenue {result.Donnee?.NomUtilisateur}",
                          "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            // Ouvrir la fenêtre principale
            var mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close(); // Fermer la fenêtre de login
        }
        else
        {
            MessageBox.Show(result.Erreur ?? "Identifiants incorrects",
                          "Échec de connexion", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}