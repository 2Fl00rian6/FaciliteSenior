using System.Windows.Input;
using FaciliteSenior.Commands;
using FaciliteSenior.Models;

namespace FaciliteSenior.ViewModels;

public sealed class BrowserViewModel : ViewModelBase
{
    private string _friendlyName = "Navigation";
    private string _pageTitle = string.Empty;
    private string _currentUrl = string.Empty;
    private bool _canGoBack;
    private bool _isLoading;
    private bool _isErrorVisible;
    private string _errorMessage = string.Empty;
    private string _statusMessage = "Choisissez une page pour commencer.";

    public BrowserViewModel()
    {
        RetryCommand = new RelayCommand(_ => RetryCurrentPage(), _ => !string.IsNullOrWhiteSpace(_currentUrl));
    }

    public event EventHandler<string>? NavigateRequested;

    public event EventHandler? RefreshRequested;

    public event EventHandler? BackRequested;

    public event EventHandler? ClearRequested;

    public ICommand RetryCommand { get; }

    public string FriendlyName
    {
        get => _friendlyName;
        private set => SetProperty(ref _friendlyName, value);
    }

    public string PageTitle
    {
        get => _pageTitle;
        private set => SetProperty(ref _pageTitle, value);
    }

    public string CurrentUrl
    {
        get => _currentUrl;
        private set
        {
            if (SetProperty(ref _currentUrl, value))
            {
                RaiseRetryAvailability();
            }
        }
    }

    public bool CanGoBack
    {
        get => _canGoBack;
        private set => SetProperty(ref _canGoBack, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsErrorVisible
    {
        get => _isErrorVisible;
        private set => SetProperty(ref _isErrorVisible, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public void OpenFavorite(FavoriteLink favorite)
    {
        var url = favorite.Url.Trim();

        // Si c'est deja la page ouverte (meme lien reclique), on ne recharge pas :
        // on retrouve la page telle qu'elle etait laissee (position, formulaire en cours...),
        // exactement comme un onglet de navigateur qu'on retrouve.
        if (!IsErrorVisible && string.Equals(url, CurrentUrl, StringComparison.OrdinalIgnoreCase))
        {
            FriendlyName = favorite.Label;
            return;
        }

        FriendlyName = favorite.Label;
        PageTitle = string.Empty;
        CurrentUrl = url;
        HideError();
        SetLoading(true, $"Ouverture de {favorite.Label}...");
        NavigateRequested?.Invoke(this, CurrentUrl);
    }

    public void NavigateInsideApplication(string friendlyName, string url)
    {
        FriendlyName = friendlyName;
        CurrentUrl = url;
        HideError();
        SetLoading(true, $"Ouverture de {friendlyName}...");
        NavigateRequested?.Invoke(this, url);
    }

    public void RefreshPage()
    {
        SetLoading(true, "Actualisation en cours...");
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    public void GoBackInPage()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ClearPage()
    {
        PageTitle = string.Empty;
        CanGoBack = false;
        IsLoading = false;
        StatusMessage = "Retour a l'accueil.";
        ClearRequested?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateNavigationState(string currentUrl, bool canGoBack, string? pageTitle)
    {
        CurrentUrl = currentUrl ?? string.Empty;
        CanGoBack = canGoBack;
        PageTitle = string.IsNullOrWhiteSpace(pageTitle) ? string.Empty : pageTitle.Trim();
        HideError();
        IsLoading = false;
        StatusMessage = string.IsNullOrWhiteSpace(PageTitle) ? $"{FriendlyName} est pret." : PageTitle;
    }

    public void SetLoading(bool isLoading, string? message = null)
    {
        IsLoading = isLoading;

        if (!string.IsNullOrWhiteSpace(message))
        {
            StatusMessage = message.Trim();
        }
    }

    public void ShowError(string message)
    {
        IsLoading = false;
        IsErrorVisible = true;
        ErrorMessage = message;
        StatusMessage = message;
    }

    public void HideError()
    {
        IsErrorVisible = false;
        ErrorMessage = string.Empty;
    }

    private void RetryCurrentPage()
    {
        if (string.IsNullOrWhiteSpace(CurrentUrl))
        {
            return;
        }

        HideError();
        SetLoading(true, "Nouvelle tentative...");
        NavigateRequested?.Invoke(this, CurrentUrl);
    }

    private void RaiseRetryAvailability()
    {
        if (RetryCommand is RelayCommand relayCommand)
        {
            relayCommand.RaiseCanExecuteChanged();
        }
    }
}
