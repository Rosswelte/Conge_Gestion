using GestionDeConges.WPF.ViewModels;
using GestionDeConges.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GestionDeConges.WPF.Views;

public partial class EmployeSpaceView : Window
{
    private readonly EmployeSpaceViewModel _vm;

    public EmployeSpaceView(EmployeSpaceViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
        Loaded += async (_, _) => await _vm.ChargerAsync();
    }

    private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
    {
        App.Services.GetRequiredService<SessionService>().FermerSession();
        var login = App.Services.GetRequiredService<LoginView>();
        login.Show();
        this.Close();
    }
}