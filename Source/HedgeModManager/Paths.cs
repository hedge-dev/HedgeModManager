using HedgeModManager.Foundation;
using System.Reflection;
using System.Runtime.Versioning;

namespace HedgeModManager;

public static class Paths
{
    private static Assembly HMMAssembly = typeof(Paths).Assembly;
    private static string? ProgramPath = null;
    private static bool IsFlatpak => Environment.GetEnvironmentVariable("FLATPAK_ID") != null;

    public static string GetProgramPath()
    {
        if (ProgramPath != null)
            return ProgramPath;

        var companyAttribute = HMMAssembly.GetCustomAttribute<AssemblyCompanyAttribute>();
        var productAttribute = HMMAssembly.GetCustomAttribute<AssemblyProductAttribute>();
        string companyName = companyAttribute?.Company ?? "hedge-dev";
        string productName = productAttribute?.Product ?? "HedgeModManager";
        return ProgramPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            companyName, productName);
    }

    public static void CreateDirectories()
    {
        Directory.CreateDirectory(GetProgramPath());
        Directory.CreateDirectory(GetCachePath());
        Directory.CreateDirectory(GetConfigPath());
        Directory.CreateDirectory(GetTempPath());
    }

    public static string GetCachePath()
    {
        return Path.Combine(GetProgramPath(), "Cache");
    }

    public static string GetConfigPath()
    {
        return Path.Combine(GetProgramPath(), "Config");
    }

    public static string GetTempPath()
    {
        return Path.Combine(GetProgramPath(), "Temp");
    }

    public static string GetUniqueTempPath()
    {
        return Path.Combine(GetTempPath(), Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Gets a list of shared program data directories<br/>
    /// <br/>
    /// This does not include the user's data directory<br/>
    /// <br/>
    /// Linux only<br/>
    /// </summary>
    [SupportedOSPlatform("linux")]
    public static string[] GetProgramDataPaths()
    {
        List<string> paths = ["/usr/local/share", "/usr/share"];

        try
        {
            if (Environment.GetEnvironmentVariable("XDG_DATA_DIRS") is string dirs && !string.IsNullOrEmpty(dirs))
                paths = [.. dirs.Split(":", StringSplitOptions.RemoveEmptyEntries)];
        }
        catch
        {
            Logger.Error("Failed to read or parse $XDG_DATA_DIRS");
        }

        // Also add host paths if running in Flatpak
        if (IsFlatpak)
        {
            List<string> newPaths = [];
            foreach (var path in paths)
            {
                if (!path.StartsWith("/run/host"))
                    newPaths.Add(Path.Combine("/run/host", path));
            }
            paths.AddRange(newPaths);
        }

        return [.. paths];
    }

    /// <summary>
    /// Gets the user's home directory<br/>
    /// <br/>
    /// On Windows, this is %USERPROFILE%<br/>
    /// On Linux, this is $HOME<br/>
    /// </summary>
    public static string GetUserHomePath()
    {
        if (Environment.GetEnvironmentVariable("HOME") is string home)
            return home;

        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    /// <summary>
    /// Gets the user's data directory<br/>
    /// <br/>
    /// On Windows, this is %LOCALAPPDATA%<br/>
    /// On Linux, this is $XDG_DATA_HOME or ~/.local/share<br/>
    /// </summary>
    public static string GetUserDataPath()
    {
        return GetUserPath("DATA", Path.Combine(".local", "share"), Environment.SpecialFolder.LocalApplicationData);
    }

    /// <summary>
    /// Gets the user's config directory<br/>
    /// <br/>
    /// On Windows, this is %APPDATA%<br/>
    /// On Linux, this is $XDG_CONFIG_HOME or ~/.config<br/>
    /// </summary>
    public static string GetUserConfigPath()
    {
        return GetUserPath("CONFIG", ".config", Environment.SpecialFolder.ApplicationData);
    }

    public static string GetUserPath(string xdgName, string? fromHome, Environment.SpecialFolder specialFolder)
    {
        if (Environment.GetEnvironmentVariable($"HOST_XDG_{xdgName}") is string hostDir && !string.IsNullOrEmpty(hostDir))
            return hostDir;

        if (!IsFlatpak && Environment.GetEnvironmentVariable($"XDG_{xdgName}") is string dir && !string.IsNullOrEmpty(dir))
            return dir;

        if (!string.IsNullOrEmpty(fromHome) && Environment.GetEnvironmentVariable("HOME") is string home)
            return Path.Combine(home, fromHome);

        return Environment.GetFolderPath(specialFolder);
    }
}
