
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using HedgeModManager.UI.Properties;
using Markdig;

namespace HedgeModManager.UI;

public static class Utils
{
    public static string ConvertToHTML(string text)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var fg = App.GetResource<ImmutableSolidColorBrush>("Text.NormalBrush") as ISolidColorBrush;
        var template = Resources.HTMLTemplate;
        if (fg is null)
        {
            text = "ERROR: Missing Brush\n\n" + text;
            fg = new SolidColorBrush(0xFFFF0000);
        }

        template = template.Replace("--var(FOREGROUND)", $"rgb({fg.Color.R}, {fg.Color.G}, {fg.Color.B})");
        template = template.Replace("CONTENT", Markdown.ToHtml(text.Replace("\\n", "\n"), pipeline));

        return template;
    }

    public static async Task<Bitmap?> DownloadBitmap(string? uri)
    {
        if (string.IsNullOrEmpty(uri))
            return null;

        try
        {
            using var client = new HttpClient();
            using var memStream = new MemoryStream();
            using var stream = await client.GetStreamAsync(uri);
            await stream.CopyToAsync(memStream);
            memStream.Position = 0;
            return new Bitmap(memStream);
        }
        catch { }
        return null;
    }

    public static string ConvertToPath(this Uri uri)
    {
        return Uri.UnescapeDataString(uri.AbsolutePath);
    }
}
