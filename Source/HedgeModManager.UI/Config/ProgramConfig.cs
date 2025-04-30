using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;

namespace HedgeModManager.UI.Config;

public partial class ProgramConfig : ConfigBase
{
    // TODO: Make use of setup
    [ObservableProperty] private bool _isSetupCompleted = true;
    [ObservableProperty] private bool _testModeEnabled = Program.IsDebugBuild;
    [ObservableProperty] private bool _checkManagerUpdates = true;
    [ObservableProperty] private bool _checkModLoaderUpdates = true;
    [ObservableProperty] private bool _checkModUpdates = true;
    [ObservableProperty] private bool _checkCodeUpdates = true;
    [ObservableProperty] private string? _lastSelectedPath;
    [ObservableProperty] private string? _theme;
    [ObservableProperty] private string? _language;
    [ObservableProperty] private DateTime _lastUpdateCheck = DateTime.MinValue;
    [ObservableProperty] private WindowState _lastWindowState = WindowState.Normal;
    [ObservableProperty] private List<string> _lastSeenLanguages = [];

    // Test Flags
    [ObservableProperty] private bool _testKeyboardInput = false;

    protected override string GetConfigFilePath()
    {
        return Path.Combine(Paths.GetConfigPath(), "ProgramConfig.json");
    }
}
