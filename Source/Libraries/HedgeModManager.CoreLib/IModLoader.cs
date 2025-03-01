using HedgeModManager.Foundation;

namespace HedgeModManager.CoreLib;

public interface IModLoader
{
    public string Name { get; }
    public IModdableGame Game { get; }

    public bool IsInstalled();
    public string? GetInstalledVersion();
    public Task<bool> CheckForUpdatesAsync();
    public Task<bool> InstallAsync();
    public Task<bool> UninstallAsync();
}
