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

        // Naviguer vers le dashboard par défaut dès l'ouverture
        Loaded += async (_, _) =>
            await _vm.NaviguerDashboardCommand.ExecuteAsync(null);
    }

    // Ferme MainWindow après que le ViewModel a ouvert LoginView
    private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
