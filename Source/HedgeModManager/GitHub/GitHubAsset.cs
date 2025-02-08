using System.Text.Json.Serialization;

namespace HedgeModManager.GitHub;

public class GitHubAsset
{
    [JsonPropertyName("id")]
    public ulong ID { get; set; } = 0;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }
    [JsonPropertyName("size")]
    public ulong Size { get; set; } = 0;
    [JsonPropertyName("browser_download_url")]
    public Uri? BrowserDownloadURL { get; set; }
}
