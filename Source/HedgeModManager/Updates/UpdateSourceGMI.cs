namespace HedgeModManager.Updates;
using Foundation;
using CoreLib;
using System.Threading;

public class UpdateSourceGMI<TMod> : IUpdateSource where TMod : IMod
{
    public const string CommandListFileName = "mod_files.txt";

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

    public async Task PerformUpdateAsync(IProgress<long>? progress, CancellationToken cancellationToken)
    {
        Logger.Debug("Downloading command list...");
        var response = await Client.GetAsync(CommandListFileName, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        string text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var lines = text.Replace("\r", "").Trim().Split('\n');
        progress?.ReportMax(lines.Length);
        progress?.Report(0);
        Logger.Debug($"Received {lines.Length} lines (trimmed)");

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var line in lines)
        {
            Logger.Debug($"Processing line \"{line}\"");
            if (string.IsNullOrEmpty(line) || line.StartsWith(';') || line.StartsWith('#'))
                continue;

            if (!line.Contains(' '))
            {
                Logger.Error($"Invalid command line: {line}");
                continue;
            }
            string type = line[..line.IndexOf(' ')];
            string path = line[(line.IndexOf(' ') + 1)..];

            int attemptsLeft = 3;
            while (attemptsLeft > 0)
            {
                try
                {
                    await ProcessCommand(type, path, cancellationToken);
                    break;
                }
                catch (Exception ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    attemptsLeft--;
                    Logger.Error(ex);
                    if (attemptsLeft == 0)
                        Logger.Error("Command process failed 3 times, skipping!");
                    else
                    {
                        Logger.Error($"Command process failed, retrying ({attemptsLeft} attempts left)");
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
            progress?.ReportAdd(1);
        }
    }

    public async Task ProcessCommand(string type, string path, CancellationToken? c)
    {
        CancellationToken cancellationToken = c ?? CancellationToken.None;
        switch (type)
        {
            case "add":
                Logger.Information($"Downloading {path}...");
                var data = await Client.GetByteArrayAsync(path, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;
                string filePath = Path.Combine(Mod.Root, path);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                File.WriteAllBytes(filePath, data);
                break;
            case "mkdir":
                Logger.Debug($"Creating directory {path}...");
                Directory.CreateDirectory(Path.Combine(Mod.Root, path));
                break;
            case "pause":
                if (int.TryParse(path, out int timeout))
                {
                    Logger.Information($"Pausing for {timeout} miliseconds...");
                    await Task.Delay(timeout, cancellationToken);
                }
                else
                {
                    Logger.Debug($"Could not parse timeout for \"{path}\"");
                }
                break;
            case "print":
                Logger.Information($"[{Mod.Title}] {path}");
                break;
            case "delete":
                if (File.Exists(path))
                {
                    Logger.Debug($"Deleting file \"{path}\"");
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Logger.Debug($"Deleting directory \"{path}\"");
                    Directory.Delete(path, true);
                }
                break;
            default:
                Logger.Error($"Unknown command type: {type}");
                break;
        }
    }
}