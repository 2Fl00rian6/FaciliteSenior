using System.Windows.Input;

namespace FaciliteSenior.ViewModels;

public sealed class ActionTileViewModel
{
    public ActionTileViewModel(
        string label,
        string description,
        string iconGlyph,
        ICommand command,
        bool isAccent = false,
        bool isDanger = false,
        string badgeKey = "Neutral")
    {
        Label = label;
        Description = description;
        IconGlyph = iconGlyph;
        Command = command;
        IsAccent = isAccent;
        IsDanger = isDanger;
        BadgeKey = badgeKey;
    }

    public string Label { get; }

    public string Description { get; }

    public string IconGlyph { get; }

    public ICommand Command { get; }

    public bool IsAccent { get; }

    public bool IsDanger { get; }

    /// <summary>
    /// Cle logique utilisee par la vue pour choisir la couleur du badge d'icone
    /// (Accent, Info, Help, Danger, Neutral).
    /// </summary>
    public string BadgeKey { get; }
}
