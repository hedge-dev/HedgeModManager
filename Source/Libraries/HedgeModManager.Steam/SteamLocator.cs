namespace HedgeModManager.Steam;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Foundation;
using System.IO;

public class SteamLocator : IGameLocator
{
    public string? SteamInstallPath { get; protected set; }

    [SupportedOSPlatform("windows")]
    private string? FindSteamLibraryWindows()
    {
        var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey("SOFTWARE\\Wow6432Node\\Valve\\Steam") ??
                  RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey("SOFTWARE\\Valve\\Steam");

        if (key != null)
        {
            var path = key.GetValue("InstallPath")?.ToString();
            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path))
                {
                    return path;
                }

                Logger.Warning($"Steam path found, but \"{path}\" does not exist!");
            }
        }

        return null;
    }

    private string? FindSteamLibraryUnix()
    {
        var pathList = new[]
        {
            Path.Combine(".steam", "steam"),
            Path.Combine(".local", "share", "Steam"),
            Path.Combine(".var", "app", "com.valvesoftware.Steam", ".steam", "steam"),
            Path.Combine("snap", "steam", "common", ".steam", "steam"),
        };

        foreach (string path in pathList)
        {
            string steamPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path);
            // Due to lack of permissions with flatpak, we will check for a file we need
            if (File.Exists(Path.Combine(steamPath, "steamapps", "libraryfolders.vdf")))
                return steamPath;
        }
        return null;
    }

    public string? FindDefaultSteamLibrary()
    {
        if (SteamInstallPath == null)
        {
            if (OperatingSystem.IsWindows())
            {
                SteamInstallPath = FindSteamLibraryWindows();
                return SteamInstallPath;
            }

            SteamInstallPath = FindSteamLibraryUnix();
            return SteamInstallPath;
        }

        return SteamInstallPath;
    }

    public List<SteamGame> Locate()
    {
        var games = new List<SteamGame>();
        var library = FindDefaultSteamLibrary();
        if (string.IsNullOrEmpty(library))
        {
            return games;
        }

        var folders = ValveDataFile.FromFile(Path.Combine(library, "steamapps", "libraryfolders.vdf"));

        foreach (var folder in folders)
        {
            var path = folder.GetCaseInsensitive("path").GetString();
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var apps = folder.GetCaseInsensitive("apps");
            if (apps == null)
            {
                continue;
            }

            var libPath = Path.Combine(path, "steamapps");
            foreach (var app in apps)
            {
                var appid = app.Name;
                if (string.IsNullOrEmpty(appid))
                {
                    continue;
                }

                try
                {
                    var manifest = ValveDataFile.FromFile(Path.Combine(libPath, $"appmanifest_{appid}.acf"));
                    var name = manifest.GetCaseInsensitive("name").GetString();
                    var installDir = manifest.GetCaseInsensitive("installdir").GetString();
                    var root = Path.Combine(libPath, "common", installDir);
                    if (Directory.Exists(root))
                    {
                        games.Add(new SteamGame
                        {
                            ID = appid,
                            Name = name,
                            Root = root,
                            PrefixRoot = Path.Combine(library, "steamapps", "compatdata", appid, "pfx")
                        });
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        return games;
    }

    IReadOnlyList<IGame> IGameLocator.Locate() => Locate();
}