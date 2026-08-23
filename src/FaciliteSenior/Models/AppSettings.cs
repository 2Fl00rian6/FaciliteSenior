using System.Text.Json;

namespace FaciliteSenior.Models;

public sealed class AppSettings
{
    public string ApplicationTitle { get; set; } = "Facilite Senior";

    public double InterfaceScale { get; set; } = 1.0;

    public bool FullScreenOnStartup { get; set; } = true;

    public bool StartWithWindows { get; set; } = true;

    public string HelperAccessCode { get; set; } = "2580";

    public DisplayPreferences DisplayPreferences { get; set; } = new();

    public HelpContent Help { get; set; } = new();

    public List<FavoriteLink> Favorites { get; set; } = new();

    public AppSettings DeepClone()
    {
        var json = JsonSerializer.Serialize(this);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }
}
