namespace HedgeModManager.Epic;
using Foundation;
using Microsoft.Win32;
using System.Runtime.Versioning;
using System.Text.Json;


public class EpicLocator : IGameLocator
{
    private readonly string HGLID = "com.heroicgameslauncher.hgl";
    private readonly JsonSerializerOptions _legendarySerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };
    private readonly JsonSerializerOptions _heroicSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public List<string> HeroicRootPaths = [];
    public string? EGLInstalledPath { get; set; } = null;

    [SupportedOSPlatform("windows")]
    public void LocateEGLRoot()
    {
        var eosKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Epic Games\\EOS");
        if (eosKey?.GetValue("ModSdkMetadataDir") is string modSdkMetadataDir)
        {
            string installedPath = Path.Combine(modSdkMetadataDir, "..", "..", "..", "UnrealEngineLauncher", "LauncherInstalled.dat");

            if (File.Exists(installedPath))
            {
                EGLInstalledPath = installedPath;
            }
        }
    }

    public void LocateHeroicRoots()
    {
        var searchPaths = new List<string>();
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (OperatingSystem.IsWindows())
        {
            // Heroic Games Launcher
            searchPaths.Add(Path.Combine(appdata, "heroic"));
            searchPaths.Add(Path.Combine("heroic"));
        }
        
        if (OperatingSystem.IsLinux())
        {
            // Heroic Games Launcher (Flatpak)
            searchPaths.Add(Path.Combine(userProfile, ".var", "app", HGLID, "config", "heroic"));
        }

        if (searchPaths.Count != 0)
        {
            HeroicRootPaths.Clear();
            searchPaths.Where(Directory.Exists).ToList().ForEach(HeroicRootPaths.Add);
        }
    }

    public List<EpicGame> Locate()
    {
        if (OperatingSystem.IsWindows())
        {
            LocateEGLRoot();
        }

        LocateHeroicRoots();
        var games = new List<EpicGame>();

        // Epic Games Launcher
        if (EGLInstalledPath != null)
        {
            var installedApps = JsonSerializer.Deserialize<EpicLauncherInstalled>(File.ReadAllText(EGLInstalledPath));
            if (installedApps != null)
            {
                foreach (var app in installedApps.InstallationList)
                {
                    games.Add(new EpicGame
                    {
                        ID = app.AppName ?? "NONE",
                        Name = app.AppName ?? "NONE",
                        Root = app.InstallLocation ?? "NONE",
                        NativeOS = "Windows"
                    });
                }
            }
        }

        foreach (string rootPath in HeroicRootPaths)
        {
            string installedPath = Path.Combine(rootPath, "legendaryConfig", "legendary", "installed.json");
            if (!File.Exists(installedPath))
                continue;

            var installed = JsonSerializer.Deserialize<Dictionary<string, LegendaryGame>>(File.ReadAllText(installedPath), _legendarySerializerOptions);
            if (installed != null)
            {
                foreach (var game in installed)
                {
                    games.Add(new EpicGame
                    {
                        ID = game.Value.AppName ?? "NONE",
                        Name = game.Value.Title ?? "NONE",
                        Root = game.Value.InstallPath ?? "NONE",
                        Executable = game.Value.Executable,
                        NativeOS = game.Value.Platform ?? "Windows",
                        PrefixRoot = GetPrefixPath(rootPath, game.Value.AppName ?? "NONE"),
                    });
                }
            }
        }

        return games;
    }

    private string? GetPrefixPath(string rootPath, string appName)
    {
        string gameConfigPath = Path.Combine(rootPath, "GamesConfig", $"{appName}.json");
        if (!File.Exists(gameConfigPath))
            return null;

        using var doc = JsonDocument.Parse(File.ReadAllText(gameConfigPath));
        if (doc.RootElement.TryGetProperty(appName, out var element))
        {
            var gameConfig = element.Deserialize<HeroicGameConfig>(_heroicSerializerOptions);
            if (gameConfig?.WinePrefix != null)
                return Path.Combine(gameConfig.WinePrefix, "pfx");
        }

        return null;
    }

    IReadOnlyList<IGame> IGameLocator.Locate() => Locate();

    public class LegendaryGame
    {
        public string? AppName { get; set; }
        public string? Executable { get; set; }
        public string? InstallPath { get; set; }
        public bool IsDlc { get; set; } = false;
        public string? LaunchParameters { get; set; }
        public string? Platform { get; set; }
        public string? Title { get; set; }
        public string? Version { get; set; }
    }

    public class HeroicGameConfig
    {
        public string? WinePrefix { get; set; }
    }
}