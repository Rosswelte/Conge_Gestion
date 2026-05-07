using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;

namespace GestionDeConges.WPF.Views.Dialogs;

public partial class ModifierPosteDialog : Window
{
    private readonly Poste _poste;
    public Poste? PosteModifie { get; private set; }

    public ModifierPosteDialog(Poste poste)
    {
        InitializeComponent();
        _poste = poste;
        Loaded += async (_, _) =>
        {
            var uow = App.Services.GetRequiredService<IUnitOfWork>();
            var depts = await uow.Postes.GetActifsAsync();
            var departements = depts.Select(p => p.Departement).DistinctBy(d => d.Id).OrderBy(d => d.Nom).ToList();
            CboDepartement.ItemsSource = departements;

            TxtNom.Text = poste.Nom;
            TxtNbMin.Text = poste.NbMinEmployes.ToString();
            CboDepartement.SelectedItem = departements.FirstOrDefault(d => d.Id == poste.IdDepartement);
        };
    }

    private void BtnModifier_Click(object sender, RoutedEventArgs e)
    {
        PanelErreur.Visibility = Visibility.Collapsed;
        var nom = TxtNom.Text.Trim();

        if (string.IsNullOrWhiteSpace(nom))
        { AfficherErreur("Le nom est obligatoire."); return; }
        if (CboDepartement.SelectedItem is not Departement dept)
        { AfficherErreur("Sélectionnez un département."); return; }
        if (!int.TryParse(TxtNbMin.Text.Trim(), out int nbMin) || nbMin < 0)
        { AfficherErreur("Nombre minimum invalide."); return; }

        _poste.Nom = nom;
        _poste.IdDepartement = dept.Id;
        _poste.NbMinEmployes = nbMin;

        PosteModifie = _poste;
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