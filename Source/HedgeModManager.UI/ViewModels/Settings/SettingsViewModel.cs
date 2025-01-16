using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.IO;
using HedgeModManager.UI.Languages;
using HedgeModManager.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HedgeModManager.UI.ViewModels.Settings;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private ModdableGameGeneric? _game;
    [ObservableProperty] private MainWindowViewModel? _mainViewModel;
    [ObservableProperty] private string _checkManagerUpdatesText = "Settings.Button.CheckUpdates";
    [ObservableProperty] private string _checkLoaderUpdatesText = "Settings.Button.CheckUpdates";
    [ObservableProperty] private string _checkModUpdatesText = "Settings.Button.CheckUpdates";
    [ObservableProperty] private string _checkCodeUpdatesText = "Settings.Button.CheckUpdates";

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

    public string InstallModLoaderText
    {
        get {
            if (Game == null || Game.ModLoader == null)
                return "";
            return Game.ModLoader.IsInstalled() ?
                "Settings.Button.UninstallML" : "Settings.Button.InstallML";
        }
    }

    public bool EnableLauncher
    {
        get => false;
        set
        {
            OnPropertyChanged(nameof(EnableLauncher));
        }
    }

    public bool HasModLoader => Game?.ModLoader != null;

    public bool SupportsMultipleLaunchMethods
    {
        get
        {
            if (MainViewModel?.GetModdableGameGeneric() is ModdableGameGeneric game)
                return game.SupportsDirectLaunch && game.SupportsLauncher;
            return false;
        }
    }

    public LanguageEntry? SelectedLanguage
    {
        get => MainViewModel?.SelectedLanguage ?? MainViewModel?.Languages.FirstOrDefault();
    }

    public ModProfile? SelectedProfile
    {
        get => MainViewModel?.SelectedProfile ?? ModProfile.Default;
    }

    public Theme SelectedTheme
    {
        get
        {
            var variant = Application.Current?.RequestedThemeVariant ?? ThemeVariant.Dark;
            return ThemeCollection.FirstOrDefault(x => x.Variant == variant, new (variant));
        }
    }

    public bool SupportsProton => Game?.NativeOS == "Windows" && OperatingSystem.IsLinux() && !string.IsNullOrEmpty(Game.PrefixRoot);

    public SettingsViewModel()
    {
        UpdateThemes();
    }

    public void UpdateThemes()
    {
        ThemeCollection.Clear();
        Themes.Themes.GetAllThemes().ForEach(x => ThemeCollection.Add(new (x)));
    }

    public void Update()
    {
        OnPropertyChanged(nameof(InstallModLoaderText));
        OnPropertyChanged(nameof(SelectedProfile));
        OnPropertyChanged(nameof(SupportsProton));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Game))
        {
            OnPropertyChanged(nameof(ModsDirectory));
            OnPropertyChanged(nameof(ModLoaderEnabled));
            OnPropertyChanged(nameof(InstallModLoaderText));
            OnPropertyChanged(nameof(EnableDebugConsole));
            OnPropertyChanged(nameof(HasModLoader));
            OnPropertyChanged(nameof(SupportsMultipleLaunchMethods));
            OnPropertyChanged(nameof(SupportsProton));
        }
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
