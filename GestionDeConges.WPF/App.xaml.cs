using GestionDeConges.Core.Interfaces;
using GestionDeConges.Data;
using GestionDeConges.Data.Context;
using GestionDeConges.Data.Repositories;
using GestionDeConges.Services.Implementations;
using GestionDeConges.WPF.ViewModels;
using GestionDeConges.WPF.Views;
using GestionDeConges.WPF.Views.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using System.Windows;

namespace GestionDeConges.WPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        QuestPDF.Settings.License = LicenseType.Community;

        var svc = new ServiceCollection();
        ConfigureServices(svc);
        Services = svc.BuildServiceProvider();

        using (var scope = Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ctx.Database.EnsureCreated();
        }

        var login = Services.GetRequiredService<LoginView>();
        login.Show();
    }

    private static void ConfigureServices(IServiceCollection svc)
    {
        // ── Base de données ───────────────────────────────────────
        svc.AddDbContext<AppDbContext>(opt =>
            opt.UseMySql(
                "Server=localhost;Database=gestion_conges;User=root;Password=;",
                ServerVersion.AutoDetect(
                    "Server=localhost;Database=gestion_conges;User=root;Password=;"),
                x => x.MigrationsAssembly("GestionDeConges.Data")),
            ServiceLifetime.Scoped);

        // ── Unit of Work ──────────────────────────────────────────
        svc.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Repositories ──────────────────────────────────────────
        svc.AddScoped<ITypeCongeRepository, TypeCongeRepository>(); 

        // ── Services métier ───────────────────────────────────────
        svc.AddScoped<IAuthService, AuthService>();
        svc.AddScoped<INotificationService, NotificationService>();
        svc.AddScoped<IEmployeService, EmployeService>();
        svc.AddScoped<IDemandeCongeService, DemandeCongeService>();
        svc.AddScoped<ISoldeCongeService, SoldeCongeService>();
        svc.AddScoped<IPosteService, PosteService>();
        svc.AddScoped<IRapportService, RapportService>();

        // ── Session ───────────────────────────────────────────────
        svc.AddSingleton<SessionService>();

        // ── ViewModels ────────────────────────────────────────────
        svc.AddTransient<LoginViewModel>();
        svc.AddTransient<MainViewModel>();
        svc.AddTransient<DashboardViewModel>();
        svc.AddTransient<DemandesViewModel>();
        svc.AddTransient<EmployesViewModel>();
        svc.AddTransient<PostesViewModel>();
        svc.AddTransient<HistoriqueViewModel>();
        svc.AddTransient<EmployeSpaceViewModel>();

        // ── Views / Dialogs ───────────────────────────────────────
        svc.AddTransient<LoginView>();
        svc.AddTransient<MainWindow>();
        svc.AddTransient<ChoixEmployeDialog>();
        svc.AddTransient<EmployeSpaceView>();
    }
}