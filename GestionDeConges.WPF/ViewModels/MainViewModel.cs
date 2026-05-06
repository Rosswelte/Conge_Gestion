using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionDeConges.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GestionDeConges.WPF.ViewModels;

/// <summary>
/// ViewModel de la fenêtre principale admin.
/// Corrige : chargement automatique des données à chaque navigation.
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly SessionService _session;

    public MainViewModel(SessionService session)
    {
        _session = session;
        NomAdmin = session.UtilisateurCourant?.NomUtilisateur ?? "Admin";
    }

    [ObservableProperty] private string _nomAdmin = string.Empty;
    [ObservableProperty] private BaseViewModel? _pageActive;

    // ── Navigation avec chargement automatique ────────────────────────────────

    [RelayCommand]
    private async Task NaviguerDashboardAsync()
    {
        var vm = App.Services.GetRequiredService<DashboardViewModel>();
        PageActive = vm;
        await vm.ChargerCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task NaviguerDemandesAsync()
    {
        var vm = App.Services.GetRequiredService<DemandesViewModel>();
        PageActive = vm;
        await vm.ChargerCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task NaviguerEmployesAsync()
    {
        var vm = App.Services.GetRequiredService<EmployesViewModel>();
        PageActive = vm;
        await vm.ChargerCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task NaviguerPostesAsync()
    {
        var vm = App.Services.GetRequiredService<PostesViewModel>();
        PageActive = vm;
        await vm.ChargerCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task NaviguerHistoriqueAsync()
    {
        var vm = App.Services.GetRequiredService<HistoriqueViewModel>();
        PageActive = vm;
        await vm.ChargerCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private void SeDeconnecter()
    {
        _session.FermerSession();
        var login = App.Services.GetRequiredService<LoginView>();
        login.Show();
        // La fermeture de MainWindow est gérée dans le code-behind
    }
}
