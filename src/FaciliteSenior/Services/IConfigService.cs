using FaciliteSenior.Models;

namespace FaciliteSenior.Services;

public interface IConfigService
{
    string DefaultConfigPath { get; }

    string UserConfigPath { get; }

    Task<AppSettings> LoadAsync();

    Task SaveAsync(AppSettings settings);
}
