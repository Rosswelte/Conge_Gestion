using CommunityToolkit.Mvvm.ComponentModel;

namespace GestionDeConges.WPF.ViewModels;

/// <summary>
/// Classe de base pour tous les ViewModels.
/// Hérite de ObservableObject (CommunityToolkit.Mvvm).
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private string _statusMessage = string.Empty;

    /// <summary>Indique qu'une opération async est en cours (spinner, blocage UI).</summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>Message d'état affiché dans la barre de statut.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Exécute une action async en activant IsBusy automatiquement.
    /// Capture les exceptions et les affiche dans StatusMessage.
    /// </summary>
    protected async Task RunSafeAsync(Func<Task> action, string? errorPrefix = null)
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            StatusMessage = $"{errorPrefix ?? "Erreur"} : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}