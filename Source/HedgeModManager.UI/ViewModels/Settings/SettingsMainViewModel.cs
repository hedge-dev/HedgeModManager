using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.IO;
using HedgeModManager.UI.Languages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.ViewModels.Settings;

public partial class SettingsMainViewModel : ViewModelBase
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

    public bool EnableSaveRedirection
    {
        get => Game?.ModLoaderConfiguration.EnableSaveFileRedirection ?? false;
        set
        {
            if (Game == null)
                return;
            Game.ModLoaderConfiguration.EnableSaveFileRedirection = value;
            OnPropertyChanged(nameof(EnableSaveRedirection));
        }
    }

    public bool SupportsMultipleLaunchMethods
    {
        get
        {
            if (MainViewModel?.GetModdableGameGeneric() is ModdableGameGeneric game)
                return game.SupportsDirectLaunch && game.SupportsLauncher;
            return MainViewModel == null;
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

    public ObservableCollection<ModProfile> SelectedProfiles => MainViewModel?.Profiles ?? [];

    public Theme SelectedTheme
    {
        get
        {
            var variant = Application.Current?.RequestedThemeVariant ?? ThemeVariant.Dark;
            return ThemeCollection.FirstOrDefault(x => x.Variant == variant, new (variant));
        }
    }

    public bool SupportsProton => (Game?.NativeOS == "Windows" && OperatingSystem.IsLinux() && !string.IsNullOrEmpty(Game.PrefixRoot)) || MainViewModel == null;
    public bool SupportsUpdates => OperatingSystem.IsWindows() || MainViewModel?.Config.TestModeEnabled == true;

    public string ModLoaderDescription
    {
        get
        {
            string text = Localize("Settings.Description.ModLoader");
            if (Game == null || Game.ModLoader == null || !Game.ModLoader.IsInstalled())
                return text;
            return $"{text} ({Game.ModLoader.GetInstalledVersion()})";
        }
    }

    public SettingsMainViewModel()
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
        OnPropertyChanged(nameof(ModsDirectory));
        OnPropertyChanged(nameof(ModLoaderEnabled));
        OnPropertyChanged(nameof(EnableDebugConsole));
        OnPropertyChanged(nameof(EnableSaveRedirection));
        OnPropertyChanged(nameof(HasModLoader));
        OnPropertyChanged(nameof(SupportsMultipleLaunchMethods));
        OnPropertyChanged(nameof(SupportsProton));
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(ModLoaderDescription));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Game) || e.PropertyName == nameof(MainViewModel))
            Update();
        base.OnPropertyChanged(e);
    }

    public class Theme(ThemeVariant variant)
    {
        public string Name { get; set; } = $"Theme.Name.{variant.Key.ToString() ?? "Unknown"}";
        public ThemeVariant Variant { get; set; } = variant;

        public override string ToString() => Name;
    }
}
