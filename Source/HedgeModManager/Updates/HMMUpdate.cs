using HedgeModManager.Foundation;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace HedgeModManager.Updates;

public class HMMUpdate(HMMUpdateManifest manifest)
{
    public const string MetadataDirectoryName = ".hmm";
    public readonly string[] LastFiles = ["mod.ini"];
    public readonly static JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HMMUpdateManifest Manifest { get; set; } = manifest;
    public List<HMMUpdateCommand> UpdateCommands { get; set; } = [];

    public async Task<bool> CreateFromDirectoryAsync(string modPath, CoreLib.IProgress<long>? progress, CancellationToken? c = default)
    {
        CancellationToken cancellationToken = c ?? CancellationToken.None;
        Logger.Debug("Creating update from files...");
        UpdateCommands.Clear();
        if (Manifest == null)
        {
            Logger.Error("Manifest is null!");
            return false;
        }
        Manifest.Files.Clear();

        Logger.Debug("Checking metadata files...");
        string ignorePath = Path.Combine(modPath, MetadataDirectoryName, "ignore_files.txt");
        string deletePath = Path.Combine(modPath, MetadataDirectoryName, "delete_files.txt");
        string[] ignoreFiles = [];
        string[] deleteFiles = [];
        if (File.Exists(ignorePath))
        {
            ignoreFiles = await File.ReadAllLinesAsync(ignorePath, cancellationToken);
        }
        else
        {
            Logger.Debug($"\"{MetadataDirectoryName}/ignore_files.txt\" is not found!");
        }
        if (File.Exists(deletePath))
        {
            deleteFiles = await File.ReadAllLinesAsync(deletePath, cancellationToken);
        }
        else
        {
            Logger.Debug($"\"{MetadataDirectoryName}/delete_files.txt\" is not found!");
        }

        // Extra ignores
        ignoreFiles = [.. ignoreFiles, "mod_files.txt", "mod_version.ini", "changelog.md", UpdateSourceHMM.ManifestFileName, $"{MetadataDirectoryName}/"];

        Logger.Debug("Scanning files...");
        var files = Directory.GetFiles(modPath, "*", SearchOption.AllDirectories);
        progress?.Report(0);
        progress?.ReportMax(files.Length);

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        await Parallel.ForEachAsync(files, options, async (localPath, cancellationToken) =>
        {
            using SHA256 sha256 = SHA256.Create();
            string relativePath = Path.GetRelativePath(modPath, localPath);
            string fileHash = await Helpers.GetFileHashAsync(localPath, sha256);
            long fileSize = new FileInfo(localPath).Length;
            if (ignoreFiles.Any(ignoreFile => Helpers.NormalizePath(relativePath).Contains(ignoreFile, StringComparison.InvariantCultureIgnoreCase)))
            {
                Logger.Debug($"  Ignored - \"{relativePath}\"");
                progress?.ReportAdd(1);
                return;
            }
            if (deleteFiles.Any(deleteFile => Helpers.NormalizePath(relativePath).Contains(deleteFile, StringComparison.InvariantCultureIgnoreCase)))
            {
                Logger.Debug($"  Deleted - \"{relativePath}\"");
                Logger.Error($"The file \"{relativePath}\" is marked to be deleted but exists within the mod files. This is inconsistent.");
                return;
            }

            Manifest.Files.Add(new()
            {
                Path = Helpers.NormalizePath(relativePath),
                Size = fileSize,
                SHA256 = fileHash
            });
            Logger.Debug($"  Added - \"{relativePath}\"");
            progress?.ReportAdd(1);
        });

        foreach (var deleteFile in deleteFiles)
        {
            Manifest.Files.Add(new()
            {
                Path = deleteFile,
                Size = -1
            });
            Logger.Debug($"  Deleted - \"{deleteFile}\"");
        }

        Logger.Debug("Reordering...");
        foreach (string fileName in LastFiles)
        {
            var entry = Manifest.Files.FirstOrDefault(x => x.Path.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (entry != null)
            {
                Manifest.Files.Remove(entry);
                Manifest.Files.Add(entry);
                Logger.Debug($"  Reordered - \"{entry.Path}\"");
            }
        }
        return true;
    }

    public async Task PrepareForDownload(string rootPath, CoreLib.IProgress<long>? progress, CancellationToken c)
    {
        Logger.Debug("Checking files...");
        UpdateCommands.Clear();
        if (Manifest == null)
        {
            Logger.Error("Manifest is null!");
            return;
        }
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        progress?.Report(0);
        progress?.ReportMax(Manifest.Files.Count);

        await Parallel.ForEachAsync(Manifest.Files, options, async (file, cancellationToken) =>
        {
            using SHA256 sha256 = SHA256.Create();
            string localPath = Path.Combine(rootPath, file.Path);
            var fileInfo = new FileInfo(localPath);
            bool fileExists = fileInfo.Exists;
            long fileSize = fileExists ? fileInfo.Length : 0;

            if (!fileExists)
            {
                if (file.Size == 0)
                {
                    UpdateCommands.Add(new(file, HMMUpdateCommandType.Create, null));
                    Logger.Debug($"  Create - \"{localPath}\"");
                }
                else
                {
                    UpdateCommands.Add(new(file, HMMUpdateCommandType.Download, Manifest.BasePath));
                    Logger.Debug($"  New - \"{localPath}\"");
                }
            }
            else if (fileExists && file.Size == -1)
            {
                UpdateCommands.Add(new(file, HMMUpdateCommandType.Delete, null));
                Logger.Debug($"  Delete - \"{localPath}\"");
            }
            else if (fileExists && file.Size > 0 && fileSize != file.Size)
            {
                UpdateCommands.Add(new(file, HMMUpdateCommandType.Download, null));
                Logger.Debug($"  Modified - \"{localPath}\"");
            }
            else if (fileExists && !string.IsNullOrEmpty(file.SHA256))
            {
                if (!await Helpers.CheckFileHashAsync(localPath, file.SHA256, sha256, c))
                {
                    UpdateCommands.Add(new(file, HMMUpdateCommandType.Download, null));
                    Logger.Debug($"  Modified - \"{localPath}\"");
                }
            }
            progress?.ReportAdd(1);
        });

        progress?.ReportMax(-1);

        // Remove duplicate downloads
        Logger.Debug("Checking for duplicate downloads...");
        var mainFileHashes = new Dictionary<string, HMMUpdateManifest.FileEntry>();
        foreach (var command in UpdateCommands)
        {
            if (command.CommandType == HMMUpdateCommandType.Download &&
                !string.IsNullOrEmpty(command.FileEntry.SHA256))
            {
                if (mainFileHashes.ContainsKey(command.FileEntry.SHA256))
                {
                    Logger.Debug($"  Duplicate - \"{command.FileEntry.Path}\"");
                    command.ExtraData = mainFileHashes[command.FileEntry.SHA256];
                    command.CommandType = HMMUpdateCommandType.Clone;
                }
                else
                {
                    mainFileHashes.Add(command.FileEntry.SHA256, command.FileEntry);
                }
            }
        }
    }

    public async Task PerformAsync(string rootPath, CoreLib.IProgress<long>? progress, CancellationToken? c)
    {
        CancellationToken cancellationToken = c ?? CancellationToken.None;
        progress?.ReportMax(UpdateCommands.Count);
        progress?.Report(0);
        foreach (var command in UpdateCommands)
        {
            await command.ProcessAsync(rootPath, cancellationToken);
            progress?.ReportAdd(1);
            if (cancellationToken.IsCancellationRequested)
            {
                Logger.Information("Update has been cancelled!");
                break;
            }
        }
    }
}
