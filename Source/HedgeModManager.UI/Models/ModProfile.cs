using System.Text.Json.Serialization;

namespace HedgeModManager.UI.Models;

public class ModProfile
{
    public static ModProfile Default = new("Default", "ModsDB.ini", "Default.ini");

    [JsonIgnore]
    public bool Enabled { get; set; } = false;
    public string Name { get; set; } = "Profile";
    public string ModDBPath { get; set; } = "ModsDB.ini";
    public string FileName { get; set; } = "Profile.ini";

    public ModProfile(string name, string modsDBPath, string fileName)
    {
        Enabled = false;
        Name = name;
        ModDBPath = modsDBPath;
        FileName = fileName;
    }

    public ModProfile(string name) : this(name, "ModsDB.ini", string.Empty)
    {
        FileName = GenerateFileNameFromName();
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

    public override string ToString()
    {
        return Name;
    }
}