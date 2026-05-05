using GestionDeConges.Core.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GestionDeConges.WPF.Converters;

// ── Bool → Visibility ─────────────────────────────────────────────────────────
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is Visibility.Visible;
}

// ── Bool inversé → Visibility (IsBusy → désactiver bouton) ───────────────────
[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is false;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is false;
}

// ── String non-vide → Visibility ─────────────────────────────────────────────
[ValueConversion(typeof(string), typeof(Visibility))]
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => !string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}

// ── StatutDemande → Brush de fond ────────────────────────────────────────────
[ValueConversion(typeof(StatutDemande), typeof(Brush))]
public class StatutToBrushConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is not StatutDemande statut) return Brushes.White;
        return statut switch
        {
            StatutDemande.Approuve => new SolidColorBrush(Color.FromRgb(234, 243, 222)),
            StatutDemande.EnAttente => new SolidColorBrush(Color.FromRgb(250, 238, 218)),
            StatutDemande.Refuse => new SolidColorBrush(Color.FromRgb(252, 235, 235)),
            StatutDemande.Annule => new SolidColorBrush(Color.FromRgb(240, 240, 240)),
            _ => Brushes.White
        };
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}

// ── StatutDemande → Brush de texte ───────────────────────────────────────────
[ValueConversion(typeof(StatutDemande), typeof(Brush))]
public class StatutToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is not StatutDemande statut) return Brushes.Black;
        return statut switch
        {
            StatutDemande.Approuve => new SolidColorBrush(Color.FromRgb(39, 80, 10)),
            StatutDemande.EnAttente => new SolidColorBrush(Color.FromRgb(99, 56, 6)),
            StatutDemande.Refuse => new SolidColorBrush(Color.FromRgb(80, 19, 19)),
            StatutDemande.Annule => new SolidColorBrush(Color.FromRgb(100, 100, 100)),
            _ => Brushes.Black
        };
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}

// ── StatutDemande → Libellé français ─────────────────────────────────────────
[ValueConversion(typeof(StatutDemande), typeof(string))]
public class StatutToLibelleConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is StatutDemande s ? s switch
        {
            StatutDemande.EnAttente => "En attente",
            StatutDemande.Approuve => "Approuvé",
            StatutDemande.Refuse => "Refusé",
            StatutDemande.Annule => "Annulé",
            _ => s.ToString()
        } : "";

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}

// ── bool EstModifiable → Visibility (bouton annuler demande) ─────────────────
[ValueConversion(typeof(bool), typeof(Visibility))]
public class EstModifiableToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}

// ── DateOnly → string formaté ─────────────────────────────────────────────────
[ValueConversion(typeof(DateOnly), typeof(string))]
public class DateOnlyToStringConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is DateOnly d ? d.ToString("dd/MM/yyyy") : "";

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => DateOnly.TryParse(value as string, out var d) ? d : DateOnly.MinValue;
}

// ── int Solde → couleur (rouge si < 3) ───────────────────────────────────────
[ValueConversion(typeof(int), typeof(Brush))]
public class SoldeToColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is int solde)
            return solde < 3
                ? new SolidColorBrush(Color.FromRgb(160, 40, 40))
                : new SolidColorBrush(Color.FromRgb(29, 158, 117));
        return Brushes.Black;
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}