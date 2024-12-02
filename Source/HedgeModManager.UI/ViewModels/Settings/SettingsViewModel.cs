using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.IO;
using HedgeModManager.UI.Languages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace HedgeModManager.UI.ViewModels.Settings;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private ModdableGameGeneric? _game;
    [ObservableProperty] private MainWindowViewModel? _mainViewModel;

    public ObservableCollection<Theme> ThemeCollection { get; } = [];

    public string ModsDirectory
    {
        get
        {
            if (Game is not ModdableGameGeneric game)
                return string.Empty;

            return PathEx.GetDirectoryName(game.ModLoaderConfiguration.DatabasePath).ToString();
        }
        set
        {
            if (Game is not ModdableGameGeneric game)
                return;

            game.ModLoaderConfiguration.DatabasePath = Path.Combine(value, game.ModDatabase.Name);
            OnPropertyChanged(nameof(ModsDirectory));
        }
    }

    public bool ModLoaderEnabled
    {
        get => Game?.ModLoaderConfiguration.Enabled ?? false;
        set
        {
            if (Game is not ModdableGameGeneric game)
                return;

            game.ModLoaderConfiguration.Enabled = value;
            OnPropertyChanged(nameof(ModLoaderEnabled));
        }
    }

    public bool EnableDebugConsole
    {
        get => Game?.ModLoaderConfiguration.LogType == "console";
        set
        {
            if (Game is not ModdableGameGeneric game)
                return;

            game.ModLoaderConfiguration.LogType = value ? "console" : "none";
            OnPropertyChanged(nameof(EnableDebugConsole));
        }
    }

    public bool EnableLauncher
    {
        get => false;
        set
        {
            OnPropertyChanged(nameof(EnableDebugConsole));
        }
    }

    public LanguageEntry? SelectedLanguage
    {
        get => MainViewModel?.SelectedLanguage ?? MainViewModel?.Languages.FirstOrDefault();
    }

    public Theme SelectedTheme
    {
        get
        {
            var variant = Application.Current?.RequestedThemeVariant ?? ThemeVariant.Dark;
            return ThemeCollection.FirstOrDefault(x => x.Variant == variant, new (variant));
        }
    }

    public SettingsViewModel()
    {
        UpdateThemes();
    }

    public void UpdateThemes()
    {
        ThemeCollection.Clear();
        Themes.Themes.GetAllThemes().ForEach(x => ThemeCollection.Add(new (x)));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Game))
            OnPropertyChanged(nameof(ModsDirectory));
        if (e.PropertyName == nameof(MainViewModel))
            OnPropertyChanged(nameof(SelectedLanguage));
        base.OnPropertyChanged(e);
    }

    public class Theme(ThemeVariant variant)
    {
        public string Name { get; set; } = $"Theme.Name.{variant.Key.ToString() ?? "Unknown"}";
        public ThemeVariant Variant { get; set; } = variant;

        public override string ToString() => Name;
    }
}
