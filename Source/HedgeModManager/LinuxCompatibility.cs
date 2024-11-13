namespace HedgeModManager;
using HedgeModManager.Foundation;
using HedgeModManager.Properties;
using HedgeModManager.Steam;
using System.IO.Compression;
using System.IO.Pipes;

public class LinuxCompatibility
{
    /// <summary>
    /// Finds the WINE prefix for the game
    /// </summary>
    /// <param name="game"></param>
    /// <returns>Full path to the prefix root</returns>
    public static string? GetPrefix(IGame game)
    {
        if (game.Platform != "Steam")
        {
            return null; // Only Steam games are supported
        }

        var locator = new SteamLocator();
        string? steamRoot = locator.FindDefaultSteamLibrary();
        if (steamRoot == null)
        {
            return null;
        }

        return Path.Combine(steamRoot, "steamapps", "compatdata", game.ID, "pfx");
    }

    /// <summary>
    /// Installs the .NET runtime to the prefix
    /// </summary>
    /// <param name="path">Path to the prefix root directory</param>
    public static bool InstallRuntimeToPrefix(string? path)
    {
        Logger.Debug($"Installing .NET runtime to {path}");
        if (path == null)
        {
            return false;
        }

        string cDrive = Path.Combine(path, "drive_c");
        using var stream = new MemoryStream(Resources.dotnetFrameworkRuntime);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            string destinationPath = Path.Combine(cDrive, entry.FullName);

            if (entry.FullName.EndsWith('/'))
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            // Try delete first
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            entry.ExtractToFile(destinationPath, true);
        }

        Logger.Debug($"Extracted .NET runtime");
        return true;
    }

    public static async Task<bool> AddDllOverride(string? path, string name)
    {
        Logger.Debug($"Adding DLL override {name} to {path}");
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            Logger.Debug($"Prefix is missing!");
            return false;
        }

        string reg = Path.Combine(path!, "user.reg");

        if (!Path.Exists(reg))
        {
            Logger.Debug($"Prefix is not initialised!");
            return false;
        }

        string dllOverrides = $"[Software\\\\Wine\\\\DllOverrides]\n\"{name}\"=\"native,builtin\"\n";
        await File.AppendAllTextAsync(reg, dllOverrides);
        Logger.Debug($"File written");
        return true;
    }

    public static bool IsPrefixValid(string? path)
    {
        if (path == null)
        {
            return false;
        }

        return Directory.Exists(path);
    }
}
