using Avalonia;
using Avalonia.Controls;

namespace HedgeModManager.UI.Languages;

public static class Language
{
    public static object? GetResource(string key)
    {
        object? val = null;
        Application.Current?.TryFindResource(key, out val);
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
