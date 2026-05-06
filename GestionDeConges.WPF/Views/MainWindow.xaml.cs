using GestionDeConges.WPF.ViewModels;
using GestionDeConges.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GestionDeConges.WPF;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;

        // Charger la page dashboard par défaut au démarrage
        Loaded += async (_, _) =>
        {
            _vm.NaviguerDashboardCommand.Execute(null);
            // Charger les données du dashboard
            if (_vm.PageActive is DashboardViewModel dash)
                await dash.ChargerAsync();
        };
    }

    // Déconnexion : ferme cette fenêtre après que le ViewModel a ouvert LoginView
    private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
    {
        // Le ViewModel ouvre déjà LoginView, on ferme juste MainWindow
        this.Close();
    }
}