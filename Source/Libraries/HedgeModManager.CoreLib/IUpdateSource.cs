namespace HedgeModManager.CoreLib;
using Foundation;

public interface IUpdateSource
{
    public string Host { get; }
    Task<bool?> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
    Task<UpdateInfo?> GetUpdateInfoAsync(CancellationToken cancellationToken = default);
    Task PerformUpdateAsync(IProgress<long>? progress, CancellationToken cancellationToken = default);
}