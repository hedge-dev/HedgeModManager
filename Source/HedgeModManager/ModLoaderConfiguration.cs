namespace HedgeModManager;
using Foundation;
using Text;

public class ModLoaderConfiguration : IModLoaderConfiguration
{
    public const string LegacySectionName = "CPKREDIR";

    public bool Enabled { get; set; } = true;
    public bool EnableSaveFileRedirection { get; set; } = false;
    public string DatabasePath { get; set; } = string.Empty;
    public string LogType { get; set; } = "none";

    public void Parse(Ini ini)
    {
        if (ini.TryGetValue(LegacySectionName, out var group))
        {
            Enabled = group.Get<int>("Enabled") != 0;
            EnableSaveFileRedirection = group.Get<int>("EnableSaveFileRedirection") != 0;
            
            DatabasePath = group.Get<string>("ModsDbIni");
            LogType = group.Get<string>("LogType");
        }
    }

    public void Serialize(Ini ini)
    {
        var group = ini.GetOrAddValue(LegacySectionName);
        group.Set("Enabled", Enabled ? 1 : 0);
        group.Set("EnableSaveFileRedirection", EnableSaveFileRedirection ? 1 : 0);
        group.Set("ModsDbIni", DatabasePath);
        group.Set("LogType", LogType);
    }

    public async Task Load(string path)
    {
        Parse(Ini.FromText(await File.ReadAllTextAsync(path)));
    }

    public async Task Save(string path)
    {
        var ini = new Ini();
        Serialize(ini);
        await File.WriteAllTextAsync(path, ini.Serialize());
    }

}