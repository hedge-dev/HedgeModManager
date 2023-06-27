namespace HedgeModManager.Updates;
using Foundation;

public class UpdateSourceGMI<TMod> : IUpdateSource where TMod : IMod
{
    public Uri Url { get; set; }
    public string Host => Url.Host;
    public ModUpdateClient Client { get; set; }
    public TMod Mod { get; set; }

    public UpdateSourceGMI(TMod mod, Uri url)
    {
        Url = url;
        Mod = mod;
        Client = new ModUpdateClient(url);
    }

    public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        var latest = await Client.GetLatestVersion(cancellationToken);
        return latest != Mod.Version;
    }

    public async Task<UpdateInfo> GetUpdateInfoAsync(CancellationToken cancellationToken)
    {
        var latest = await Client.GetLatestVersion(cancellationToken);
        var changelog = await Client.GetChangelog(cancellationToken);

        return new UpdateInfo(latest, changelog);
    }

    public Task PerformUpdateAsync(CancellationToken cancellationToken)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }
}