using System.Text.Json.Serialization;

namespace HedgeModManager.GitHub;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "No Tag";
    [JsonPropertyName("name")]
    public string Name { get; set; } = "No Name";
    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; } = false;
    [JsonPropertyName("published_at")]
    public DateTimeOffset PublishedAt { get; set; }
    [JsonPropertyName("assets")]
    public List<GitHubAsset> Assets { get; set; } = [];
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}
