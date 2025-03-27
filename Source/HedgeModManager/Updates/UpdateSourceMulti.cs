namespace HedgeModManager.Updates;
using CoreLib;
using Foundation;
using System.Threading;

public class UpdateSourceMulti : IUpdateSource
{
    public static List<Type> UpdateSources = [typeof(UpdateSourceHMM), typeof(UpdateSourceGMI)];
    public const string ManifestFileName = "update_manifest.json";

    public Uri BaseURI { get; set; }
    public string Host => BaseURI.Host;
    public IMod Mod { get; set; }
    public IUpdateSource? UpdateSource { get; set; }

    public UpdateSourceMulti(IMod mod, Uri baseURI)
    {
        BaseURI = baseURI;
        Mod = mod;
    }

    public async Task<bool?> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        bool? result = null;
        if (UpdateSource == null)
        {
            foreach (var source in UpdateSources)
            {
                var updateSource = Activator.CreateInstance(source, [Mod, BaseURI]) as IUpdateSource;
                if (updateSource != null)
                {
                    result = await updateSource.CheckForUpdatesAsync(cancellationToken);
                    if (result != null)
                    {
                        UpdateSource = updateSource;
                        Logger.Debug($"Using update source {source.Name} for \"{Mod.Title}\"");
                        return result;
                    }
                }
                else
                {
                    Logger.Error($"Failed to create update source for {source.Name}");
                }
                (UpdateSource as IDisposable)?.Dispose();
            }
            return null;
        }
        return await UpdateSource.CheckForUpdatesAsync(cancellationToken);
    }

    public async Task<UpdateInfo?> GetUpdateInfoAsync(CancellationToken cancellationToken)
    {
        UpdateInfo? result = null;
        if (UpdateSource == null)
        {
            foreach (var source in UpdateSources)
            {
                var updateSource = Activator.CreateInstance(source, [Mod, BaseURI]) as IUpdateSource;
                if (updateSource != null)
                {
                    result = await updateSource.GetUpdateInfoAsync(cancellationToken);
                    if (result != null)
                    {
                        UpdateSource = updateSource;
                        Logger.Debug($"Using update source {source.Name} for \"{Mod.Title}\"");
                        return result;
                    }
                }
                else
                {
                    Logger.Error($"Failed to create update source for {source.Name}");
                }
                (UpdateSource as IDisposable)?.Dispose();
            }
            return null;
        }
        return await UpdateSource.GetUpdateInfoAsync(cancellationToken);
    }

    public async Task PerformUpdateAsync(IProgress<long>? progress, CancellationToken cancellationToken)
    {
        if (UpdateSource == null)
        {
            Logger.Error("Cannot perform update with without an update source!");
            return;
        }
        await UpdateSource.PerformUpdateAsync(progress, cancellationToken);
    }
}