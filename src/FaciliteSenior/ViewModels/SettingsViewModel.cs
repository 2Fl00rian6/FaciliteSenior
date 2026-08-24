using System.Collections.ObjectModel;
using System.Windows.Input;
using FaciliteSenior.Commands;
using FaciliteSenior.Models;

namespace FaciliteSenior.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private string _applicationTitle = "Facilite";
    private double _interfaceScale = 1.0;
    private double _cardScale = 1.0;
    private bool _fullScreenOnStartup = true;
    private bool _startWithWindows = true;
    private bool _showCardIcons = true;
    private bool _showCardLabels = true;
    private bool _showCardDescriptions = true;
    private string _helperAccessCode = string.Empty;
    private string _helpTitle = "Besoin d'aide ?";
    private string _helpMessage = string.Empty;
    private string _helperName = "Votre aidant";
    private string _phoneNumber = "00 00 00 00 00";
    private string _configPath = string.Empty;

    public SettingsViewModel()
    {
        AddFavoriteCommand = new RelayCommand(_ => AddFavorite());
        SaveCommand = new RelayCommand(_ => SaveRequested?.Invoke(this, EventArgs.Empty));
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? SaveRequested;

    public event EventHandler? CancelRequested;

    public ObservableCollection<FavoriteEditorViewModel> Favorites { get; } = new();

    public ICommand AddFavoriteCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public string ApplicationTitle
    {
        get => _applicationTitle;
        set => SetProperty(ref _applicationTitle, value);
    }

    public double InterfaceScale
    {
        get => _interfaceScale;
        set
        {
            if (SetProperty(ref _interfaceScale, Math.Clamp(value, 0.7, 1.6)))
            {
                OnPropertyChanged(nameof(InterfaceScaleDisplay));
            }
        }
    }

    public string InterfaceScaleDisplay => $"{InterfaceScale:0.0}x";

    public double CardScale
    {
        get => _cardScale;
        set
        {
            if (SetProperty(ref _cardScale, Math.Clamp(value, 0.7, 1.5)))
            {
                OnPropertyChanged(nameof(CardScaleDisplay));
            }
        }
    }

    public string CardScaleDisplay => $"{CardScale:0.0}x";

    public bool FullScreenOnStartup
    {
        get => _fullScreenOnStartup;
        set => SetProperty(ref _fullScreenOnStartup, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
    }

    public bool ShowCardIcons
    {
        get => _showCardIcons;
        set => SetProperty(ref _showCardIcons, value);
    }

    public bool ShowCardLabels
    {
        get => _showCardLabels;
        set => SetProperty(ref _showCardLabels, value);
    }

    public bool ShowCardDescriptions
    {
        get => _showCardDescriptions;
        set => SetProperty(ref _showCardDescriptions, value);
    }

    public string HelperAccessCode
    {
        get => _helperAccessCode;
        set => SetProperty(ref _helperAccessCode, value);
    }

    public string HelpTitle
    {
        get => _helpTitle;
        set => SetProperty(ref _helpTitle, value);
    }

    public string HelpMessage
    {
        get => _helpMessage;
        set => SetProperty(ref _helpMessage, value);
    }

    public string HelperName
    {
        get => _helperName;
        set => SetProperty(ref _helperName, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public string ConfigPath
    {
        get => _configPath;
        private set => SetProperty(ref _configPath, value);
    }

    public void Load(AppSettings settings, string configPath)
    {
        ApplicationTitle = settings.ApplicationTitle;
        InterfaceScale = settings.InterfaceScale;
        CardScale = settings.DisplayPreferences.CardScale;
        FullScreenOnStartup = settings.FullScreenOnStartup;
        StartWithWindows = settings.StartWithWindows;
        ShowCardIcons = settings.DisplayPreferences.ShowCardIcons;
        ShowCardLabels = settings.DisplayPreferences.ShowCardLabels;
        ShowCardDescriptions = settings.DisplayPreferences.ShowCardDescriptions;
        HelperAccessCode = settings.HelperAccessCode;
        HelpTitle = settings.Help.Title;
        HelpMessage = settings.Help.Message;
        HelperName = settings.Help.HelperName;
        PhoneNumber = settings.Help.PhoneNumber;
        ConfigPath = configPath;

        Favorites.Clear();

        foreach (var favorite in settings.Favorites)
        {
            AddFavorite(FavoriteEditorViewModel.FromModel(favorite));
        }
    }

    public bool TryBuildSettings(out AppSettings settings, out string errorMessage)
    {
        settings = new AppSettings();
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ApplicationTitle))
        {
            errorMessage = "Le titre de l'application ne peut pas etre vide.";
            return false;
        }

        if (!ShowCardIcons && !ShowCardLabels && !ShowCardDescriptions)
        {
            errorMessage = "Laissez au moins un element visible sur les cartes : icone, titre ou description.";
            return false;
        }

        if (Favorites.Count == 0)
        {
            errorMessage = "Ajoutez au moins un lien favori.";
            return false;
        }

        var favorites = new List<FavoriteLink>();

        foreach (var favoriteEditor in Favorites)
        {
            if (string.IsNullOrWhiteSpace(favoriteEditor.Label))
            {
                errorMessage = "Chaque lien doit avoir un libelle clair.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(favoriteEditor.Url)
                && (!Uri.TryCreate(favoriteEditor.Url, UriKind.Absolute, out var uri)
                    || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
            {
                errorMessage = $"L'adresse du lien \"{favoriteEditor.Label}\" doit commencer par http:// ou https://.";
                return false;
            }

            favorites.Add(favoriteEditor.ToModel());
        }

        settings = new AppSettings
        {
            ApplicationTitle = ApplicationTitle.Trim(),
            InterfaceScale = InterfaceScale,
            FullScreenOnStartup = FullScreenOnStartup,
            StartWithWindows = StartWithWindows,
            HelperAccessCode = HelperAccessCode.Trim(),
            DisplayPreferences = new DisplayPreferences
            {
                CardScale = CardScale,
                ShowCardIcons = ShowCardIcons,
                ShowCardLabels = ShowCardLabels,
                ShowCardDescriptions = ShowCardDescriptions
            },
            Help = new HelpContent
            {
                Title = string.IsNullOrWhiteSpace(HelpTitle) ? "Besoin d'aide ?" : HelpTitle.Trim(),
                Message = string.IsNullOrWhiteSpace(HelpMessage) ? "En cas de probleme, appelle ton aidant." : HelpMessage.Trim(),
                HelperName = string.IsNullOrWhiteSpace(HelperName) ? "Votre aidant" : HelperName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? "00 00 00 00 00" : PhoneNumber.Trim()
            },
            Favorites = favorites
        };

        return true;
    }

    private void AddFavorite(FavoriteEditorViewModel? favorite = null)
    {
        var editor = favorite ?? new FavoriteEditorViewModel
        {
            Label = "Nouveau lien",
            Description = "Description courte",
            Url = "https://",
            ShowInDocuments = true
        };

        editor.RemoveRequested += OnFavoriteRemoveRequested;
        Favorites.Add(editor);
    }

    private void OnFavoriteRemoveRequested(object? sender, EventArgs e)
    {
        if (sender is FavoriteEditorViewModel editor)
        {
            editor.RemoveRequested -= OnFavoriteRemoveRequested;
            Favorites.Remove(editor);
        }
    }
}
