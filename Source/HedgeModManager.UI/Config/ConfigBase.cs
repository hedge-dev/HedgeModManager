using HedgeModManager.Foundation;
using HedgeModManager.UI.ViewModels;
using System.Text.Json;

namespace HedgeModManager.UI.Config;

public abstract class ConfigBase : ViewModelBase
{
    public virtual void Load()
    {
        string filePath = GetConfigFilePath();
        if (!File.Exists(filePath))
            return;

        string jsonData = File.ReadAllText(filePath);

        var config = JsonSerializer.Deserialize(jsonData, GetType(), Program.JsonSerializerOptions);

        // Copy data
        if (config != null)
        {
            foreach (var property in GetType().GetProperties())
                if (property.CanWrite)
                    property.SetValue(this, property.GetValue(config));
        }
    }

    public virtual async Task LoadAsync()
    {
        string filePath = GetConfigFilePath();
        if (!File.Exists(filePath))
            return;

        string jsonData = await File.ReadAllTextAsync(filePath);

        var config = JsonSerializer.Deserialize(jsonData, GetType(), Program.JsonSerializerOptions);

        // Copy data
        if (config != null)
        {
            foreach (var property in GetType().GetProperties())
                if (property.CanWrite)
                    property.SetValue(this, property.GetValue(config));
        }
    }

    public virtual async Task SaveAsync()
    {
        string filePath = GetConfigFilePath();

        string jsonData = JsonSerializer.Serialize(this, GetType(), Program.JsonSerializerOptions);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, jsonData);
        }
        catch
        {
            Logger.Error($"Failed to save config file: {filePath} [{GetType().FullName}]");
        }
    }

    public virtual void Reset()
    {
        var config = Activator.CreateInstance(GetType());
        foreach (var property in GetType().GetProperties())
            if (property.CanWrite)
                property.SetValue(this, property.GetValue(config));
    }

    protected abstract string GetConfigFilePath();
}
