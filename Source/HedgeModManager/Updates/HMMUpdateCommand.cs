using HedgeModManager.Foundation;

namespace HedgeModManager.Updates;

public class HMMUpdateCommand(HMMUpdateManifest.FileEntry fileEntry, HMMUpdateCommandType commandType, string? baseHost)
{
    public HMMUpdateManifest.FileEntry FileEntry { get; set; } = fileEntry;
    public HMMUpdateCommandType CommandType { get; set; } = commandType;
    public object? ExtraData { get; set; }
    public string? BaseHost { get; set; } = baseHost;

    public async Task ProcessAsync(string root, CancellationToken? c)
    {
        CancellationToken cancellationToken = c ?? CancellationToken.None;
        string localPath = Path.Combine(root, FileEntry.Path);
        switch (CommandType)
        {
            case HMMUpdateCommandType.Download:
                string? remotePath = FileEntry.DownloadPaths?.FirstOrDefault();
                if (string.IsNullOrEmpty(remotePath))
                {
                    if (!string.IsNullOrEmpty(BaseHost))
                    {
                        remotePath = Helpers.CombineURL(BaseHost, Helpers.EncodeURL(FileEntry.Path));
                    }
                    else
                    {
                        Logger.Error("Remote path is null and base host is null! Command cannot be processed");
                        return;
                    }
                }
                await Network.DownloadFile(remotePath, localPath, null, null, cancellationToken);
                break;
            case HMMUpdateCommandType.Delete:
                if (File.Exists(localPath))
                {
                    Logger.Debug($"Deleting file \"{localPath}\"");
                    File.Delete(localPath);
                }
                else if (Directory.Exists(localPath))
                {
                    Logger.Debug($"Deleting directory \"{localPath}\"");
                    Directory.Delete(localPath, true);
                }
                break;
            case HMMUpdateCommandType.Create:
                if (localPath.Contains('.'))
                {
                    Logger.Debug($"Creating file {localPath}...");
                    File.Create(localPath).Close();
                }
                else
                {
                    Logger.Debug($"Creating directory {localPath}...");
                    Directory.CreateDirectory(localPath);
                }
                break;
            case HMMUpdateCommandType.Clone:
                try
                {
                    if (ExtraData is HMMUpdateManifest.FileEntry otherFile)
                    {
                        string otherFileLocalPath = Path.Combine(root, otherFile.Path);
                        if (File.Exists(otherFileLocalPath))
                        {
                            Logger.Debug($"Copying file \"{otherFileLocalPath}\" to \"{localPath}\"");
                            File.Copy(otherFileLocalPath, localPath, true);
                        }
                        else
                        {
                            Logger.Error($"File \"{otherFileLocalPath}\" does not exist!");
                        }

                    }
                    Logger.Error("Extra data is not a file entry!");
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                break;
            default:
                Logger.Error($"Unknown command type: {CommandType}");
                break;
        }
    }
}
