namespace HedgeModManager.Updates;
using System.Text;
using Text;

public class ModUpdateClient : HttpClient
{
    public const string VersionFileName = "mod_version.ini";
    private DataCache mCache;
    public ModUpdateClient(Uri baseUri)
    {
        BaseAddress = baseUri;
    }

    public async Task<string> GetLatestVersion(CancellationToken cancellationToken = default)
    {
        if (mCache.Version != null)
        {
            return mCache.Version;
        }

        await FillCache(cancellationToken);
        return mCache.Version!;
    }

    public async Task<string> GetChangelog(CancellationToken cancellationToken = default)
    {
        if (mCache.Changelog != null)
        {
            return mCache.Changelog;
        }

        await FillCache(cancellationToken);
        if (!string.IsNullOrEmpty(mCache.ChangelogPath))
        {
            var response = await GetAsync(mCache.ChangelogPath, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                mCache.Changelog = string.Empty;
                return mCache.Changelog;
            }

            mCache.Changelog = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        return mCache.Changelog ?? string.Empty;
    }

    public void Invalidate()
    {
        mCache = default;
    }

    private async Task FillCache(CancellationToken cancellationToken)
    {
        var response = await GetAsync(VersionFileName, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var info = Ini.FromText(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        if (info.TryGetValue("Main", out var mainSection))
        {
            mCache.Version = mainSection.Get("VersionString", string.Empty);
            mCache.ChangelogPath = mainSection.Get<string?>("Markdown");
        }

        if (string.IsNullOrEmpty(mCache.ChangelogPath) && info.TryGetValue("Changelog", out var changelogGroup))
        {
            var logBuilder = new StringBuilder();
            var logs = changelogGroup.GetList<string>("String");

            foreach (var log in logs)
            {
                logBuilder.Append($"- {log.Replace("\\n", "\n")}");
            }

            mCache.Changelog = logBuilder.ToString();
        }

        mCache.Version ??= string.Empty;
    }

    private struct DataCache
    {
        public string? Version = null;
        public string? Changelog = null;
        public string? ChangelogPath = null;

        public DataCache()
        {
        }
    }
}