using System.Collections.ObjectModel;
using System.Windows;
using FaciliteSenior.Commands;
using FaciliteSenior.Models;

namespace FaciliteSenior.ViewModels;

public sealed class DocumentsViewModel : ViewModelBase
{
    private string _emptyMessage = "Aucun lien n'est encore configure.";
    private int _columns = 2;
    private double _cardScale = 1.0;
    private bool _showCardIcons = true;
    private bool _showCardLabels = true;
    private bool _showCardDescriptions = true;
    private bool _isCompactLayout;

    public ObservableCollection<ActionTileViewModel> Links { get; } = new();

    public string EmptyMessage
    {
        get => _emptyMessage;
        private set => SetProperty(ref _emptyMessage, value);
    }

    public bool HasLinks => Links.Count > 0;

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

    public double TileMinHeight => Math.Round((ShowCardDescriptions ? 156 : 112) * CardScale * (IsCompactLayout ? 0.82 : 1.0));

    public double TileWidth => Math.Round((IsCompactLayout ? 220 : 300) * CardScale);

    public Thickness TilePadding => IsCompactLayout
        ? new Thickness(16 * CardScale, 14 * CardScale, 16 * CardScale, 14 * CardScale)
        : new Thickness(22 * CardScale, 20 * CardScale, 22 * CardScale, 20 * CardScale);

    public Thickness TileMargin => IsCompactLayout
        ? new Thickness(8)
        : new Thickness(12);

    public double IconFontSize => Math.Round((IsCompactLayout ? 26 : 34) * CardScale);

    public double LabelFontSize => Math.Round((IsCompactLayout ? 20 : 28) * CardScale);

    public double DescriptionFontSize => Math.Round((IsCompactLayout ? 14 : 18) * CardScale);

    public void Load(IEnumerable<FavoriteLink> favorites, Action<FavoriteLink> openFavorite, DisplayPreferences preferences)
    {
        Links.Clear();
        ApplyDisplayPreferences(preferences);

        foreach (var favorite in favorites.Where(link => link.ShowInDocuments).OrderBy(link => link.Label))
        {
            Links.Add(new ActionTileViewModel(
                favorite.Label,
                string.IsNullOrWhiteSpace(favorite.Description) ? "Ouvrir ce site dans l'application." : favorite.Description,
                "\uE71B",
                new RelayCommand(_ => openFavorite(favorite)),
                badgeKey: "Info"));
        }

        EmptyMessage = HasLinks
            ? "Choisissez un site important."
            : "Ajoutez des liens dans les parametres aidant pour remplir cette page.";

        OnPropertyChanged(nameof(HasLinks));
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
