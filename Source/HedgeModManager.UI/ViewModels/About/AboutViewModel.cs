using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace HedgeModManager.UI.ViewModels.About;

public partial class AboutViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<KeyValuePair<string, Credit[]>> _credits = [];

    public string Version { get; } = Program.ApplicationVersion;

    public async Task Update()
    {
        // Load credits
        var creditsDict = await JsonSerializer.DeserializeAsync<Dictionary<string, Credit[]>>
            (Utils.LoadAsset($"Documents/Credits.json"));

        if (creditsDict != null)
            Credits = new(creditsDict);
    }

    public partial class Credit : ViewModelBase
    {
        public string Name { get; set; } = "Credit Name";
        public string Role { get; set; } = "Credit Role";
        public string GitHub { get; set; } = string.Empty;

        [ObservableProperty] public object? _image = null;

        public string GetGitHubURL()
        {
            return $"https://github.com/{GitHub}";
        }
    }

}
