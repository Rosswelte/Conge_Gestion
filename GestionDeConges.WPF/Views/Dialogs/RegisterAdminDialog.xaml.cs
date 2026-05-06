using GestionDeConges.Core.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace GestionDeConges.WPF.Views.Dialogs;

public partial class RegisterAdminDialog : Window
{
    private readonly IAuthService _authService;

    public RegisterAdminDialog(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void BtnCreer_Click(object sender, RoutedEventArgs e)
    {
        var nomUtil = TxtNomUtilisateur.Text.Trim();
        var motDePasse = TxtMotDePasse.Password;
        var codeSecret = TxtCodeSecret.Password;

        // Validation
        if (string.IsNullOrWhiteSpace(nomUtil))
        {
            AfficherErreur("Veuillez saisir un nom d'utilisateur.");
            return;
        }

        if (motDePasse.Length < 6)
        {
            AfficherErreur("Le mot de passe doit contenir au moins 6 caractères.");
            return;
        }

        if (string.IsNullOrWhiteSpace(codeSecret))
        {
            AfficherErreur("Veuillez saisir le code secret administrateur.");
            return;
        }

        // Appel au service
        MasquerErreur();
        Cursor = Cursors.Wait;
        BtnCreer.IsEnabled = false;

        try
        {
            var resultat = await _authService.CreerAdminAsync(nomUtil, motDePasse, codeSecret);

            if (resultat.Succes)
            {
                MessageBox.Show(
                    $"Le compte administrateur \"{nomUtil}\" a été créé avec succès.{Environment.NewLine}{Environment.NewLine}Vous pouvez maintenant vous connecter.",
                    "✅ Compte créé",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            else
            {
                AfficherErreur(resultat?.Erreur ?? "Erreur inconnue lors de la création.");
            }
        }
        catch (Exception ex)
        {
            AfficherErreur($"Erreur : {ex.Message}");
        }
        finally
        {
            Cursor = Cursors.Arrow;
            BtnCreer.IsEnabled = true;
        }
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void AfficherErreur(string message)
    {
        TxtErreur.Text = message;
        TxtErreur.Visibility = Visibility.Visible;
    }

    private void MasquerErreur()
    {
        TxtErreur.Text = "";
        TxtErreur.Visibility = Visibility.Collapsed;
    }
}