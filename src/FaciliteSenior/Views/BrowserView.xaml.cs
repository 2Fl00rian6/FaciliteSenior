using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FaciliteSenior.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace FaciliteSenior.Views;

public partial class BrowserView : UserControl
{
    private const string SafetyScript = """
        (() => {
          const stopEvent = (event) => {
            event.preventDefault();
            event.stopPropagation();
          };

          window.addEventListener('dragover', stopEvent, true);
          window.addEventListener('drop', stopEvent, true);

          window.open = function(url) {
            if (typeof url === 'string' && url.length > 0) {
              window.location.href = url;
            }
            return null;
          };
        })();
        """;

    private BrowserViewModel? _viewModel;
    private bool _isInitialized;
    private bool _eventsAttached;

    public BrowserView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await EnsureBrowserInitializedAsync();

            if (_viewModel is not null && !string.IsNullOrWhiteSpace(_viewModel.CurrentUrl))
            {
                await NavigateToAsync(_viewModel.CurrentUrl);
            }
        }
        catch (Exception ex)
        {
            ShowInitializationError(ex);
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BrowserViewModel oldViewModel)
        {
            oldViewModel.NavigateRequested -= OnNavigateRequested;
            oldViewModel.RefreshRequested -= OnRefreshRequested;
            oldViewModel.BackRequested -= OnBackRequested;
            oldViewModel.ClearRequested -= OnClearRequested;
        }

        _viewModel = e.NewValue as BrowserViewModel;

        if (_viewModel is not null)
        {
            _viewModel.NavigateRequested += OnNavigateRequested;
            _viewModel.RefreshRequested += OnRefreshRequested;
            _viewModel.BackRequested += OnBackRequested;
            _viewModel.ClearRequested += OnClearRequested;
        }
    }

    private async Task EnsureBrowserInitializedAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        ConfigureCreationProperties();
        await BrowserControl.EnsureCoreWebView2Async();
        ConfigureBrowser();
        await BrowserControl.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(SafetyScript);
        AttachCoreEvents();
        _isInitialized = true;
    }

    private void ConfigureCreationProperties()
    {
        if (BrowserControl.CreationProperties is not null)
        {
            return;
        }

        var appDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FaciliteSenior",
            "WebView2");

        Directory.CreateDirectory(appDataRoot);

        BrowserControl.CreationProperties = new CoreWebView2CreationProperties
        {
            UserDataFolder = appDataRoot
        };
    }

    private void ConfigureBrowser()
    {
        if (BrowserControl.CoreWebView2 is null)
        {
            return;
        }

        var settings = BrowserControl.CoreWebView2.Settings;
        settings.AreDefaultContextMenusEnabled = false;
        settings.AreBrowserAcceleratorKeysEnabled = false;
        settings.AreDevToolsEnabled = false;
        settings.IsStatusBarEnabled = false;
        settings.IsZoomControlEnabled = false;
        settings.IsPinchZoomEnabled = false;
        settings.IsSwipeNavigationEnabled = false;

        // Enregistrement des mots de passe chiffre localement : le navigateur integre utilise deja
        // un profil persistant (UserDataFolder ci-dessus), on active donc explicitement le
        // gestionnaire de mots de passe et l'auto-remplissage natifs de WebView2/Chromium.
        // Le stockage est chiffre par Windows (DPAPI, lie au compte utilisateur) exactement comme
        // dans Edge ou Chrome : c'est la meme technologie eprouvee, pas un coffre-fort maison.
        settings.IsPasswordAutosaveEnabled = true;
        settings.IsGeneralAutofillEnabled = true;
    }

    private void AttachCoreEvents()
    {
        if (_eventsAttached || BrowserControl.CoreWebView2 is null)
        {
            return;
        }

        BrowserControl.NavigationStarting += OnNavigationStarting;
        BrowserControl.NavigationCompleted += OnNavigationCompleted;
        BrowserControl.SourceChanged += OnSourceChanged;
        BrowserControl.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        BrowserControl.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
        BrowserControl.CoreWebView2.HistoryChanged += OnHistoryChanged;
        BrowserControl.CoreWebView2.ProcessFailed += OnProcessFailed;
        _eventsAttached = true;
    }

    private async void OnNavigateRequested(object? sender, string url)
    {
        try
        {
            await NavigateToAsync(url);
        }
        catch (Exception ex)
        {
            ShowInitializationError(ex);
        }
    }

    private void OnRefreshRequested(object? sender, EventArgs e)
    {
        try
        {
            if (BrowserControl.CoreWebView2 is not null)
            {
                BrowserControl.Reload();
                return;
            }

            if (_viewModel is not null && !string.IsNullOrWhiteSpace(_viewModel.CurrentUrl))
            {
                _ = NavigateToAsync(_viewModel.CurrentUrl);
            }
        }
        catch (Exception ex)
        {
            ShowInitializationError(ex);
        }
    }

    private void OnBackRequested(object? sender, EventArgs e)
    {
        if (BrowserControl.CanGoBack)
        {
            BrowserControl.GoBack();
        }
    }

    private void OnClearRequested(object? sender, EventArgs e)
    {
        if (BrowserControl.CoreWebView2 is not null)
        {
            BrowserControl.CoreWebView2.Navigate("about:blank");
        }
    }

    private async Task NavigateToAsync(string url)
    {
        if (_viewModel is null || string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !IsSupportedScheme(uri))
        {
            _viewModel.ShowError("L'adresse du site n'est pas reconnue. Verifiez le lien dans les parametres aidant.");
            return;
        }

        try
        {
            await EnsureBrowserInitializedAsync();
            BrowserControl.Source = uri;
        }
        catch (Exception ex)
        {
            ShowInitializationError(ex);
        }
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) || !IsSupportedScheme(uri))
        {
            e.Cancel = true;
            _viewModel.ShowError("Cette action ne peut pas etre ouverte ici. Utilisez uniquement les liens prevus dans l'application.");
            return;
        }

        _viewModel.SetLoading(true, $"Chargement de {_viewModel.FriendlyName}...");
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (e.IsSuccess)
        {
            UpdateBrowserState();
            return;
        }

        _viewModel.ShowError(MapWebError(e.WebErrorStatus));
        UpdateBrowserState();
    }

    private async void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;

        if (string.IsNullOrWhiteSpace(e.Uri))
        {
            return;
        }

        try
        {
            await NavigateToAsync(e.Uri);
        }
        catch (Exception ex)
        {
            ShowInitializationError(ex);
        }
    }

    private void OnDocumentTitleChanged(object? sender, object e)
    {
        UpdateBrowserState();
    }

    private void OnHistoryChanged(object? sender, object e)
    {
        UpdateBrowserState();
    }

    private void OnSourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        UpdateBrowserState();
    }

    private void OnProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
    {
        _viewModel?.ShowError("La page a rencontre un probleme. Revenez a l'accueil puis reessayez.");
    }

    private void UpdateBrowserState()
    {
        if (_viewModel is null)
        {
            return;
        }

        _viewModel.UpdateNavigationState(
            BrowserControl.Source?.ToString() ?? string.Empty,
            BrowserControl.CanGoBack,
            BrowserControl.CoreWebView2?.DocumentTitle);
    }

    private void ShowInitializationError(Exception ex)
    {
        if (_viewModel is null)
        {
            return;
        }

        var message = ex is WebView2RuntimeNotFoundException
            ? "Microsoft WebView2 est manquant. Installez ce composant puis relancez l'application."
            : "Le navigateur integre ne peut pas demarrer pour le moment. Revenez a l'accueil puis reessayez.";

        _viewModel.ShowError(message);
    }

    private static bool IsSupportedScheme(Uri uri)
    {
        return uri.Scheme == Uri.UriSchemeHttp
               || uri.Scheme == Uri.UriSchemeHttps
               || uri.Scheme == "about";
    }

    private static string MapWebError(CoreWebView2WebErrorStatus status)
    {
        return status switch
        {
            CoreWebView2WebErrorStatus.HostNameNotResolved => "Le site n'a pas ete trouve. Verifiez votre connexion Internet.",
            CoreWebView2WebErrorStatus.Timeout => "La page met trop de temps a repondre. Reessayez dans un instant.",
            CoreWebView2WebErrorStatus.CannotConnect => "Connexion impossible pour le moment. Verifiez Internet puis reessayez.",
            CoreWebView2WebErrorStatus.ConnectionAborted => "La connexion a ete interrompue. Reessayez doucement.",
            CoreWebView2WebErrorStatus.ConnectionReset => "La connexion a ete coupee. Reessayez dans un instant.",
            CoreWebView2WebErrorStatus.Disconnected => "Internet semble indisponible. Verifiez la connexion puis reessayez.",
            CoreWebView2WebErrorStatus.ServerUnreachable => "Le site ne repond pas pour le moment. Reessayez plus tard.",
            CoreWebView2WebErrorStatus.CertificateExpired => "Le certificat du site pose probleme. Attendez puis contactez l'aidant si besoin.",
            CoreWebView2WebErrorStatus.CertificateIsInvalid => "La securite du site ne peut pas etre verifiee. Revenez a l'accueil et contactez l'aidant.",
            CoreWebView2WebErrorStatus.ValidAuthenticationCredentialsRequired => "Une identification est demandee sur ce site. Utilisez vos identifiants habituels.",
            _ => "La page ne peut pas etre affichee pour le moment. Revenez a l'accueil puis reessayez."
        };
    }

    private void OnBrowserPreviewDragOver(object sender, DragEventArgs e)
    {
        e.Handled = true;
    }

    private void OnBrowserPreviewDrop(object sender, DragEventArgs e)
    {
        e.Handled = true;
    }
}
