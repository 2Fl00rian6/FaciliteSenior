using System.IO;
using System.Text;
using System.Text.Json;
using FaciliteSenior.Models;

namespace FaciliteSenior.Services;

public sealed class ConfigService : IConfigService
{
    private const string UserDirectoryName = "FaciliteSenior";
    private const string UserFileName = "configuration.json";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public string DefaultConfigPath => Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    public string UserConfigPath
    {
        get
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, UserDirectoryName, UserFileName);
        }
    }

    public async Task<AppSettings> LoadAsync()
    {
        await EnsureUserConfigExistsAsync();

        var settings = await TryReadFileAsync(UserConfigPath)
                       ?? await TryReadFileAsync(DefaultConfigPath)
                       ?? new AppSettings();

        return Sanitize(settings);
    }

    public async Task SaveAsync(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(UserConfigPath)!);

        var sanitized = Sanitize(settings);
        var json = JsonSerializer.Serialize(sanitized, _jsonOptions);
        await File.WriteAllTextAsync(UserConfigPath, json, Encoding.UTF8);
    }

    private async Task EnsureUserConfigExistsAsync()
    {
        if (File.Exists(UserConfigPath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(UserConfigPath)!);

        if (File.Exists(DefaultConfigPath))
        {
            File.Copy(DefaultConfigPath, UserConfigPath, overwrite: false);
            return;
        }

        var fallbackJson = JsonSerializer.Serialize(new AppSettings(), _jsonOptions);
        await File.WriteAllTextAsync(UserConfigPath, fallbackJson, Encoding.UTF8);
    }

    private async Task<AppSettings?> TryReadFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, Encoding.UTF8);
            return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static AppSettings Sanitize(AppSettings settings)
    {
        settings.ApplicationTitle = string.IsNullOrWhiteSpace(settings.ApplicationTitle)
            ? "Facilite"
            : settings.ApplicationTitle.Trim();

        settings.InterfaceScale = Math.Clamp(settings.InterfaceScale, 0.7, 1.6);
        settings.DisplayPreferences ??= new DisplayPreferences();
        settings.DisplayPreferences.CardScale = Math.Clamp(settings.DisplayPreferences.CardScale, 0.7, 1.5);

        if (!settings.DisplayPreferences.ShowCardIcons
            && !settings.DisplayPreferences.ShowCardLabels
            && !settings.DisplayPreferences.ShowCardDescriptions)
        {
            settings.DisplayPreferences.ShowCardLabels = true;
        }

        settings.Help ??= new HelpContent();
        settings.Help.Title = string.IsNullOrWhiteSpace(settings.Help.Title) ? "Besoin d'aide ?" : settings.Help.Title.Trim();
        settings.Help.Message = string.IsNullOrWhiteSpace(settings.Help.Message)
            ? "En cas de probleme, appelle Florian."
            : settings.Help.Message.Trim();
        settings.Help.HelperName = string.IsNullOrWhiteSpace(settings.Help.HelperName) ? "Florian" : settings.Help.HelperName.Trim();
        settings.Help.PhoneNumber = string.IsNullOrWhiteSpace(settings.Help.PhoneNumber) ? "06 00 00 00 00" : settings.Help.PhoneNumber.Trim();

        settings.Favorites ??= new List<FavoriteLink>();

        foreach (var favorite in settings.Favorites)
        {
            favorite.Id = string.IsNullOrWhiteSpace(favorite.Id) ? Guid.NewGuid().ToString("N") : favorite.Id.Trim();
            favorite.Label = string.IsNullOrWhiteSpace(favorite.Label) ? "Lien" : favorite.Label.Trim();
            favorite.Description ??= string.Empty;
            favorite.Url ??= string.Empty;
        }

        return settings;
    }
}
