using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.ViewModels;
using System.Text.Json;

namespace HedgeModManager.UI.Config;

public partial class ProgramConfig : ViewModelBase
{
    [ObservableProperty] private bool _isSetupCompleted = false;
    [ObservableProperty] private bool _testModeEnabled = Program.IsDebugBuild;
    [ObservableProperty] private bool _checkManagerUpdates = true;
    [ObservableProperty] private bool _checkModLoaderUpdates = true;
    [ObservableProperty] private bool _checkModUpdates = true;
    [ObservableProperty] private bool _checkCodeUpdates = true;
    [ObservableProperty] private string? _lastSelectedPath;
    [ObservableProperty] private string? _theme;
    [ObservableProperty] private string? _language;
    [ObservableProperty] private DateTime _lastUpdateCheck = DateTime.MinValue;

    private string GetConfigPath()
    {
        string baseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Program.ApplicationCompany, Program.ApplicationName);
        return Path.Combine(baseDirectory, "ProgramConfig.json");
    }

    public void Load()
    {
        string filePath = GetConfigPath();
        if (!File.Exists(filePath))
            return;

        string jsonData = File.ReadAllText(filePath);

        var config = JsonSerializer.Deserialize<ProgramConfig>(jsonData, Program.JsonSerializerOptions);

        // Copy data
        if (config != null)
        {
            foreach (var property in GetType().GetProperties())
                if (property.CanWrite)
                    property.SetValue(this, property.GetValue(config));
        }
    }

    public async Task LoadAsync()
    {
        string filePath = GetConfigPath();
        if (!File.Exists(filePath))
            return;

        string jsonData = await File.ReadAllTextAsync(filePath);

        var config = JsonSerializer.Deserialize<ProgramConfig>(jsonData, Program.JsonSerializerOptions);

        // Copy data
        if (config != null)
        {
            foreach (var property in GetType().GetProperties())
                if (property.CanWrite)
                    property.SetValue(this, property.GetValue(config));
        }
    }

    public async Task SaveAsync()
    {
        string filePath = GetConfigPath();

        string jsonData = JsonSerializer.Serialize(this, Program.JsonSerializerOptions);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, jsonData);
        }
        catch
        {
            // TODO: Log error
        }
    }

    public void Reset()
    {
        var config = new ProgramConfig();
        foreach (var property in GetType().GetProperties())
            if (property.CanWrite)
                property.SetValue(this, property.GetValue(config));
    }
}
