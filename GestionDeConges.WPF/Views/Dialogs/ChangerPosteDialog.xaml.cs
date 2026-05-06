using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Interfaces;
using System.Windows;

namespace GestionDeConges.WPF.Views.Dialogs;

public partial class ChangerPosteDialog : Window
{
    private readonly IPosteService _posteService;

    public int IdPosteSelectionne { get; private set; }
    public DateOnly DateDebutSelectionnee { get; private set; }

    public ChangerPosteDialog(IPosteService posteService, Employe employe)
    {
        InitializeComponent();
        _posteService = posteService;
        TxtEmploye.Text = $"Employé : {employe.NomComplet} — Poste actuel : {employe.Poste?.Nom ?? "Aucun"}";
        Loaded += async (_, _) => await ChargerPostesAsync();
    }

    private async Task ChargerPostesAsync()
    {
        var postes = await _posteService.GetActifsAsync();
        CboPostes.ItemsSource = postes.ToList();
        if (CboPostes.Items.Count > 0)
            CboPostes.SelectedIndex = 0;
    }

    private void BtnChanger_Click(object sender, RoutedEventArgs e)
    {
        if (CboPostes.SelectedItem is not Poste poste)
        {
            MessageBox.Show("Veuillez sélectionner un poste.", "Attention",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IdPosteSelectionne = poste.Id;
        DateDebutSelectionnee = DateOnly.FromDateTime(DpDateDebut.SelectedDate ?? DateTime.Today);
        DialogResult = true;
        Close();
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}