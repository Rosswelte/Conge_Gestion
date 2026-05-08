using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Interfaces;
using System.Windows;

namespace GestionDeConges.WPF.Views.Dialogs;

public partial class ModifierDemandeDialog : Window
{
    private readonly DemandeConge _demande;
    private readonly ITypeCongeRepository _typeRepo;
    public DemandeConge? DemandeModifiee { get; private set; }

    public ModifierDemandeDialog(DemandeConge demande, ITypeCongeRepository typeRepo)
    {
        InitializeComponent();
        _demande = demande;
        _typeRepo = typeRepo;
        Loaded += async (_, _) =>
        {
            var types = await _typeRepo.GetActifsAsync();
            CboType.ItemsSource = types.ToList();
            CboType.SelectedItem = types.FirstOrDefault(t => t.Id == demande.IdTypeConge);
            DpDebut.SelectedDate = demande.DateDebut.ToDateTime(TimeOnly.MinValue);
            DpFin.SelectedDate = demande.DateFin.ToDateTime(TimeOnly.MinValue);
            TxtMotif.Text = demande.Motif ?? "";
        };
    }

    private void BtnModifier_Click(object sender, RoutedEventArgs e)
    {
        PanelErreur.Visibility = Visibility.Collapsed;

        if (CboType.SelectedItem is not TypeConge type)
        { AfficherErreur("Sélectionnez un type de congé."); return; }
        if (DpDebut.SelectedDate is null || DpFin.SelectedDate is null)
        { AfficherErreur("Les dates sont obligatoires."); return; }
        if (DpDebut.SelectedDate > DpFin.SelectedDate)
        { AfficherErreur("La date de début doit être avant la date de fin."); return; }

        _demande.IdTypeConge = type.Id;
        _demande.DateDebut = DateOnly.FromDateTime(DpDebut.SelectedDate.Value);
        _demande.DateFin = DateOnly.FromDateTime(DpFin.SelectedDate.Value);
        _demande.Motif = string.IsNullOrWhiteSpace(TxtMotif.Text) ? null : TxtMotif.Text.Trim();

        DemandeModifiee = _demande;
        DialogResult = true;
        Close();
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void AfficherErreur(string msg)
    {
        TxtErreur.Text = msg;
        PanelErreur.Visibility = Visibility.Visible;
    }
}