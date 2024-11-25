using HedgeModManager.UI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HedgeModManager.UI;

public static class GameBanana
{
    public static Dictionary<string, string> GameIDMapping = new()
    {
        { "hedgemmgens", "SonicGenerations" },
        { "hedgemmlw", "SonicLostWorld" },
        { "hedgemmforces", "SonicForces" },
        { "hedgemmtenpex", "PuyoPuyoTetris2" },
        { "hedgemmmusashi", "Tokyo2020" },
        { "hedgemmrainbow", "SonicColorsUltimate" },
        { "hedgemmhite", "SonicOrigins" },
        { "hedgemmrangers", "SonicFrontiers" },
        { "hedgemmmillersonic", "SonicGenerations2024" },
        { "hedgemmmillershadow", "ShadowGenerations" },
    };

    public static async Task<ModDownloadInfo?> GetDownloadInfo(string schema, string downloadURL, string type, string id)
    {
        Logger.Information("Downloading item data from GameBanana...");
        var client = new HttpClient();
        var response = await client.GetAsync(ProfilePage.GetRequestURL(type, id));
        if (!response.IsSuccessStatusCode)
        {
            Logger.Error("Failed to download item data from GameBanana.");
            return null;
        }

        var jsonStr = await response.Content.ReadAsStringAsync();
        var profilePage = JsonSerializer.Deserialize<ProfilePage>(jsonStr);
        if (profilePage == null)
        {
            Logger.Error("Failed to parse item data from GameBanana.");
            return null;
        }

        var authors = new Dictionary<string, List<ModDownloadInfo.Author>>();

        foreach (var group in profilePage.Authors)
        {
            var authorList = new List<ModDownloadInfo.Author>();
            foreach (var author in group.Authors)
            {
                authorList.Add(new()
                {
                    Name = author.Name,
                    Link = author.ProfileURL,
                    Description = author.Role
                });
            }
            authors.TryAdd(group.Name, authorList);
        }

        var downloadInfo = new ModDownloadInfo()
        {
            GameID = GameIDMapping[schema],
            Name = profilePage.Name,
            Description = profilePage.Description,
            Authors = authors,
            Images = profilePage.PreviewMedia.Images.Select(x => $"{x.BaseURL}/{x.FilePath}").ToList(),
            DownloadURL = downloadURL,
        };

        return downloadInfo;
    }

    public class ProfilePage
    {
        [JsonPropertyName("_idRow")]
        public int ID { get; set; }
        [JsonPropertyName("_aPreviewMedia")]
        public PreviewMedia PreviewMedia { get; set; } = new();
        [JsonPropertyName("_sName")]
        public string Name { get; set; } = "No Name";
        [JsonPropertyName("_sDescription")]
        public string Subtitle { get; set; } = string.Empty;
        [JsonPropertyName("_sText")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("_aCredits")]
        public List<CreditGroup> Authors { get; set; } = [];

        public static string GetRequestURL(string type, string id)
        {
            return $"https://gamebanana.com/apiv11/{type}/{id}/ProfilePage";
        }
    }

    public class CreditGroup
    {
        [JsonPropertyName("_sGroupName")]
        public string Name { get; set; } = "Group Name";
        [JsonPropertyName("_aAuthors")]
        public List<CreditAuthor> Authors { get; set; } = [];
    }

    public class CreditAuthor
    {
        [JsonPropertyName("_sRole")]
        public string Role { get; set; } = "Role Name";
        [JsonPropertyName("_sName")]
        public string Name { get; set; } = "No Name";
        [JsonPropertyName("_sProfileUrl")]
        public string ProfileURL { get; set; } = string.Empty;
    }

    public class PreviewMedia
    {
        [JsonPropertyName("_aImages")]
        public List<Image> Images { get; set; } = [];
    }
    
    public class Image
    {
        [JsonPropertyName("_sType")]
        public string Type { get; set; } = "screenshot";
        [JsonPropertyName("_sBaseUrl")]
        public string BaseURL { get; set; } = string.Empty;
        [JsonPropertyName("_sCaption")]
        public string Caption { get; set; } = string.Empty;
        [JsonPropertyName("_sFile")]
        public string FilePath { get; set; } = string.Empty;
    }
}