using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using System.Text.Json.Serialization;

namespace HedgeModManager.UI.Config;

public partial class GameConfig : ConfigBase
{
    private string _gameName = "GameName";

    [ObservableProperty] private List<string> _expandedCodes = [];

    [JsonIgnore] public string GameName => _gameName;

    public GameConfig() : base() { }

    public GameConfig(string gameName) : this()
    {
        _gameName = gameName;
    }

    public GameConfig(IGame game) : this(game.Name) { }

    protected override string GetConfigFilePath()
    {
        return Path.Combine(Paths.GetConfigPath(), "Games", $"{GameName}.json");
    }
}
