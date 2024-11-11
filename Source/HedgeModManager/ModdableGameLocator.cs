﻿namespace HedgeModManager;
using Foundation;
using Steam;
using System.Linq;

public class ModdableGameLocator
{

    public static List<GameInfo> ModdableGameList = new()
    {
        new ()
        {
            ID = "SonicGenerations",
            ModLoaderFileName = "d3d9.dll",
            PlatformInfos = new()
            {
                { "Steam", new ("71340", "SonicGenerations.exe") }
            }
        },
        new ()
        {
            ID = "SonicLostWorld",
            ModLoaderFileName = "d3d9.dll",
            PlatformInfos = new() { { "Steam", new ("329440", "slw.exe") } }
        },
        new ()
        {
            ID = "SonicForces",
            ModLoaderFileName = "d3d11.dll",
            PlatformInfos = new() { { "Steam", new ("637100", Path.Combine("build", "main", "projects", "exec", "Sonic Forces.exe")) } }
        },
        new ()
        {
            ID = "PuyoPuyoTetris2",
            ModLoaderFileName = "dinput8.dll",
            PlatformInfos = new() { { "Steam", new ("1259790", "PuyoPuyoTetris2.exe") } }
        },
        new ()
        {
            ID = "Tokyo2020",
            ModLoaderFileName = "dinput8.dll",
            PlatformInfos = new() { { "Steam", new ("981890", "musashi.exe") } }
        },
        new ()
        {
            ID = "SonicColorsUltimate",
            ModLoaderFileName = "d3d11.dll",
            PlatformInfos = new()
            {
                { "Steam", new ("2055290", Path.Combine("exec", "SonicColorsUltimate.exe")) },
                { "Epic Games", new ("e5071e19d08c45a6bdda5d92fbd0a03e", Path.Combine("rainbow Shipping", "Sonic Colors - Ultimate.exe")) }
            }
        },
        new ()
        {
            ID = "SonicOrigins",
            ModLoaderFileName = "dinput8.dll",
            PlatformInfos = new()
            {
                { "Steam", new ("1794960", Path.Combine("build", "main", "projects", "exec", "SonicOrigins.exe")) },
                { "Epic Games", new ("5070a8e44cf74ba3b9a4ca0c0dce5cf1", Path.Combine("build", "main", "projects", "exec", "SonicOrigins.exe")) }
            }
        },
        new ()
        {
            ID = "SonicFrontiers",
            ModLoaderFileName = "d3d11.dll",
            PlatformInfos = new()
            {
                { "Steam", new ("1237320", "SonicFrontiers.exe") },
                { "Epic Games", new ("c5ca98fa240c4eb796835f97126df8e7", "SonicFrontiers.exe") }
            }
        },
        new ()
        {
            ID = "SonicGenerations2024",
            ModLoaderFileName = "d3d11.dll",
            PlatformInfos = new()
            {
                { "Steam", new ("2513280", "SONIC_GENERATIONS.exe") },
                { "Epic Games", new ("a88805d3fbec4ca9bfc248105f6adb0a", "SONIC_GENERATIONS.exe") }
            }
        },
        new ()
        {
            ID = "ShadowGenerations",
            ModLoaderFileName = "d3d11.dll",
            PlatformInfos = new()
            {
                { "Steam", new ("2513280", "SONIC_X_SHADOW_GENERATIONS.exe") },
                { "Epic Games", new ("a88805d3fbec4ca9bfc248105f6adb0a", "SONIC_X_SHADOW_GENERATIONS.exe") }
            }
        }
    };

    public static List<IModdableGame> LocateGames()
    {
        var games = new List<IModdableGame>();
        var steamGames = new SteamLocator().Locate();

        foreach (var gameInfo in ModdableGameList)
        {
            if (gameInfo.PlatformInfos.TryGetValue("Steam", out var platformInfo))
            {
                var steamGame = steamGames.FirstOrDefault(x => x.ID == platformInfo.ID);
                if (steamGame != null)
                {
                    games.Add(new ModdableGameGeneric(steamGame)
                    {
                        Name = gameInfo.ID,
                        Root = Path.GetDirectoryName(Path.Combine(steamGame.Root, platformInfo.Executable))!,
                        Executable = platformInfo.Executable,
                        ModLoaderFileName = gameInfo.ModLoaderFileName ?? ""
                    });
                }
            }
        }

        return games;
    }

    public class GameInfo
    {
        public required string ID { get; init; }
        public string? ModLoaderFileName { get; init; }
        public required Dictionary<string, GamePlatformInfo> PlatformInfos { get; init; }
    }

    public record struct GamePlatformInfo(string ID, string Executable)
    {
    }
}