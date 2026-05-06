using GestionDeConges.Core.Interfaces;
using GestionDeConges.Data;
using GestionDeConges.Data.Context;
using GestionDeConges.Services.Implementations;
using GestionDeConges.WPF.Views;
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

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        var loginWindow = Services.GetRequiredService<LoginView>();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Base de données
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseMySql("Server=localhost;Database=gestion_conges;User=root;Password=;",
                         ServerVersion.AutoDetect("Server=localhost;Database=gestion_conges;User=root;Password=;")));

        // Repositories & UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddScoped<IEmployeService, EmployeService>();
        services.AddScoped<IDemandeCongeService, DemandeCongeService>();
        services.AddScoped<IPosteService, PosteService>();
        services.AddScoped<IRapportService, RapportService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Views
        services.AddTransient<LoginView>();
        services.AddTransient<MainWindow>();   
    }
}