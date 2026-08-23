namespace FaciliteSenior.Models;

public sealed class DisplayPreferences
{
    public double CardScale { get; set; } = 1.0;

    public bool ShowCardIcons { get; set; } = true;

    public bool ShowCardLabels { get; set; } = true;

    public bool ShowCardDescriptions { get; set; } = true;
}
