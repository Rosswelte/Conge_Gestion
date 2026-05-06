using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Interfaces;
using System.Windows;

namespace GestionDeConges.WPF.Views.Dialogs;

public partial class ChoixEmployeDialog : Window
{
    private readonly IEmployeService _employeService;
    public Employe? EmployeSelectionne { get; private set; }

    public ChoixEmployeDialog(IEmployeService employeService)
    {
        InitializeComponent();
        _employeService = employeService;
        Loaded += async (_, _) => await ChargerEmployesAsync();
    }

    private async Task ChargerEmployesAsync()
    {
        var employes = await _employeService.GetActifsAsync();
        CboEmployes.ItemsSource = employes.OrderBy(e => e.Nom).ToList();
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        if (CboEmployes.SelectedItem is not Employe emp)
        {
            MessageBox.Show("Veuillez sélectionner votre nom.",
                "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        EmployeSelectionne = emp;
        DialogResult = true;
        Close();
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}