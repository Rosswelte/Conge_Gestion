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
        CacherErreur();

        var nom = TxtNom.Text.Trim();
        var prenom = TxtPrenom.Text.Trim();
        var email = TxtEmail.Text.Trim();
        var telephone = TxtTelephone.Text.Trim();

        // ── Validation ──────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(nom))
        { AfficherErreur("Le nom est obligatoire."); TxtNom.Focus(); return; }
        if (nom.Length < 2)
        { AfficherErreur("Le nom doit contenir au moins 2 caractères."); TxtNom.Focus(); return; }
        if (nom.Length > 60)
        { AfficherErreur("Le nom ne doit pas dépasser 60 caractères."); TxtNom.Focus(); return; }
        if (nom.Any(char.IsDigit))
        { AfficherErreur("Le nom  ne doit pas contenir de chiffres."); return; }
        if (string.IsNullOrWhiteSpace(prenom))
        { AfficherErreur("Le prénom est obligatoire."); TxtPrenom.Focus(); return; }
        if (prenom.Length < 2)
        { AfficherErreur("Le prénom doit contenir au moins 2 caractères."); TxtPrenom.Focus(); return; }
        if (prenom.Length > 60)
        { AfficherErreur("Le prénom ne doit pas dépasser 60 caractères."); TxtPrenom.Focus(); return; }
        if (prenom.Any(char.IsDigit))
        { AfficherErreur("Le prenom  ne doit pas contenir de chiffres."); return; }

        if (string.IsNullOrWhiteSpace(email))
        { AfficherErreur("L'email est obligatoire."); TxtEmail.Focus(); return; }
        if (!IsValidEmail(email))
        { AfficherErreur("Format d'email invalide (ex: nom@domaine.com)."); TxtEmail.Focus(); return; }
        if (email.Length > 120)
        { AfficherErreur("L'email ne doit pas dépasser 120 caractères."); TxtEmail.Focus(); return; }

        if (!string.IsNullOrWhiteSpace(telephone))
        {
            if (!IsValidTelephone(telephone))
            { AfficherErreur("Téléphone invalide (chiffres, espaces, +, -, ., /, ( ) uniquement)."); TxtTelephone.Focus(); return; }
            if (telephone.Length < 6)
            { AfficherErreur("Le numéro de téléphone est trop court (min. 6 caractères)."); TxtTelephone.Focus(); return; }
            if (telephone.Length > 20)
            { AfficherErreur("Le numéro de téléphone ne doit pas dépasser 20 caractères."); TxtTelephone.Focus(); return; }
        }
        if (DpDateNaissance.SelectedDate is not null)
        {
            var dateNaissance = DateOnly.FromDateTime(DpDateNaissance.SelectedDate.Value);
            if (dateNaissance > DateOnly.FromDateTime(DateTime.Today))
            { AfficherErreur("La date de naissance ne peut pas être dans le futur."); return; }
            if (dateNaissance.Year < 1900)
            { AfficherErreur("La date de naissance semble incorrecte."); return; }
        }
        // Validation âge (≥ 18 ans)
        if (DpDateNaissance.SelectedDate is not null)
        {
            var dateNaissance = DateOnly.FromDateTime(DpDateNaissance.SelectedDate.Value);
            var age = DateTime.Today.Year - DpDateNaissance.SelectedDate.Value.Year;
            if (DpDateNaissance.SelectedDate.Value > DateTime.Today.AddYears(-age))
                age--; // ajuste si l'anniversaire n'est pas encore passé

            if (age < 18)
            {
                AfficherErreur("L'employé doit avoir au moins 18 ans.");
                return;
            }
            if (age > 100)
            {
                AfficherErreur("La date de naissance semble incorrecte (plus de 100 ans).");
                return;
            }
        }

        if (DpDateEmbauche.SelectedDate is null)
        { AfficherErreur("La date d'embauche est obligatoire."); return; }
        var dateEmbauche = DateOnly.FromDateTime(DpDateEmbauche.SelectedDate.Value);
        if (dateEmbauche > DateOnly.FromDateTime(DateTime.Today))
        { AfficherErreur("La date d'embauche ne peut pas être dans le futur."); return; }
        if (dateEmbauche.Year < 2000)
        { AfficherErreur("La date d'embauche semble incorrecte (avant 2000)."); return; }

        if (CboPoste.SelectedItem is not Poste poste)
        { AfficherErreur("Veuillez sélectionner un poste."); return; }

        if (CboSexe.SelectedItem is null)
        { AfficherErreur("Veuillez sélectionner le sexe."); return; }

        // ── Construction ────────────────────────────────────────────────────
        var sexe = Sexe.M;
        if (CboSexe.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            sexe = tag switch { "F" => Sexe.F, "Autre" => Sexe.Autre, _ => Sexe.M };

        EmployeCree = new Employe
        {
            Nom = nom,
            Prenom = prenom,
            Email = email.ToLower(),
            Telephone = string.IsNullOrWhiteSpace(telephone) ? null : telephone,
            DateNaissance = DpDateNaissance.SelectedDate is not null
                ? DateOnly.FromDateTime(DpDateNaissance.SelectedDate.Value)
                : null,
            IdPoste = poste.Id,
            DateEmbauche = dateEmbauche,
            Sexe = sexe,
            EstActif = true
        };

        DialogResult = true;
        Close();
    }

    // ── Validation helpers ──────────────────────────────────────────────────
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && email.Contains('.') && email.Contains('@');
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidTelephone(string telephone)
        => Regex.IsMatch(telephone, @"^[\d\s\+\-\.\/\(\)]{6,20}$");

    // ── UI helpers ──────────────────────────────────────────────────────────
    private void AfficherErreur(string message)
    {
        TxtErreur.Text = message;
        PanelErreur.Visibility = Visibility.Visible;
    }

    private void CacherErreur()
        => PanelErreur.Visibility = Visibility.Collapsed;

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}