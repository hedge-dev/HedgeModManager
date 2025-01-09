using Avalonia.Media;
using Avalonia.Media.Imaging;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Models;

namespace HedgeModManager.UI;

public class Games
{
    public static IImage GetIcon(string? gameName)
    {
        try
        {
            return new Bitmap(Utils.LoadAsset($"Graphics/Icons/{gameName}.png"));
        }
        catch
        {
            return new Bitmap(Utils.LoadAsset($"Graphics/Icons/Default.png"));
        }
    }

    public static List<UIGame> GetUIGames(IEnumerable<IModdableGame> hmmGame)
    {
        var games = new List<UIGame>();
        foreach (var game in hmmGame)
        {
            var icon = GetIcon(game.Name);
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
