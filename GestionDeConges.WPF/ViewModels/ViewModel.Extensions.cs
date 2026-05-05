// Ce fichier ajoute les propriétés proxy DateTime nécessaires pour que
// les DatePicker WPF fonctionnent avec DateOnly (non supporté nativement).
// On étend EmployeSpaceViewModel avec des propriétés partielles.

using CommunityToolkit.Mvvm.ComponentModel;

namespace GestionDeConges.WPF.ViewModels;

/// <summary>
/// Propriétés proxy DateTime exposées pour les DatePicker WPF.
/// WPF ne supporte pas DateOnly nativement (pas de conversion automatique).
/// Ces propriétés font le pont entre DateTime (DatePicker) et DateOnly (modèle).
/// </summary>
public partial class EmployeSpaceViewModel
{
    // ── DateDebut proxy ───────────────────────────────────────────────────────
    private DateTime _dateDebutDatetime = DateTime.Today;

    public DateTime DateDebutDatetime
    {
        get => _dateDebutDatetime;
        set
        {
            if (SetProperty(ref _dateDebutDatetime, value))
                DateDebut = DateOnly.FromDateTime(value);
        }
    }

    // ── DateFin proxy ─────────────────────────────────────────────────────────
    private DateTime _dateFinDatetime = DateTime.Today;

    public DateTime DateFinDatetime
    {
        get => _dateFinDatetime;
        set
        {
            if (SetProperty(ref _dateFinDatetime, value))
                DateFin = DateOnly.FromDateTime(value);
        }
    }
}