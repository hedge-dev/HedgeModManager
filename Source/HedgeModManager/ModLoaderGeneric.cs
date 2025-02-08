using HedgeModManager.CoreLib;
using HedgeModManager.Foundation;
using HedgeModManager.IO;
using System.IO.Compression;

namespace HedgeModManager;

public class ModLoaderGeneric : IModLoader
{
    public string Name { get; set; } = "None";
    public IModdableGame Game { get; init; }
    
    public string FileName { get; set; }
    public string? DownloadURL { get; set; }
    public string DownloadFileName { get; set; }
    public string NativeOS { get; set; } = "Windows";
    public bool Is64Bit { get; set; } = true;

    public ModLoaderGeneric(IModdableGame game, string name, string? fileName, string? downloadURL, bool is64Bit)
    {
        Game = game;
        Name = name;
        FileName = fileName ?? $"{Name}.bin";
        DownloadURL = downloadURL;
        string extention = Path.GetExtension(DownloadURL) ?? ".dll";
        DownloadFileName = $"{PathEx.CleanFileName(Name)}.{extention}";
        Is64Bit = is64Bit;
    }

    public string GetCachePath()
    {
        return Path.Combine(Paths.GetCachePath(), "ModLoaders", DownloadFileName);
    }

    public string GetInstallPath()
    {
        return Path.Combine(Game.Root, FileName);
    }

    public async Task<bool> Download(IProgress<long>? progress, string path)
    {
        if (!string.IsNullOrEmpty(DownloadURL))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            Logger.Information($"Downloading {Name}...");
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(DownloadURL, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.Error($"Failed to download mod loader. Code: {response.StatusCode}");
                    return false;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(path);

                var buffer = new byte[1048576];
                int bytesRead = 0;
                long totalBytesRead = 0L;

                while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    progress?.Report(totalBytesRead += bytesRead);
                }

                Logger.Information($"Finished downloading {Name}");
            }
            catch (Exception e)
            {
                // Clean up
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                Logger.Error($"Failed to download mod loader");
                Logger.Error(e);
                return false;
            }
            return true;
        }
        Logger.Error($"Failed to download mod loader. No download URL");
        return false;
    }

    public async Task<bool> InstallAsync()
    {
        // Prepare prefix
        if (OperatingSystem.IsLinux() && NativeOS == "Windows" && Is64Bit)
        {
            Logger.Information("Setting up prefix...");
            string? prefix = Game.PrefixRoot;
            Logger.Debug($"Prefix: {prefix}");
            if (LinuxCompatibility.IsPrefixValid(prefix))
            {
                await LinuxCompatibility.InstallRuntimeToPrefix(prefix);
                Logger.Information("Applying patches to prefix...");
                await LinuxCompatibility.AddDllOverride(prefix, FileName.Replace(".dll", ""));
            }
            else
            {
                Logger.Error("Prefix missing! Please run the game atleast once");
                // Abort
                return false;
            }
        }

        string path = GetInstallPath();
        if (await Download(null, path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetCachePath())!);
            File.Copy(path, GetCachePath(), true);
        }
        else
        {
            path = GetCachePath();
            if (!File.Exists(path))
            {
                Logger.Error($"No mod loader found in cache!");
                return false;
            }
            Logger.Information("Using cached mod loader");
        }

        if (DownloadFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var fileStream = File.OpenRead(path);
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            Logger.Debug("Opened zip");

            foreach (var entry in archive.Entries)
            {
                string destinationPath = Path.Combine(Game.Root, entry.FullName);

                if (entry.FullName.EndsWith('/'))
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                // Rename mod loader file
                if (entry.FullName.EndsWith($"{Name}.dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.Debug($"Renamed {PathEx.GetFileName(entry.FullName)} to {FileName}");
                    destinationPath = GetInstallPath();
                }

                // Try delete first
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                entry.ExtractToFile(destinationPath, true);
            }
            Logger.Debug("Extracted zip");
        }
        else
        {
            if (path != GetInstallPath())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.Copy(path, GetInstallPath(), true);
            }
        }

        Logger.Information("Completed mod loader installation");
        return true;
    }

    public bool IsInstalled()
    {
        if (string.IsNullOrEmpty(FileName))
        {
            return true;
        }

        return File.Exists(GetInstallPath());
    }

    public Task<bool> UninstallAsync()
    {
        if (string.IsNullOrEmpty(FileName))
        {
            return Task.FromResult(true);
        }

        File.Delete(GetInstallPath());
        return Task.FromResult(true);
    }
}
