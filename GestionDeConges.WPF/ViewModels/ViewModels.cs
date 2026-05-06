using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;
using GestionDeConges.Core.Interfaces;
using GestionDeConges.WPF.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace GestionDeConges.WPF.ViewModels;

// ── MainViewModel ─────────────────────────────────────────────────────────────
/// <summary>ViewModel de la fenêtre principale admin (navigation sidebar).</summary>

// ── DashboardViewModel ────────────────────────────────────────────────────────
public partial class DashboardViewModel : BaseViewModel
{
    private readonly IRapportService _rapportService;

    public DashboardViewModel(IRapportService rapportService)
    {
        _rapportService = rapportService;
    }

    [ObservableProperty] private StatistiquesGlobales? _statistiques;

    [ObservableProperty] private int _totalDemandes;
    [ObservableProperty] private int _enAttente;
    [ObservableProperty] private int _approuvees;
    [ObservableProperty] private int _refusees;
    [ObservableProperty] private int _totalEmployes;
    [ObservableProperty] private int _employesEnConge;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        await RunSafeAsync(async () =>
        {
            Statistiques = await _rapportService.GetStatistiquesAsync(DateTime.Now.Year);
            TotalDemandes = Statistiques.TotalDemandes;
            EnAttente = Statistiques.EnAttente;
            Approuvees = Statistiques.Approuvees;
            Refusees = Statistiques.Refusees;
            TotalEmployes = Statistiques.TotalEmployes;
            EmployesEnConge = Statistiques.EmployesEnConge;
        }, "Erreur chargement tableau de bord");
    }
}

// ── DemandesViewModel ─────────────────────────────────────────────────────────
public partial class DemandesViewModel : BaseViewModel
{
    private readonly IDemandeCongeService _demandeService;
    private readonly SessionService _session;

    public DemandesViewModel(IDemandeCongeService demandeService, SessionService session)
    {
        _demandeService = demandeService;
        _session = session;
    }

    [ObservableProperty] private ObservableCollection<DemandeConge> _demandes = [];
    [ObservableProperty] private DemandeConge? _demandeSelectionnee;
    [ObservableProperty] private string _filtreRecherche = string.Empty;
    [ObservableProperty] private string _erreur = string.Empty;
    [ObservableProperty] private string _succes = string.Empty;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        await RunSafeAsync(async () =>
        {
            var liste = await _demandeService.GetToutesAsync();
            Demandes = new ObservableCollection<DemandeConge>(liste);
        });
    }

    [RelayCommand]
    private async Task ApprouverAsync()
    {
        if (DemandeSelectionnee is null) return;
        Erreur = Succes = string.Empty;
        await RunSafeAsync(async () =>
        {
            var r = await _demandeService.ApprouverAsync(
                DemandeSelectionnee.Id, _session.IdCourant);
            if (r.Succes) { Succes = "Demande approuvée !"; await ChargerAsync(); }
            else Erreur = r.Erreur ?? "Erreur inconnue";
        });
    }

    [RelayCommand]
    private async Task RefuserAsync(string commentaire)
    {
        if (DemandeSelectionnee is null) return;
        Erreur = Succes = string.Empty;
        await RunSafeAsync(async () =>
        {
            var r = await _demandeService.RefuserAsync(
                DemandeSelectionnee.Id, _session.IdCourant, commentaire);
            if (r.Succes) { Succes = "Demande refusée."; await ChargerAsync(); }
            else Erreur = r.Erreur ?? "Erreur inconnue";
        });
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        if (DemandeSelectionnee is null) return;
        var r = await _demandeService.SupprimerAsync(
            DemandeSelectionnee.Id, _session.IdCourant);
        if (r.Succes) await ChargerAsync();
        else Erreur = r.Erreur ?? "Erreur suppression";
    }
}

// ── EmployesViewModel ─────────────────────────────────────────────────────────

public partial class EmployesViewModel : BaseViewModel
{
    private readonly IEmployeService _employeService;
    private readonly IPosteService _posteService;
    private readonly SessionService _session;


    public EmployesViewModel(IEmployeService employeService, IPosteService posteService, SessionService session)
    {
        _employeService = employeService;
        _posteService = posteService;
        _session = session;
    }

    [ObservableProperty] private ObservableCollection<Employe> _employes = [];
    [ObservableProperty] private ObservableCollection<Employe> _supprimes = [];
    [ObservableProperty] private Employe? _employeSelectionne;
    [ObservableProperty] private string _filtreRecherche = string.Empty;
    [ObservableProperty] private bool _afficherHistorique;
    [ObservableProperty] private string _erreur = string.Empty;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        await RunSafeAsync(async () =>
        {
            if (AfficherHistorique)
            {
                var s = await _employeService.GetSupprimesAsync();
                Supprimes = new ObservableCollection<Employe>(s);
            }
            else
            {
                var liste = string.IsNullOrWhiteSpace(FiltreRecherche)
                    ? await _employeService.GetActifsAsync()
                    : await _employeService.RechercherAsync(FiltreRecherche);
                Employes = new ObservableCollection<Employe>(liste);
            }
        });
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        if (EmployeSelectionne is null) return;
        var r = await _employeService.SupprimerAsync(
            EmployeSelectionne.Id,
            _session.IdCourant);

        if (r.Succes) await ChargerAsync();
        else Erreur = r.Erreur ?? "Erreur";
    }

    [RelayCommand]
    private async Task RestaurerAsync()
    {
        if (EmployeSelectionne is null) return;
        var r = await _employeService.RestaurerAsync(EmployeSelectionne.Id);
        if (r.Succes) await ChargerAsync();
        else Erreur = r.Erreur ?? "Erreur";
    }

    [RelayCommand]
    private async Task ChangerPosteAsync(Employe? employe)
    {
        if (employe is null) return;
        var dialog = new ChangerPosteDialog(_posteService, employe)
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            var resultat = await _employeService.ChangerPosteAsync(
                employe.Id, dialog.IdPosteSelectionne, dialog.DateDebutSelectionnee);

            if (resultat.Succes)
                await ChargerAsync();
            else
                Erreur = resultat.Erreur ?? "Erreur lors du changement de poste.";
        }
    }

    public async Task AjouterEmployeAsync(Employe employe)
    {
        var resultat = await _employeService.CreerAsync(employe);
        if (resultat.Succes)
            await ChargerAsync();
        else
            Erreur = resultat.Erreur ?? "Erreur";
    }


    partial void OnFiltreRechercheChanged(string value)
        => _ = ChargerAsync();
}

// ── PostesViewModel ───────────────────────────────────────────────────────────
public partial class PostesViewModel : BaseViewModel
{
    private readonly IPosteService _posteService;

    public PostesViewModel(IPosteService posteService)
        => _posteService = posteService;

    [ObservableProperty] private ObservableCollection<Poste> _postes = [];
    [ObservableProperty] private ObservableCollection<Poste> _supprimes = [];
    [ObservableProperty] private Poste? _posteSelectionne;
    [ObservableProperty] private bool _afficherHistorique;
    [ObservableProperty] private string _erreur = string.Empty;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        await RunSafeAsync(async () =>
        {
            if (AfficherHistorique)
            {
                var s = await _posteService.GetSupprimesAsync();
                Supprimes = new ObservableCollection<Poste>(s);
            }
            else
            {
                var liste = await _posteService.GetActifsAsync();
                Postes = new ObservableCollection<Poste>(liste);
            }
        });
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        if (PosteSelectionne is null) return;
        var r = await _posteService.SupprimerAsync(PosteSelectionne.Id);
        if (r.Succes) await ChargerAsync();
        else Erreur = r.Erreur ?? "Erreur";
    }

    [RelayCommand]
    private async Task RestaurerAsync()
    {
        if (PosteSelectionne is null) return;
        var r = await _posteService.RestaurerAsync(PosteSelectionne.Id);
        if (r.Succes) await ChargerAsync();
        else Erreur = r.Erreur ?? "Erreur";
    }
}

// ── HistoriqueViewModel ───────────────────────────────────────────────────────
/// <summary>Affiche les éléments soft-deletés : congés, employés, postes.</summary>
public partial class HistoriqueViewModel : BaseViewModel
{
    private readonly IEmployeService _employeService;
    private readonly IPosteService _posteService;
    private readonly IDemandeCongeService _demandeService;

    public HistoriqueViewModel(
        IEmployeService employeService,
        IPosteService posteService,
        IDemandeCongeService demandeService)
    {
        _employeService = employeService;
        _posteService = posteService;
        _demandeService = demandeService;
    }

    [ObservableProperty] private ObservableCollection<Employe> _employesSupprimes = [];
    [ObservableProperty] private ObservableCollection<Poste> _postesSupprimes = [];
    [ObservableProperty] private ObservableCollection<DemandeConge> _demandesSupprimes = [];
    [ObservableProperty] private ObservableCollection<HistoriquePoste> _historiquePostes = [];

    [RelayCommand]
    public async Task ChargerAsync()
    {
        await RunSafeAsync(async () =>
        {
            var emp = await _employeService.GetSupprimesAsync();
            var pos = await _posteService.GetSupprimesAsync();
            var histo = await _employeService.GetTousHistoriquePostesAsync(); 

            EmployesSupprimes = new ObservableCollection<Employe>(emp);
            PostesSupprimes = new ObservableCollection<Poste>(pos);
            HistoriquePostes = new ObservableCollection<HistoriquePoste>(histo);
        });
    }

    [RelayCommand]
    private async Task RestaurerEmployeAsync(Employe? employe)
    {
        if (employe is null) return;
        await _employeService.RestaurerAsync(employe.Id);
        await ChargerAsync();
    }

    [RelayCommand]
    private async Task RestaurerPosteAsync(Poste? poste)
    {
        if (poste is null) return;
        await _posteService.RestaurerAsync(poste.Id);
        await ChargerAsync();
    }
}

// ── EmployeSpaceViewModel ─────────────────────────────────────────────────────
/// <summary>ViewModel de l'espace employé (demandes personnelles uniquement).</summary>
public partial class EmployeSpaceViewModel : BaseViewModel
{
    private readonly IDemandeCongeService _demandeService;
    private readonly ISoldeCongeService _soldeService;
    private readonly ITypeCongeRepository _typeCongeRepo;
    private readonly SessionService _session;

    public EmployeSpaceViewModel(
        IDemandeCongeService demandeService,
        ISoldeCongeService soldeService,
        ITypeCongeRepository typeCongeRepo,
        SessionService session)
    {
        _demandeService = demandeService;
        _soldeService = soldeService;
        _typeCongeRepo = typeCongeRepo;
        _session = session;
        NomEmploye = session.UtilisateurCourant?.NomUtilisateur ?? "";
    }

    [ObservableProperty] private string _nomEmploye = string.Empty;
    [ObservableProperty] private ObservableCollection<DemandeConge> _mesDemandes = [];
    [ObservableProperty] private ObservableCollection<SoldeConge> _mesSoldes = [];
    [ObservableProperty] private ObservableCollection<TypeConge> _typesConges = [];

    // Nouvelle demande
    [ObservableProperty] private TypeConge? _typeSelectionne;
    [ObservableProperty] private DateOnly _dateDebut = DateOnly.FromDateTime(DateTime.Today);
    [ObservableProperty] private DateOnly _dateFin = DateOnly.FromDateTime(DateTime.Today);
    [ObservableProperty] private string? _motif;
    [ObservableProperty] private string _erreur = string.Empty;
    [ObservableProperty] private string _succes = string.Empty;
    [ObservableProperty] private int _soldeRestant;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        if (_session.IdEmployeCourant < 0) return;
        await RunSafeAsync(async () =>
        {
            var dem = await _demandeService.GetParEmployeAsync(_session.IdEmployeCourant);
            var sol = await _soldeService.GetParEmployeAsync(_session.IdEmployeCourant, DateTime.Now.Year);
            var types = await _typeCongeRepo.GetActifsAsync();

            MesDemandes = new ObservableCollection<DemandeConge>(dem);
            MesSoldes = new ObservableCollection<SoldeConge>(sol);
            TypesConges = new ObservableCollection<TypeConge>(types);
        });
    }

    partial void OnTypeSelectionneChanged(TypeConge? value)
    {
        if (value is null) { SoldeRestant = 0; return; }
        _ = MettreAJourSoldeAsync(value.Id);
    }

    private async Task MettreAJourSoldeAsync(int idType)
    {
        SoldeRestant = await _soldeService.GetSoldeRestantAsync(
            _session.IdEmployeCourant, idType, DateTime.Now.Year);
    }

    [RelayCommand]
    private async Task SoumettreDemandeAsync()
    {
        Erreur = Succes = string.Empty;
        if (TypeSelectionne is null)
        {
            Erreur = "Veuillez sélectionner un type de congé.";
            return;
        }

        await RunSafeAsync(async () =>
        {
            var demande = new DemandeConge
            {
                IdEmploye = _session.IdEmployeCourant,
                IdTypeConge = TypeSelectionne.Id,
                DateDebut = DateDebut,
                DateFin = DateFin,
                Motif = Motif,
                Statut = Core.Enums.StatutDemande.EnAttente
            };

            var r = await _demandeService.SoumettreAsync(demande);
            if (r.Succes)
            {
                Succes = "Demande soumise avec succès ! En attente d'approbation.";
                Motif = null;
                await ChargerAsync();
            }
            else
            {
                Erreur = r.Erreur ?? "Erreur inconnue";
            }
        }, "Erreur soumission");
    }

    [RelayCommand]
    private async Task AnnulerDemandeAsync(DemandeConge? demande)
    {
        if (demande is null) return;
        var r = await _demandeService.AnnulerAsync(demande.Id, _session.IdCourant);
        if (r.Succes) await ChargerAsync();
        else Erreur = r.Erreur ?? "Erreur annulation";
    }
}