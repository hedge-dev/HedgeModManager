using Avalonia.Styling;
using SharpCompress;

namespace HedgeModManager.UI.Themes;

public static class Themes
{
    public static ThemeVariant Darker { get; } = new("Darker", ThemeVariant.Dark);

    public static ThemeVariant GetTheme(string? name)
    {
        if (name == null)
            return ThemeVariant.Dark;

        return GetAllThemes()
            .FirstOrDefault(x => name.Equals(x.Key as string,
            StringComparison.InvariantCultureIgnoreCase)) ?? ThemeVariant.Dark;
    }

    public static List<ThemeVariant> GetAllThemes()
    {
        var list = new List<ThemeVariant>(
        [
            ThemeVariant.Dark,
            //ThemeVariant.Light
        ]);
        typeof(Themes)
            .GetProperties()
            .Where(x => x.PropertyType == typeof(ThemeVariant))
            .Select(x => x.GetValue(null))
            .Cast<ThemeVariant>()
            .ForEach(list.Add);
        return list;
    }
}
