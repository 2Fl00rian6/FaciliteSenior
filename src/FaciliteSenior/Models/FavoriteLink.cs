namespace FaciliteSenior.Models;

public sealed class FavoriteLink
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public bool ShowOnHome { get; set; }

    public bool ShowInDocuments { get; set; } = true;

    public bool ConfirmBeforeOpen { get; set; } = true;
}
