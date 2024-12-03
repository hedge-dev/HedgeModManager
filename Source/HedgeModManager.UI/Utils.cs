
using Avalonia.Media.Immutable;
using HedgeModManager.UI.Properties;
using Markdig;

namespace HedgeModManager.UI;

public class Utils
{
    public static string ConvertToHTML(string text)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var fg = App.GetWindowResource<ImmutableSolidColorBrush>("Text.NormalBrush");
        var template = Resources.HTMLTemplate;
        if (fg is null)
            return "HTML CONVERT ERROR";

        template = template.Replace("--var(FOREGROUND)", $"rgb({fg.Color.R}, {fg.Color.G}, {fg.Color.B})");
        template = template.Replace("CONTENT", Markdown.ToHtml(text.Replace("\\n", "\n"), pipeline));

        return template;
    }
}
