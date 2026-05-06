using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;
using GestionDeConges.Core.Interfaces;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
namespace GestionDeConges.WPF.Views.Dialogs;

public partial class AjouterEmployeDialog : Window
{
    private readonly IPosteService _posteService;
    public Employe? EmployeCree { get; private set; }

    public AjouterEmployeDialog(IPosteService posteService)
    {
        InitializeComponent();
        _posteService = posteService;
        Loaded += async (_, _) => await ChargerPostesAsync();
    }

    private async Task ChargerPostesAsync()
    {
        var postes = await _posteService.GetActifsAsync();
        CboPoste.ItemsSource = postes.ToList();
        if (CboPoste.Items.Count > 0)
            CboPoste.SelectedIndex = 0;
    }

    private void BtnAjouter_Click(object sender, RoutedEventArgs e)
    {
        // Réinitialiser l'erreur
        MasquerErreur();

        var nom = TxtNom.Text.Trim();
        var prenom = TxtPrenom.Text.Trim();
        var email = TxtEmail.Text.Trim();
        var telephone = TxtTelephone.Text.Trim();

        // Validation
        if (string.IsNullOrWhiteSpace(nom))
        {
            AfficherErreur("Le nom est obligatoire.");
            return;
        }
        if (string.IsNullOrWhiteSpace(prenom))
        {
            AfficherErreur("Le prénom est obligatoire.");
            return;
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            AfficherErreur("L'email est obligatoire.");
            return;
        }
        if (!IsValidEmail(email))
        {
            AfficherErreur("Format d'email invalide (ex: nom@domaine.com).");
            return;
        }
        if (!string.IsNullOrWhiteSpace(telephone) && !IsValidTelephone(telephone))
        {
            AfficherErreur("Le téléphone ne doit contenir que des chiffres, espaces, +, -, . ou /");
            return;
        }
        if (CboPoste.SelectedItem is not Poste poste)
        {
            AfficherErreur("Veuillez sélectionner un poste.");
            return;
        }

        var sexe = Sexe.M;
        if (CboSexe.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            sexe = tag switch
            {
                "F" => Sexe.F,
                "Autre" => Sexe.Autre,
                _ => Sexe.M
            };
        }

        EmployeCree = new Employe
        {
            Nom = nom,
            Prenom = prenom,
            Email = email,
            Telephone = string.IsNullOrWhiteSpace(telephone) ? null : telephone,
            IdPoste = poste.Id,
            DateEmbauche = DateOnly.FromDateTime(DpDateEmbauche.SelectedDate ?? DateTime.Today),
            Sexe = sexe,
            EstActif = true
        };

        DialogResult = true;
        Close();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidTelephone(string telephone)
    {
        // Accepte : chiffres, espaces, +, -, ., /
        return System.Text.RegularExpressions.Regex.IsMatch(telephone, @"^[\d\s\+\-\.\/\(\)]+$");
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

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    
}