namespace HedgeModManager.Steam;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Foundation;
using System.IO;
using System.Collections.Generic;

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

    public static string? LocateSteamPrefix(string appid, IReadOnlyList<string> compatDirs)
    {
        if (!OperatingSystem.IsLinux())
            return null;

        foreach (var compatDir in compatDirs)
        {
            var protonDir = Path.Combine(compatDir, appid);
            if (Directory.Exists(protonDir))
            {
                return Path.Combine(protonDir, "pfx");
            }
        }

        if (compatDirs.Count > 0)
        {
            Logger.Debug($"Using default prefix for appid {appid}");
            return Path.Combine(compatDirs[0], appid, "pfx");
        }

        Logger.Warning($"Could not locate Steam prefix for appid {appid}");
        return null;
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
        var compatDirs = new List<string>();

        if (OperatingSystem.IsLinux())
        {
            var compatDataDir = Path.Combine(library, "steamapps", "compatdata");
            if (Directory.Exists(compatDataDir))
            {
                compatDirs.Add(compatDataDir);
            }
        }

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

            // Paths will be encoded by Windows steam and will need to be adapted for macOS Paths.
            // C:\ is equivalent to the Wine prefix.
            // Any other drive letter should be removed and treated as raw Unix path.
            if (OperatingSystem.IsMacOS())
            {
                if (path.StartsWith(@"C:\"))
                {
                    path = SteamInstallPath;
                }
                else
                {
                    // Remove drive letter and replace slashes
                    path = path[2..].Replace(@"\", "/");
                }
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
                            PrefixRoot = LocateSteamPrefix(appid, compatDirs)
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