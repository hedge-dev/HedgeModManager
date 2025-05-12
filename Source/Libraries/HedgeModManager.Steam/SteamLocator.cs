namespace HedgeModManager.Steam;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Foundation;
using System.IO;
using System.Collections.Generic;
using ValveKeyValue;

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

    /// <summary>
    /// Searches for appmanifest_{appid}.acf files and returns a list of parsed manifests
    /// </summary>
    /// <param name="library">Path to library, which must contain the steamapps directory</param>
    /// <returns>List of manifests as KVObject</returns>
    public static List<KVObject> ScanAppsInLibrary(string library)
    {
        var foundManifests = new List<KVObject>();
        var steamAppsDir = Path.Combine(library, "steamapps");
        if (!Directory.Exists(steamAppsDir))
        {
            return foundManifests;
        }

        foreach (var file in Directory.GetFiles(steamAppsDir, "appmanifest_*.acf"))
        {
            try
            {
                var manifest = ValveDataFile.FromFile(file);
                if (manifest != null)
                {
                    foundManifests.Add(manifest);
                    continue;
                }
            }
            catch
            {
                Logger.Error($"Failed to parse manifest file: {file}");
            }
        }
        return foundManifests;
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

        foreach (var folder in folders)
        {
            var path = folder.GetCaseInsensitive("path").GetString();
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (OperatingSystem.IsLinux())
            {
                var compatDataDir = Path.Combine(path, "steamapps", "compatdata");
                if (Directory.Exists(compatDataDir))
                {
                    compatDirs.Add(compatDataDir);
                }
            }
        }

        foreach (var folder in folders)
        {
            var path = folder.GetCaseInsensitive("path").GetString();
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var manifests = ScanAppsInLibrary(path);
            var libraryAppsPath = Path.Combine(path, "steamapps");
            foreach (var manifest in manifests)
            {
                try
                {
                    var appid = manifest.GetCaseInsensitive("appid").GetString();
                    var name = manifest.GetCaseInsensitive("name").GetString();
                    var installDir = manifest.GetCaseInsensitive("installdir").GetString();
                    var root = Path.Combine(libraryAppsPath, "common", installDir);
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
                    Logger.Error($"Failed to read manifest file: {manifest}");
                }
            }
        }

        return games;
    }

    IReadOnlyList<IGame> IGameLocator.Locate() => Locate();
}