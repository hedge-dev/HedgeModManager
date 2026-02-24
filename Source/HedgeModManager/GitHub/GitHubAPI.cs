namespace HedgeModManager.GitHub;

public class GitHubAPI
{
    public const string EndpointURL = "https://api.github.com";

    public static string BuildPath(params string[] paths)
    {
        return $"{EndpointURL}/{string.Join("/", paths)}";
    }

    public static async Task<GitHubRelease?> GetRelease(string owner, string repo, string releaseID = "latest")
    {
        return await Network.Get<GitHubRelease>(BuildPath("repos", owner, repo, "releases", releaseID));
    }
}
