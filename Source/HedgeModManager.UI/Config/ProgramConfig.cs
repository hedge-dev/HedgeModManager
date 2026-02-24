using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;

namespace HedgeModManager.UI.Config;

public partial class ProgramConfig : ConfigBase
{
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
    [ObservableProperty] private IntegrationsConfig _integrations = new();

    // Test Flags
    [ObservableProperty] private bool _testKeyboardInput = false;

    protected override string GetConfigFilePath()
    {
        return Path.Combine(Paths.GetConfigPath(), "ProgramConfig.json");
    }

    public class IntegrationsConfig
    {
        public bool GameBananaEnabled { get; set; } = true;
        public bool GameBananaRemoteDLEnabled { get; set; } = false;
        public string GameBananaRemoteDLMemberID { get; set; } = string.Empty;
        public string GameBananaRemoteDLSecretKey { get; set; } = string.Empty;
    }
}
