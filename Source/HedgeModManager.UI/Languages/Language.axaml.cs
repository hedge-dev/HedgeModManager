using System;

namespace HedgeModManager.UI.Languages;

public static class Language
{
    public static string? GetResource(string key)
    {
        string? val = null;
        val = App.GetWindowResource<string>(key);
        if (val == null)
            return App.GetResource<string>(key);
        return val;
    }

    public static string Localize(string key, params object[] args)
    {
        var resource = GetResource(key);
        if (resource is string str)
            return string.Format(str, args);
        return key;
    }
}

public class LanguageEntry
{
    public static int TotalLineCount = 0;

    public string Code { get; set; } = "en";
    public string Name { get; set; } = "English";
    public int Lines { get; set; } = 0;
    public bool Local { get; set; } = false;

    public override string ToString()
    {
        return Lines != TotalLineCount ? $"{Name} ({Math.Floor((float)Lines / TotalLineCount * 100)}%)" : Name;
    }
}
