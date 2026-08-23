using System.Collections.ObjectModel;
using System.Windows;
using FaciliteSenior.Commands;
using FaciliteSenior.Models;

namespace FaciliteSenior.ViewModels;

public sealed class HomeViewModel : ViewModelBase
{
    private int _columns = 2;
    private double _cardScale = 1.0;
    private bool _showCardIcons = true;
    private bool _showCardLabels = true;
    private bool _showCardDescriptions = true;
    private bool _isCompactLayout;

    public ObservableCollection<ActionTileViewModel> Actions { get; } = new();

    public int Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, Math.Max(1, value));
    }

    public double CardScale
    {
        get => _cardScale;
        private set
        {
            if (SetProperty(ref _cardScale, value))
            {
                RaiseLayoutPropertiesChanged();
            }
        }
    }

    public bool ShowCardIcons
    {
        get => _showCardIcons;
        private set => SetProperty(ref _showCardIcons, value);
    }

    public bool ShowCardLabels
    {
        get => _showCardLabels;
        private set => SetProperty(ref _showCardLabels, value);
    }

    public bool ShowCardDescriptions
    {
        get => _showCardDescriptions;
        private set => SetProperty(ref _showCardDescriptions, value);
    }

    public bool IsCompactLayout
    {
        get => _isCompactLayout;
        set
        {
            if (SetProperty(ref _isCompactLayout, value))
            {
                RaiseLayoutPropertiesChanged();
            }
        }
    }

    public double TileMinHeight => Math.Round((ShowCardDescriptions ? 170 : 120) * CardScale * (IsCompactLayout ? 0.82 : 1.0));

    public double TileWidth => Math.Round((IsCompactLayout ? 240 : 320) * CardScale);

    public Thickness TilePadding => IsCompactLayout
        ? new Thickness(16 * CardScale, 14 * CardScale, 16 * CardScale, 14 * CardScale)
        : new Thickness(24 * CardScale, 22 * CardScale, 24 * CardScale, 22 * CardScale);

    public Thickness TileMargin => IsCompactLayout
        ? new Thickness(8)
        : new Thickness(12);

    public double IconFontSize => Math.Round((IsCompactLayout ? 30 : 44) * CardScale);

    public double LabelFontSize => Math.Round((IsCompactLayout ? 22 : 30) * CardScale);

    public double DescriptionFontSize => Math.Round((IsCompactLayout ? 15 : 18) * CardScale);

    public void Load(AppSettings settings, Action<FavoriteLink> openFavorite, Action openDocuments, Action openHelp, Action exitApplication)
    {
        Actions.Clear();

        ApplyDisplayPreferences(settings.DisplayPreferences);

        foreach (var favorite in settings.Favorites.Where(link => link.ShowOnHome).OrderBy(link => link.Label))
        {
            Actions.Add(new ActionTileViewModel(
                favorite.Label,
                string.IsNullOrWhiteSpace(favorite.Description) ? "Ouvrir cette page dans l'application." : favorite.Description,
                "\uE8A7",
                new RelayCommand(_ => openFavorite(favorite)),
                isAccent: true,
                badgeKey: "Accent"));
        }

        Actions.Add(new ActionTileViewModel(
            "Documents importants",
            "CAF, Ameli, Impots, photos et autres liens utiles.",
            "\uE130",
            new RelayCommand(_ => openDocuments()),
            badgeKey: "Info"));

        Actions.Add(new ActionTileViewModel(
            "Aide / Assistance",
            "Voir un message simple et le numero a appeler.",
            "\uE897",
            new RelayCommand(_ => openHelp()),
            badgeKey: "Help"));

        Actions.Add(new ActionTileViewModel(
            "Quitter",
            "Fermer l'application en toute securite.",
            "\uE711",
            new RelayCommand(_ => exitApplication()),
            isDanger: true,
            badgeKey: "Danger"));
    }

    private void ApplyDisplayPreferences(DisplayPreferences preferences)
    {
        CardScale = preferences.CardScale;
        ShowCardIcons = preferences.ShowCardIcons;
        ShowCardLabels = preferences.ShowCardLabels;
        ShowCardDescriptions = preferences.ShowCardDescriptions;
    }

    private void RaiseLayoutPropertiesChanged()
    {
        OnPropertyChanged(nameof(TileMinHeight));
        OnPropertyChanged(nameof(TileWidth));
        OnPropertyChanged(nameof(TilePadding));
        OnPropertyChanged(nameof(TileMargin));
        OnPropertyChanged(nameof(IconFontSize));
        OnPropertyChanged(nameof(LabelFontSize));
        OnPropertyChanged(nameof(DescriptionFontSize));
    }
}
