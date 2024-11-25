using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HedgeModManager.UI.Models;

public class ModDownloadInfo
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "mod";
    [JsonPropertyName("game")]
    public string GameID { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Mod Name";
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    [JsonPropertyName("authors")]
    public Dictionary<string, List<Author>> Authors { get; set; } = [];
    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = [];
    [JsonPropertyName("download")]
    public string DownloadURL { get; set; } = string.Empty;

    public class Author
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
