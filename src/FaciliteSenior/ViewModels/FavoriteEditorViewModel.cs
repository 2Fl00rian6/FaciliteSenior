using System.Windows.Input;
using FaciliteSenior.Commands;
using FaciliteSenior.Models;

namespace FaciliteSenior.ViewModels;

public sealed class FavoriteEditorViewModel : ViewModelBase
{
    private string _id = Guid.NewGuid().ToString("N");
    private string _label = string.Empty;
    private string _description = string.Empty;
    private string _url = string.Empty;
    private bool _showOnHome;
    private bool _showInDocuments = true;
    private bool _confirmBeforeOpen = true;

    public FavoriteEditorViewModel()
    {
        RemoveCommand = new RelayCommand(_ => RemoveRequested?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? RemoveRequested;

    public ICommand RemoveCommand { get; }

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Label
    {
        get => _label;
        set => SetProperty(ref _label, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public bool ShowOnHome
    {
        get => _showOnHome;
        set => SetProperty(ref _showOnHome, value);
    }

    public bool ShowInDocuments
    {
        get => _showInDocuments;
        set => SetProperty(ref _showInDocuments, value);
    }

    public bool ConfirmBeforeOpen
    {
        get => _confirmBeforeOpen;
        set => SetProperty(ref _confirmBeforeOpen, value);
    }

    public FavoriteLink ToModel()
    {
        return new FavoriteLink
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N") : Id.Trim(),
            Label = Label.Trim(),
            Description = Description.Trim(),
            Url = Url.Trim(),
            ShowOnHome = ShowOnHome,
            ShowInDocuments = ShowInDocuments,
            ConfirmBeforeOpen = ConfirmBeforeOpen
        };
    }

    public static FavoriteEditorViewModel FromModel(FavoriteLink favorite)
    {
        return new FavoriteEditorViewModel
        {
            Id = favorite.Id,
            Label = favorite.Label,
            Description = favorite.Description,
            Url = favorite.Url,
            ShowOnHome = favorite.ShowOnHome,
            ShowInDocuments = favorite.ShowInDocuments,
            ConfirmBeforeOpen = favorite.ConfirmBeforeOpen
        };
    }
}
