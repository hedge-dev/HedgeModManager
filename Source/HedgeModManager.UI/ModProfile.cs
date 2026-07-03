namespace HedgeModManager.UI;
using CoreLib;
using System.Text.Json.Serialization;
using UI.Models;

public class ModProfile
{
    public static ModProfile Default = new("Default", "ModsDB.ini", "Default.ini");

    [JsonIgnore]
    public bool Enabled { get; set; } = false;
    public string Name { get; set; } = "Profile";
    // File path relative to the mods directory for the modloader to access
    public string ModDBPath { get; set; } = "ModsDB.ini";
    // File path ralative to the mod's backup directory for restore
    public string FileName { get; set; } = "Profile.ini";

    public ModProfile(string name, string modsDBPath, string fileName)
    {
        Enabled = false;
        Name = name;
        ModDBPath = modsDBPath;
        FileName = fileName;
    }

    public ModProfile(string name) : this(name, string.Empty, string.Empty)
    {
        FileName = GenerateFileNameFromName();
        ModDBPath = FileName;
    }

    public ModProfile() { }

    /// <summary>
    /// Generates a file name based off the profile's name.
    /// 
    /// This does not check if the file exists.
    /// </summary>
    /// <returns></returns>
    public string GenerateFileNameFromName()
    {
        return string.Join(string.Empty, Name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "") + ".ini";
    }

    public async Task RestoreModConfigAsync(ModDatabaseGeneric database, IProgress<long> progress)
    {
        if (database == null)
            return;

        var mods = database.Mods
            .Where(x => !string.IsNullOrEmpty(x.ConfigSchemaFile))
            .ToList();
        
        progress.ReportMax(mods.Count);
        progress.Report(0);
        for (int i = 0; i < mods.Count; i++)
        {
            progress.Report(i);
            var mod = mods[i];
            var schema = await ModConfig.LoadSchemaFile(Path.Combine(mod.Root, mod.ConfigSchemaFile));
            if (schema == null)
                continue;

            bool isWindows = database.NativeOS == "Windows";
            await schema.Load(Path.Combine(mod.Root, "profiles", FileName));
            await schema.Save(Path.Combine(mod.Root, schema.IniFile), isWindows);
        }
        progress.Report(mods.Count);
    }

    public async Task BackupModConfigAsync(ModDatabaseGeneric database, ModProfile? sourceProfile, IProgress<long> progress)
    {
        if (database == null)
            return;

        var mods = database.Mods
            .Where(x => !string.IsNullOrEmpty(x.ConfigSchemaFile))
            .ToList();

        progress.ReportMax(mods.Count);
        progress.Report(0);

        for (int i = 0; i < mods.Count; i++)
        {
            progress.Report(i);
            var mod = mods[i];
            var schema = await ModConfig.LoadSchemaFile(Path.Combine(mod.Root, mod.ConfigSchemaFile));
            if (schema == null)
                continue;

            string sourceProfilePath = Path.Combine(mod.Root, schema.IniFile);
            if (sourceProfile != null)
                sourceProfilePath = Path.Combine(mod.Root, "profiles", sourceProfile.FileName);

            await schema.Load(sourceProfilePath);
            await schema.Save(Path.Combine(mod.Root, "profiles", FileName), false);
        }
        progress.Report(mods.Count);
    }

    public async Task DeleteModConfigAsync(ModDatabaseGeneric database, IProgress<long> progress)
    {
        if (database == null)
            return;

        var mods = database.Mods
            .Where(x => !string.IsNullOrEmpty(x.ConfigSchemaFile))
            .ToList();

        progress.ReportMax(mods.Count);
        progress.Report(0);
        for (int i = 0; i < mods.Count; i++)
        {
            progress.Report(i);
            var mod = mods[i];

            string profileModPath = Path.Combine(mod.Root, "profiles", FileName);
            if (File.Exists(profileModPath))
                File.Delete(profileModPath);
        }
        progress.Report(mods.Count);
    }

    public override string ToString()
    {
        return Name;
    }
}