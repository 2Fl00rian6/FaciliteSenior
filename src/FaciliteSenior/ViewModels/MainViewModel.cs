using System.Windows.Input;
using FaciliteSenior.Commands;
using FaciliteSenior.Models;
using FaciliteSenior.Services;

namespace FaciliteSenior.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private enum ScreenKind
    {
        Home,
        Browser,
        Documents,
        Help,
        Settings
    }

    private readonly IConfigService _configService;
    private readonly IDialogService _dialogService;
    private readonly IWindowsStartupService _windowsStartupService;
    private readonly Stack<ScreenKind> _history = new();
    private readonly RelayCommand _backCommand;
    private readonly RelayCommand _refreshCommand;
    private readonly RelayCommand _closePageCommand;

    private AppSettings _currentSettings = new();
    private ViewModelBase _currentContent;
    private ScreenKind _currentScreen = ScreenKind.Home;
    private string _windowTitle = "Facilite Senior";
    private string _statusText = "Choisissez un grand bouton pour commencer.";
    private bool _isFullScreen;
    private double _interfaceScale = 1.0;

    public MainViewModel(IConfigService configService, IDialogService dialogService, IWindowsStartupService windowsStartupService)
    {
        _configService = configService;
        _dialogService = dialogService;
        _windowsStartupService = windowsStartupService;

        HomeViewModel = new HomeViewModel();
        BrowserViewModel = new BrowserViewModel();
        DocumentsViewModel = new DocumentsViewModel();
        HelpViewModel = new HelpViewModel();
        SettingsViewModel = new SettingsViewModel();

        _currentContent = HomeViewModel;

        ShowHomeCommand = new RelayCommand(_ => ShowHome());
        ShowDocumentsCommand = new RelayCommand(_ => ShowDocuments());
        ShowHelpCommand = new RelayCommand(_ => ShowHelp());
        _backCommand = new RelayCommand(_ => GoBack(), _ => CanGoBack());
        _refreshCommand = new RelayCommand(_ => BrowserViewModel.RefreshPage(), _ => _currentScreen == ScreenKind.Browser);
        _closePageCommand = new RelayCommand(_ => CloseCurrentPage(), _ => _currentScreen != ScreenKind.Home);
        ToggleFullScreenCommand = new RelayCommand(_ => ToggleFullScreen());
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        ExitCommand = new RelayCommand(_ => ConfirmAndExit());

        BrowserViewModel.PropertyChanged += OnBrowserPropertyChanged;
        SettingsViewModel.SaveRequested += OnSettingsSaveRequested;
        SettingsViewModel.CancelRequested += OnSettingsCancelRequested;
    }

    public event EventHandler<bool>? FullScreenRequested;

    public event EventHandler? RequestCloseApplication;

    public HomeViewModel HomeViewModel { get; }

    public BrowserViewModel BrowserViewModel { get; }

    public DocumentsViewModel DocumentsViewModel { get; }

    public HelpViewModel HelpViewModel { get; }

    public SettingsViewModel SettingsViewModel { get; }

    public ViewModelBase CurrentContent
    {
        get => _currentContent;
        private set => SetProperty(ref _currentContent, value);
    }

    public string WindowTitle
    {
        get => _windowTitle;
        private set => SetProperty(ref _windowTitle, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsFullScreen
    {
        get => _isFullScreen;
        private set
        {
            if (SetProperty(ref _isFullScreen, value))
            {
                OnPropertyChanged(nameof(FullScreenLabel));
                OnPropertyChanged(nameof(FullScreenIconGlyph));
            }
        }
    }

    public double InterfaceScale
    {
        get => _interfaceScale;
        private set => SetProperty(ref _interfaceScale, value);
    }

    public string FullScreenLabel => IsFullScreen ? "Mode fenetre" : "Plein ecran";

    public string FullScreenIconGlyph => IsFullScreen ? "" : "";

    public ICommand ShowHomeCommand { get; }

    public ICommand ShowDocumentsCommand { get; }

    public ICommand ShowHelpCommand { get; }

    /// <summary>Etat de selection pour la barre laterale (mise en surbrillance de l'onglet actif).</summary>
    public bool IsHomeSelected => _currentScreen == ScreenKind.Home;

    public bool IsDocumentsSelected => _currentScreen == ScreenKind.Documents;

    public bool IsHelpSelected => _currentScreen == ScreenKind.Help;

    public bool IsSettingsSelected => _currentScreen == ScreenKind.Settings;

    public ICommand BackCommand => _backCommand;

    public ICommand RefreshCommand => _refreshCommand;

    public ICommand ClosePageCommand => _closePageCommand;

    public ICommand ToggleFullScreenCommand { get; }

    public ICommand OpenSettingsCommand { get; }

    public ICommand ExitCommand { get; }

    public async Task InitializeAsync()
    {
        var settings = await _configService.LoadAsync();
        ApplySettings(settings);
        _windowsStartupService.ApplyStartupSetting(_currentSettings.StartWithWindows);
        ShowHome();

        IsFullScreen = settings.FullScreenOnStartup;
        FullScreenRequested?.Invoke(this, IsFullScreen);
    }

    private void ApplySettings(AppSettings settings)
    {
        _currentSettings = settings.DeepClone();
        WindowTitle = _currentSettings.ApplicationTitle;
        InterfaceScale = _currentSettings.InterfaceScale;

        HomeViewModel.Load(_currentSettings, OpenFavorite, ShowDocuments, ShowHelp, ConfirmAndExit);
        DocumentsViewModel.Load(_currentSettings.Favorites, OpenFavorite, _currentSettings.DisplayPreferences);
        HelpViewModel.Load(_currentSettings.Help);
        SettingsViewModel.Load(_currentSettings.DeepClone(), _configService.UserConfigPath);

        UpdateStatusForCurrentScreen();
        UpdateCommandStates();
    }

    private void ShowHome()
    {
        NavigateTo(ScreenKind.Home, addCurrentToHistory: false, clearHistory: true);
    }

    private void ShowDocuments()
    {
        NavigateTo(ScreenKind.Documents);
    }

    private void ShowHelp()
    {
        NavigateTo(ScreenKind.Help);
    }

    private void OpenFavorite(FavoriteLink favorite)
    {
        if (string.IsNullOrWhiteSpace(favorite.Url))
        {
            _dialogService.ShowMessage(
                "Lien a configurer",
                $"Le lien \"{favorite.Label}\" n'est pas encore renseigne. Ouvrez les parametres aidant pour ajouter la bonne adresse.");
            return;
        }

        if (!Uri.TryCreate(favorite.Url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            _dialogService.ShowMessage(
                "Adresse incorrecte",
                $"Le lien \"{favorite.Label}\" n'est pas valide. Corrigez l'adresse dans les parametres aidant.");
            return;
        }

        if (favorite.ConfirmBeforeOpen
            && !_dialogService.Confirm(
                $"Ouvrir {favorite.Label}",
                $"Voulez-vous ouvrir {favorite.Label} dans l'application ?",
                "Ouvrir",
                "Annuler"))
        {
            return;
        }

        BrowserViewModel.OpenFavorite(favorite);
        NavigateTo(ScreenKind.Browser);
    }

    private void OpenSettings()
    {
        if (!_dialogService.RequestAccessCode(
                _currentSettings.HelperAccessCode,
                "Parametres aidant",
                "Entrez le code d'acces pour modifier les liens et les reglages."))
        {
            _dialogService.ShowMessage("Acces refuse", "Le code saisi n'est pas correct.");
            return;
        }

        SettingsViewModel.Load(_currentSettings.DeepClone(), _configService.UserConfigPath);
        NavigateTo(ScreenKind.Settings);
    }

    private async void OnSettingsSaveRequested(object? sender, EventArgs e)
    {
        if (!SettingsViewModel.TryBuildSettings(out var settings, out var errorMessage))
        {
            _dialogService.ShowMessage("Verification necessaire", errorMessage);
            return;
        }

        await _configService.SaveAsync(settings);
        ApplySettings(settings);
        _windowsStartupService.ApplyStartupSetting(_currentSettings.StartWithWindows);
        ShowHome();

        IsFullScreen = settings.FullScreenOnStartup;
        FullScreenRequested?.Invoke(this, IsFullScreen);

        _dialogService.ShowMessage("Parametres enregistres", "Les changements ont bien ete pris en compte.");
    }

    private void OnSettingsCancelRequested(object? sender, EventArgs e)
    {
        ShowHome();
    }

    private void GoBack()
    {
        if (_currentScreen == ScreenKind.Browser && BrowserViewModel.CanGoBack)
        {
            BrowserViewModel.GoBackInPage();
            return;
        }

        if (_history.Count == 0)
        {
            ShowHome();
            return;
        }

        var previousScreen = _history.Pop();
        NavigateTo(previousScreen, addCurrentToHistory: false, clearHistory: false);
    }

    private void CloseCurrentPage()
    {
        ShowHome();
    }

    private void ToggleFullScreen()
    {
        IsFullScreen = !IsFullScreen;
        FullScreenRequested?.Invoke(this, IsFullScreen);
    }

    private void ConfirmAndExit()
    {
        var confirmed = _dialogService.Confirm(
            "Fermer l'application",
            "Voulez-vous vraiment fermer l'application ?",
            "Fermer",
            "Rester ici");

        if (confirmed)
        {
            RequestCloseApplication?.Invoke(this, EventArgs.Empty);
        }
    }

    private void NavigateTo(ScreenKind target, bool addCurrentToHistory = true, bool clearHistory = false)
    {
        if (clearHistory)
        {
            _history.Clear();
        }
        else if (addCurrentToHistory && _currentScreen != target)
        {
            _history.Push(_currentScreen);
        }

        var leavingBrowser = _currentScreen == ScreenKind.Browser && target != ScreenKind.Browser;

        _currentScreen = target;
        CurrentContent = ResolveContent(target);

        if (leavingBrowser)
        {
            BrowserViewModel.ClearPage();
        }

        OnPropertyChanged(nameof(IsHomeSelected));
        OnPropertyChanged(nameof(IsDocumentsSelected));
        OnPropertyChanged(nameof(IsHelpSelected));
        OnPropertyChanged(nameof(IsSettingsSelected));

        UpdateStatusForCurrentScreen();
        UpdateCommandStates();
    }

    private ViewModelBase ResolveContent(ScreenKind screen)
    {
        return screen switch
        {
            ScreenKind.Browser => BrowserViewModel,
            ScreenKind.Documents => DocumentsViewModel,
            ScreenKind.Help => HelpViewModel,
            ScreenKind.Settings => SettingsViewModel,
            _ => HomeViewModel
        };
    }

    private bool CanGoBack()
    {
        return (_currentScreen == ScreenKind.Browser && BrowserViewModel.CanGoBack) || _history.Count > 0;
    }

    private void UpdateStatusForCurrentScreen()
    {
        StatusText = _currentScreen switch
        {
            ScreenKind.Home => "Choisissez un grand bouton. Tout reste dans cette application.",
            ScreenKind.Documents => "Les liens importants s'ouvrent ici, sans navigateur externe.",
            ScreenKind.Help => HelpViewModel.CallSummary,
            ScreenKind.Settings => "Page reservee a l'aidant pour regler les liens et la lisibilite.",
            ScreenKind.Browser => BrowserViewModel.StatusMessage,
            _ => StatusText
        };
    }

    private void UpdateCommandStates()
    {
        _backCommand.RaiseCanExecuteChanged();
        _refreshCommand.RaiseCanExecuteChanged();
        _closePageCommand.RaiseCanExecuteChanged();
    }

    private void OnBrowserPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BrowserViewModel.CanGoBack))
        {
            UpdateCommandStates();
        }

        if (_currentScreen == ScreenKind.Browser
            && e.PropertyName is nameof(BrowserViewModel.StatusMessage) or nameof(BrowserViewModel.PageTitle))
        {
            UpdateStatusForCurrentScreen();
        }
    }
}
