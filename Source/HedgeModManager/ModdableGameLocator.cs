namespace HedgeModManager;
using CoreLib;
using Foundation;
using HedgeModManager.Epic;
using HedgeModManager.Properties;
using HedgeModManager.Text;
using Microsoft.Win32;
using Steam;
using System.IO;
using System.Linq;

public class ModdableGameLocator
{
    public static readonly List<GameInfo> ModdableGameList =
    [
        new()
        {
            ID = "SonicGenerations",
            ModLoaderName = "HE1ModLoader",
            ModLoaderFileName = "d3d9.dll",
            ModLoaderIncompatibleFileNames = [ "dinput8.dll" ],
            ModLoaderDownloadURL = Resources.HE1MLDownloadURL,
            Is64Bit = false,
            PlatformInfos = new()
            {
                { "Steam", [new ("71340", "SonicGenerations.exe")] }
            }
        },
        new()
        {
            ID = "SonicLostWorld",
            ModLoaderName = "HE1ModLoader",
            ModLoaderFileName = "d3d9.dll",
            ModLoaderIncompatibleFileNames = [ "dinput8.dll" ],
            ModLoaderDownloadURL = Resources.HE1MLDownloadURL,
            Is64Bit = false,
            PlatformInfos = new() { { "Steam", [new ("329440", "slw.exe")] } }
        },
        new()
        {
            ID = "SonicForces",
            ModLoaderName = "HE2ModLoader",
            ModLoaderFileName = "d3d11.dll",
            ModLoaderIncompatibleFileNames = [ "dinput8.dll" ],
            ModLoaderDownloadURL = Resources.HE2MLDownloadURL,
            PlatformInfos = new() { { "Steam", [new ("637100", Path.Combine("build", "main", "projects", "exec", "Sonic Forces.exe"))] } }
        },
        new()
        {
            ID = "PuyoPuyoTetris2",
            ModLoaderName = "HE2ModLoader",
            ModLoaderFileName = "dinput8.dll",
            ModLoaderDownloadURL = Resources.HE2MLDownloadURL,
            PlatformInfos = new() { { "Steam", [new("1259790", "PuyoPuyoTetris2.exe")] } }
        },
        new()
        {
            ID = "Tokyo2020",
            ModLoaderName = "HE2ModLoader",
            ModLoaderFileName = "dinput8.dll",
            ModLoaderDownloadURL = Resources.HE2MLDownloadURL,
            PlatformInfos = new() { { "Steam", [new("981890", "musashi.exe")] } }
        },
        new()
        {
            ID = "SonicColorsUltimate",
            ModLoaderName = "RainbowModLoader",
            ModLoaderFileName = "d3d11.dll",
            ModLoaderDownloadURL = Resources.RMLDownloadURL,
            PlatformInfos = new()
            {
                { "Steam", [new("2055290", Path.Combine("exec", "SonicColorsUltimate.exe"))] },
                { "Epic", [new ("e5071e19d08c45a6bdda5d92fbd0a03e", Path.Combine("rainbow Shipping", "Sonic Colors - Ultimate.exe"))] }
            }
        },
        new()
        {
            ID = "SonicOrigins",
            ModLoaderName = "HiteModLoader",
            ModLoaderFileName = "dinput8.dll",
            ModLoaderDownloadURL = Resources.HMLDownloadURL,
            PlatformInfos = new()
            {
                { "Steam", [new("1794960", Path.Combine("build", "main", "projects", "exec", "SonicOrigins.exe"))] },
                { "Epic", [new ("5070a8e44cf74ba3b9a4ca0c0dce5cf1", Path.Combine("build", "main", "projects", "exec", "SonicOrigins.exe"))] }
            }
        },
        new()
        {
            ID = "SonicFrontiers",
            ModLoaderName = "HE2ModLoader",
            ModLoaderFileName = "d3d11.dll",
            ModLoaderIncompatibleFileNames = [ "dinput8.dll" ],
            ModLoaderDownloadURL = Resources.HE2MLDownloadURL,
            PlatformInfos = new()
            {
                { "Steam", [new("1237320", "SonicFrontiers.exe")] },
                { "Epic", [new("c5ca98fa240c4eb796835f97126df8e7", "SonicFrontiers.exe")] }
            }
        },
        // NOTE: Mod loader is not ready yet
        /*new()
        {
            ID = "SonicGenerations2024",
            ModLoaderName = "HE2ModLoader",
            ModLoaderFileName = "d3d11.dll",
            ModLoaderDownloadURL = Resources.HE2MLDownloadURL,
            ModDatabaseDirectoryName = "mods_sonic",
            PlatformInfos = new()
            {
                { "Steam", [new("2513280", "SONIC_GENERATIONS.exe")] },
                { "Epic", [new("a88805d3fbec4ca9bfc248105f6adb0a", "SONIC_GENERATIONS.exe") }
            }
        },*/
        new()
        {
            ID = "ShadowGenerations",
            ModLoaderName = "HE2ModLoader",
            ModLoaderFileName = "d3d11.dll",
            ModLoaderDownloadURL = Resources.HE2MLDownloadURL,
            ModDatabaseDirectoryName = "mods_shadow",
            PlatformInfos = new()
            {
                { "Steam", [new("2513280", "SONIC_X_SHADOW_GENERATIONS.exe")] },
                { "Epic", [new("a88805d3fbec4ca9bfc248105f6adb0a", "SONIC_X_SHADOW_GENERATIONS.exe")] }
            }
        },
        new()
        {
            ID = "UnleashedRecompiled",
            SupportsCodes = false,
            PlatformInfos = new()
            {
                { "Windows", [new(string.Empty, "SOFTWARE\\UnleashedRecomp")] },
                { "Flatpak", [new("io.github.hedge_dev.unleashedrecomp", "unleashedrecomp")] },
                { "Desktop", [new("io.github.hedge_dev.unleashedrecomp", "unleashedrecomp"), new("UnleashedRecomp", "unleashedrecomp")] }
            }
        }
    ];

    public static List<IModdableGame> LocateGames()
    {
        var games = new List<IModdableGame>();
        var steamLocator = new SteamLocator();
        var epicLocator = new EpicLocator();
        var steamGames = steamLocator.Locate();
        var epicGames = epicLocator.Locate();

        if (!string.IsNullOrEmpty(steamLocator.SteamInstallPath))
        {
            Logger.Debug($"Found Steam at: {steamLocator.SteamInstallPath}");
            Logger.Debug("  Supported games:");
            foreach (var game in steamGames)
            {
                if (ModdableGameList.Any(gameinfo => 
                    gameinfo.PlatformInfos.Any(platformInfo => 
                    platformInfo.Key == "Steam" && platformInfo.Value.Any(platformEntry => platformEntry.ID == game.ID))))
                {
                    Logger.Debug($"    {game.ID} - {game.Root}");
                }
            }
        }
        else
        {
            Logger.Debug("Steam not found!");
        }

        if (epicLocator.HeroicRootPaths.Count > 0)
        {
            Logger.Debug("Found Heroic at: ");
            foreach (var path in epicLocator.HeroicRootPaths)
            {
                Logger.Debug($"  {path}");
            }
        }
        else
        {
            Logger.Debug("Heroic not found!");
        }

        if (OperatingSystem.IsLinux())
        {
            Logger.Debug("XDG Data paths: " + string.Join(':', Paths.GetProgramDataPaths()));
        }

        foreach (var gameInfo in ModdableGameList)
        {
            if (gameInfo.PlatformInfos.TryGetValue("Steam", out var steamInfo))
            {
                foreach (var entry in steamInfo)
                {
                    var steamGame = steamGames.FirstOrDefault(x => x.ID == entry.ID);
                    if (steamGame != null)
                    {
                        var game = new ModdableGameGeneric(steamGame)
                        {
                            Name = gameInfo.ID,
                            Root = Path.GetDirectoryName(Path.Combine(steamGame.Root, entry.Executable))!,
                            Executable = entry.Executable,
                            ModLoaderName = gameInfo.ModLoaderName ?? "None",
                            Is64Bit = gameInfo.Is64Bit
                        };
                        game.ModDatabase.SupportsCodeCompilation = gameInfo.SupportsCodes;
                        game.ModLoader = new ModLoaderGeneric(game, game.ModLoaderName,
                            gameInfo.ModLoaderFileName, gameInfo.ModLoaderIncompatibleFileNames,
                            gameInfo.ModLoaderDownloadURL, gameInfo.Is64Bit);
                        if (gameInfo.ModDatabaseDirectoryName != null)
                            game.DefaultDatabaseDirectory = gameInfo.ModDatabaseDirectoryName;
                        games.Add(game);
                    }
                }
            }
            if (gameInfo.PlatformInfos.TryGetValue("Epic", out var epicInfo))
            {
                foreach (var entry in epicInfo)
                {
                    var epicGame = epicGames.FirstOrDefault(x => x.ID == entry.ID);
                    if (epicGame != null)
                    {
                        var game = new ModdableGameGeneric(epicGame)
                        {
                            Name = gameInfo.ID,
                            Root = Path.GetDirectoryName(Path.Combine(epicGame.Root, epicGame.Executable ?? entry.Executable))!,
                            Executable = epicGame.Executable ?? entry.Executable,
                            ModLoaderName = gameInfo.ModLoaderName ?? "None",
                            Is64Bit = gameInfo.Is64Bit
                        };
                        game.ModDatabase.SupportsCodeCompilation = gameInfo.SupportsCodes;
                        game.ModLoader = new ModLoaderGeneric(game, game.ModLoaderName,
                            gameInfo.ModLoaderFileName, gameInfo.ModLoaderIncompatibleFileNames,
                            gameInfo.ModLoaderDownloadURL, gameInfo.Is64Bit);
                        if (gameInfo.ModDatabaseDirectoryName != null)
                            game.DefaultDatabaseDirectory = gameInfo.ModDatabaseDirectoryName;
                        games.Add(game);
                    }
                }
            }
            if (gameInfo.PlatformInfos.TryGetValue("Windows", out var windowsInfo) &&
                OperatingSystem.IsWindows())
            {
                foreach (var entry in windowsInfo)
                {
                    string root = string.Empty;
                    string exe = string.Empty;
                    try
                    {
                        // "Executable" is used to hold the registry key
                        var key = Registry.CurrentUser.OpenSubKey(entry.Executable);
                        if (key != null)
                        {
                            exe = key.GetValue("ExecutableFilePath") as string ?? string.Empty;
                            root = key.GetValue("RootDirectoryPath") as string ?? string.Empty;
                            if (string.IsNullOrEmpty(root))
                                root = Path.GetDirectoryName(exe)!;
                        }
                        key?.Close();
                    }
                    catch { }

                    if (Directory.Exists(root) && File.Exists(exe))
                    {
                        var gameSimple = new GameSimple(
                            "Windows", string.Empty, gameInfo.ID,
                            root, Path.GetFileName(exe), "Windows", exe.Replace(" ", "\\ "), null, null);

                        var game = new ModdableGameGeneric(gameSimple)
                        {
                            SupportsDirectLaunch = true,
                            SupportsLauncher = false,
                            Is64Bit = gameInfo.Is64Bit
                        };
                        game.ModDatabase.SupportsCodeCompilation = gameInfo.SupportsCodes;
                        games.Add(game);
                    }
                }
            }
            // NOTE: This detection method is heavily flawed.
            if (gameInfo.PlatformInfos.TryGetValue("Flatpak", out var flatpakInfo))
            {
                foreach (var entry in flatpakInfo)
                {
                    string root = Path.Combine(Paths.GetUserHomePath(), ".var/app", entry.ID);

                    if (Directory.Exists(Path.Combine(root, "data")))
                    {
                        // Use custom path for Unleashed Recompiled
                        if (gameInfo.ID == "UnleashedRecompiled")
                            root = Path.Combine(Paths.GetUserConfigPath(), "UnleashedRecomp");

                        var gameSimple = new GameSimple(
                            "Flatpak", entry.ID, gameInfo.ID,
                            root, Path.GetFileName(entry.Executable), "Linux",
                            null, $"xdg-open {entry.Executable}:", null);

                        var game = new ModdableGameGeneric(gameSimple)
                        {
                            SupportsDirectLaunch = true,
                            SupportsLauncher = false,
                            Is64Bit = gameInfo.Is64Bit
                        };
                        game.ModDatabase.SupportsCodeCompilation = gameInfo.SupportsCodes;
                        games.Add(game);
                    }
                }
            }
            if (OperatingSystem.IsLinux() && gameInfo.PlatformInfos.TryGetValue("Desktop", out var desktopInfo))
            {
                foreach (var entry in desktopInfo)
                {
                    string root = Path.Combine(Paths.GetUserConfigPath(), entry.ID);
                    // Use custom path for Unleashed Recompiled
                    if (gameInfo.ID == "UnleashedRecompiled")
                        root = Path.Combine(Paths.GetUserConfigPath(), "UnleashedRecomp");

                    string[] searchPaths = [Paths.GetUserDataPath(), .. Paths.GetProgramDataPaths()];
                    foreach (var searchPath in searchPaths)
                    {
                        string path = Path.Combine(searchPath, "applications", $"{entry.ID}.desktop");
                        if (File.Exists(path))
                        {
                            string directCmd = $"xdg-open {entry.Executable}:";
                            string? launcherCmd = null;
                            string launchRoot = root;

                            // Parse desktop
                            try
                            {
                                Logger.Debug($"Reading desktop at \"{path}\"");
                                var desktopIni = Ini.FromFile(path);
                                if (desktopIni.TryGetValue("Desktop Entry", out var iniGroup))
                                {
                                    string MimeTypes = iniGroup.Get("MimeType", string.Empty);
                                    if (!string.IsNullOrEmpty(MimeTypes))
                                    {
                                        var splits = MimeTypes.Split(';');
                                        foreach (var split in splits)
                                        {
                                            if (split.StartsWith("x-scheme-handler/"))
                                            {
                                                launcherCmd = $"xdg-open {split.Split('/')[1]}:";
                                                Logger.Debug($"  Launcher command: {launcherCmd}");
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Logger.Debug("  No MIME types");
                                    }
                                    directCmd = iniGroup.Get("Exec", directCmd);
                                    Logger.Debug($"  Direct command: {directCmd}");
                                    launchRoot = iniGroup.Get("Path", launchRoot);
                                    Logger.Debug($"  Launch Root: {launchRoot}");
                                }

                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Failed to parse desktop file at \"{path}\"");
                                Logger.Error(e);
                            }

                            // Force launcher in Flatpak
                            if (Helpers.IsFlatpak)
                            {
                                if (launcherCmd != null)
                                    directCmd = launcherCmd;
                                // Setting launcher to null to prevent the option within UI
                                launcherCmd = null;
                            }

                            var gameSimple = new GameSimple(
                                "Desktop", entry.ID, gameInfo.ID,
                                root, Path.GetFileName(entry.Executable), "Linux",
                                directCmd, launcherCmd, launchRoot);

                            var game = new ModdableGameGeneric(gameSimple)
                            {
                                Is64Bit = gameInfo.Is64Bit
                            };
                            game.ModDatabase.SupportsCodeCompilation = gameInfo.SupportsCodes;
                            if (!games.Any(games => games.ID == game.ID))
                            {
                                games.Add(game);
                            }
                            else
                            {
                                Logger.Debug($"Game \"{game.ID}\" at \"{path}\" already exists");
                            }
                        }
                    }
                }
            }
        }

        return games;
    }

    public class GameInfo
    {
        public required string ID { get; init; }
        public string? ModLoaderName { get; init; }
        public string? ModLoaderFileName { get; init; }
        public string? ModLoaderDownloadURL { get; init; }
        public string[] ModLoaderIncompatibleFileNames { get; set; } = [];
        public string? ModDatabaseDirectoryName { get; init; }
        public bool Is64Bit { get; init; } = true;
        public bool SupportsCodes { get; init; } = true;
        public required Dictionary<string, List<GamePlatformInfo>> PlatformInfos { get; init; }
    }

    public record struct GamePlatformInfo(string ID, string Executable)
    {
    }
}