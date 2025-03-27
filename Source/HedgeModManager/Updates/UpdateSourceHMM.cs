namespace HedgeModManager.Updates;
using CoreLib;
using Foundation;
using System.Threading;

public class UpdateSourceHMM : IUpdateSource
{
    public const string ManifestFileName = "update_manifest.json";

    public Uri BaseURI { get; set; }
    public string Host => BaseURI.Host;
    public IMod Mod { get; set; }
    public HMMUpdateManifest? Manifest { get; set; }

    public UpdateSourceHMM(IMod mod, Uri baseURI)
    {
        BaseURI = baseURI;
        Mod = mod;
    }

    public async Task<bool?> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        Manifest = await Network.Get<HMMUpdateManifest>(Helpers.CombineURL(BaseURI, ManifestFileName), HMMUpdate.JsonOptions, cancellationToken);
        if (Manifest == null)
        {
            Logger.Error("Failed to get update manifest!");
            return null;
        }
        return Manifest.ModVersion != Mod.Version;
    }

    public async Task<UpdateInfo?> GetUpdateInfoAsync(CancellationToken cancellationToken)
    {
        Manifest = await Network.Get<HMMUpdateManifest>(Helpers.CombineURL(BaseURI, ManifestFileName), HMMUpdate.JsonOptions, cancellationToken);
        if (Manifest == null)
        {
            Logger.Debug("Failed to get update manifest!");
            return null;
        }

        string? changeLog = null;
        if (!string.IsNullOrEmpty(Manifest.ChangelogPath))
        {
            Logger.Debug("Downloading changelog");
            changeLog = await Network.DownloadString(Manifest.ChangelogPath, cancellationToken);
        }

        return new UpdateInfo(Manifest.ModVersion, changeLog ?? string.Empty);
    }

    public async Task PerformUpdateAsync(IProgress<long>? progress, CancellationToken cancellationToken)
    {
        Logger.Debug("Beginning update...");
        if (Manifest == null)
        {
            Logger.Error("Cannot perform update with missing manifest!");
            return;
        }
        Manifest.BasePath ??= BaseURI.AbsoluteUri;
        var update = new HMMUpdate(Manifest);

        progress?.ReportMax(-1);
        progress?.Report(0);
        
        await update.PrepareForDownload(Mod.Root, progress, cancellationToken);
        Logger.Debug($"  Command count: {update.UpdateCommands.Count}");
        Logger.Debug($"  Base Path: {update.Manifest.BasePath}");
        await update.PerformAsync(Mod.Root, progress, cancellationToken);
    }
}