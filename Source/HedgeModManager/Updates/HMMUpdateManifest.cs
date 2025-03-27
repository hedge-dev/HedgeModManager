using System.Text.Json.Serialization;

namespace HedgeModManager.Updates;

public class HMMUpdateManifest
{
    public int ManifestVersion { get; set; } = 1;
    public string ModVersion { get; set; } = "1.0.0";
    public string? BasePath { get; set; }
    public string? ChangelogPath { get; set; }
    public List<FileEntry> Files { get; set; } = [];

    public class FileEntry
    {
        public required string Path { get; set; }
        public string[]? DownloadPaths { get; set; }
        [JsonPropertyName("sha256")]
        public string? SHA256 { get; set; }
        public long? Size { get; set; }
    }
}
