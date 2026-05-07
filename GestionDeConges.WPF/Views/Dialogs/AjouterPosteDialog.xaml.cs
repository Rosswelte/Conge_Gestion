using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;

namespace GestionDeConges.WPF.Views.Dialogs;

public partial class AjouterPosteDialog : Window
{
    public Poste? PosteCree { get; private set; }

    public AjouterPosteDialog()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            var uow = App.Services.GetRequiredService<IUnitOfWork>();
            var depts = await uow.Postes.GetActifsAsync();
            var departements = depts.Select(p => p.Departement).DistinctBy(d => d.Id).OrderBy(d => d.Nom).ToList();
            CboDepartement.ItemsSource = departements;
            if (departements.Count > 0) CboDepartement.SelectedIndex = 0;
        };
    }

    private void BtnAjouter_Click(object sender, RoutedEventArgs e)
    {
        PanelErreur.Visibility = Visibility.Collapsed;
        var nom = TxtNom.Text.Trim();

        if (string.IsNullOrWhiteSpace(nom))
        { AfficherErreur("Le nom du poste est obligatoire."); return; }
        if (CboDepartement.SelectedItem is not Departement dept)
        { AfficherErreur("Veuillez sélectionner un département."); return; }
        if (!int.TryParse(TxtNbMin.Text.Trim(), out int nbMin) || nbMin < 0)
        { AfficherErreur("Nombre minimum d'employés invalide."); return; }

        PosteCree = new Poste
        {
            Nom = nom,
            IdDepartement = dept.Id,
            NbMinEmployes = nbMin,
            EstActif = true
        };

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