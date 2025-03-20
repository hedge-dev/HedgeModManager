﻿using HedgeModManager.Text;
using HedgeModManager.UI.Converters.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HedgeModManager.UI.Models;

public class ModConfig
{
    public string IniFile { get; set; } = "config.ini";
    public List<ConfigGroup> Groups { get; set; } = [];
    public Dictionary<string, List<ConfigEnum>> Enums { get; set; } = [];

    public class ConfigGroup
    {
        public string Name { get; set; } = "ConfigGroup";
        public string DisplayName { get; set; } = "Config Group";
        public List<ConfigElement> Elements { get; set; } = [];

        public override string ToString() => $"{DisplayName}";
    }

    public class ConfigElement
    {
        public string Name { get; set; } = "ConfigName";
        public List<string> Description { get; set; } = [];
        public string DisplayName { get; set; } = "Config Name";
        public string Type { get; set; } = "string";
        [JsonConverter(typeof(StringDynamicConverter))]
        public dynamic Value { get; set; } = string.Empty;
        [JsonConverter(typeof(StringDoubleConverter))]
        public double? MinValue { get; set; }
        [JsonConverter(typeof(StringDoubleConverter))]
        public double? MaxValue { get; set; }
        [JsonConverter(typeof(StringDynamicConverter))]
        public dynamic? DefaultValue { get; set; }

        public override string ToString() => $"{DisplayName}: {Value}";
    }

    public class ConfigEnum
    {
        public string DisplayName { get; set; } = "Enum";
        [JsonConverter(typeof(StringDynamicConverter))]
        public dynamic Value { get; set; } = "Item";
        public List<string> Description { get; set; } = [];

        public override string ToString() => DisplayName;
    }

    public static async Task<ModConfig?> LoadSchemaFile(string? jsonPath)
    {
        if (!File.Exists(jsonPath))
            return null;

        string jsonData = await File.ReadAllTextAsync(jsonPath);

        return JsonSerializer.Deserialize<ModConfig>(jsonData, Program.JsonSerializerOptions) ?? new();
    }

    public async Task Load(string iniPath)
    {
        if (!File.Exists(iniPath))
        {
            LoadDefaults();
            return;
        }

        var ini = Ini.FromText(await File.ReadAllTextAsync(iniPath));
        foreach (var group in Groups)
        {
            IniGroup? iniGroup = null;
            ini.TryGetValue(group.Name, out iniGroup);
            if (iniGroup == null)
                continue;
            foreach (var element in group.Elements)
            {
                string? valString = null;
                iniGroup.TryGetValue(element.Name, out object? obj);
                if (obj == null)
                    valString = element.DefaultValue?.ToString();
                else
                    valString = obj?.ToString();

                if (valString != null)
                {
                    // Convert types
                    switch (element.Type)
                    {
                        case "bool":
                            if (bool.TryParse(valString, out bool valVool))
                                element.Value = valVool;
                            else
                                element.Value = (bool)element.DefaultValue;
                            break;
                        case "float":
                            if (float.TryParse(valString, out float valFloat))
                                element.Value = valFloat;
                            else
                                element.Value = (float)element.DefaultValue;
                            break;
                        case "int":
                            if (int.TryParse(valString, out int valInt))
                                element.Value = valInt;
                            else
                                element.Value = (int)element.DefaultValue;
                            break;
                        default:
                            // Check enums
                            if (Enums.TryGetValue(element.Type, out List<ConfigEnum>? enumList))
                            {
                                var enumVal = enumList.FirstOrDefault(x => x.Value.ToString() == valString);
                                if (enumVal != null)
                                    element.Value = enumVal.Value;
                                else
                                    element.Value = element.DefaultValue ?? string.Empty;
                            }
                            else
                                element.Value = valString;
                            break;
                    }
                }
            }
        }
    }

    public void LoadDefaults()
    {
        foreach (var group in Groups)
        {
            foreach (var element in group.Elements)
            {
                element.Value = element.DefaultValue!;
            }
        }
    }

    public async Task Save(string iniPath, bool isWindows, bool useExisting = true)
    {
        var ini = new Ini();
        string lineEnding = isWindows ? "\r\n" : Environment.NewLine;

        // Reuse existing config if exists
        if (useExisting && File.Exists(iniPath))
            ini = Ini.FromText(await File.ReadAllTextAsync(iniPath));

        foreach (var group in Groups)
        {
            var iniGroup = ini.GetOrAddValue(group.Name);
            foreach (var element in group.Elements)
                iniGroup[element.Name] = element.Value;
        }

        // Fix issue with counts being strings
        foreach (var group in ini.Groups)
            foreach (var key in group.Value)
                if (key.Key.EndsWith("Count") && key.Value is string value)
                    if (int.TryParse(value, out int count))
                        group.Value[key.Key] = count;

        Directory.CreateDirectory(Path.GetDirectoryName(iniPath)!);
        await File.WriteAllTextAsync(iniPath, ini.Serialize().ReplaceLineEndings(lineEnding));
    }

    public void GenerateSchemaExample()
    {
        Groups.Add(new ConfigGroup()
        {
            DisplayName = "Example Group",
            Name = "Group1",
            Elements = new List<ConfigElement>()
            {
                new() { DisplayName = "Example Bool", Name = "exBool", DefaultValue = false, Type = "bool", Description = ["Line 1", "Line 2"] },
                new() { DisplayName = "Example String", Name = "exString", DefaultValue = string.Empty, Type = "string", Description = ["Line 1", "Line 2"] },
                new() { DisplayName = "Example Float", Name = "exFloat", DefaultValue = 0.0f, Type = "float", Description = ["Line 1", "Line 2"] },
                new() { DisplayName = "Example Int", Name = "exInt", DefaultValue = 0, Type = "int", Description = ["Line 1", "Line 2"] },
                new() { DisplayName = "Example Enum", Name = "exEnum", DefaultValue = "Item1", Type = "ExampleEnum", Description = ["Line 1", "Line 2"] }
            }
        });

        Enums.Add("ExampleEnum", 
        [
            new() { DisplayName = "Item 1", Value = "Item1", Description = new List<string>() { "Line 1", "Line 2" }},
            new() { DisplayName = "Item 2", Value = "Item2", Description = new List<string>() { "Line 1", "Line 2" }}
        ]);
    }
}
