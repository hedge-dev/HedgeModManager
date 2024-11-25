using Avalonia.Media.Imaging;
using Avalonia.Platform;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HedgeModManager.UI;

public class Games
{
    public static readonly List<ExtraGameInfo> ExtraGameInfos =
    [
        new () { GameID = "SonicGenerations", IconName = "SonicGenerations.png" },
        new () { GameID = "SonicLostWorld", IconName = "SonicLostWorld.png" },
        new () { GameID = "SonicForces", IconName = "SonicForces.png" },
        new () { GameID = "PuyoPuyoTetris2", IconName = "PuyoPuyoTetris2.png" },
        new () { GameID = "Tokyo2020", IconName = "Tokyo2020.png" },
        new () { GameID = "SonicColorsUltimate", IconName = "SonicColorsUltimate.png" },
        new () { GameID = "SonicOrigins", IconName = "SonicOrigins.png" },
        new () { GameID = "SonicFrontiers", IconName = "SonicFrontiers.png" },
        new () { GameID = "SonicGenerations2024", IconName = "SonicGenerations2024.png" },
        new () { GameID = "ShadowGenerations", IconName = "ShadowGenerations.png" },
    ];

    public static Uri ConvertToAssetUri(string path)
    {
        return new Uri($"avares://HedgeModManager.UI/Assets/{path}");
    }

    public static List<UIGame> GetUIGames(IEnumerable<IModdableGame> hmmGame)
    {
        var games = new List<UIGame>();
        foreach (var game in hmmGame)
        {
            var extraInfo = ExtraGameInfos.FirstOrDefault(x => x.GameID == game.Name);
            var iconName = extraInfo?.IconName ?? "Default.png";
            var icon = new Bitmap(AssetLoader.Open(ConvertToAssetUri($"Graphics/Icons/{iconName}")));
            games.Add(new UIGame(game, icon));
        }
        return games;
    }

    // This class contains extra information about the games
    public class ExtraGameInfo
    {
        public string GameID { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
    }

}
