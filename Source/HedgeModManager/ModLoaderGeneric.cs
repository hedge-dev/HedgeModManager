using HedgeModManager.CoreLib;
using HedgeModManager.Foundation;
using HedgeModManager.IO;
using HedgeModManager.Properties;
using HedgeModManager.Text;
using PeNet;
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
        DownloadFileName = $"{PathEx.CleanFileName(Name)}{extention}";
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

    public async Task<bool> Download(IProgress<long>? progress, string path, bool useCache)
    {
        if (!string.IsNullOrEmpty(DownloadURL))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            Logger.Information($"Downloading {Name}...");
            await Network.DownloadFile(DownloadURL, path, useCache ? Path.Combine("ModLoaders", DownloadFileName) : null, progress);
            return true;
        }
        Logger.Error($"Failed to download mod loader. No download URL");
        return false;
    }

    public string? GetInstalledVersion()
    {
        if (string.IsNullOrEmpty(FileName))
        {
            Logger.Debug("Null mod loader requested");
            return null;
        }

        if (!File.Exists(GetInstallPath()))
        {
            Logger.Debug("Mod loader not found");
            return null;
        }

        var peFile = new PeFile(GetInstallPath());
        var resources = peFile.Resources;
        if (resources != null)
        {
            Logger.Debug("Found resources");
            var versionInfo = resources.VsVersionInfo;
            if (versionInfo != null)
            {
                Logger.Debug("Found version info");
                uint ls = versionInfo.VsFixedFileInfo.DwProductVersionLS;
                uint ms = versionInfo.VsFixedFileInfo.DwProductVersionMS;
                string versionStr = $"{ms >> 16}.{ms & 0xFFFF}.{ls >> 16}.{ls & 0xFFFF}";
                Logger.Debug($"Version: {versionStr}");
                return versionStr;
            }
            else
            {
                Logger.Debug("No version info found");
            }
        }
        else
        {
            Logger.Debug("No resources found");
        }
        return null;
    }

    public async Task<bool> CheckForUpdatesAsync()
    {
        // Currently using HMM 7 mod loader updates
        string? manifestData = await Network.DownloadString(Resources.ModLoaderManifestURL);
        if (string.IsNullOrEmpty(manifestData))
        {
            Logger.Error("Failed to download mod loader manifest");
            return false;
        }

        var ini = Ini.FromText(manifestData);
        if (ini == null)
        {
            Logger.Error("Failed to parse mod loader manifest");
            return false;
        }

        if (ini.TryGetValue(Name, out var group))
        {
            string version = group.Get<string>("LoaderVersion");
            var fullVersion = string.Join('.', version.Split('.').Concat(Enumerable.Repeat("0", 4)).Take(4));
            //string changeLog = group.Get<string>("LoaderChangeLog");
            return fullVersion != GetInstalledVersion();
        }

        return false;
    }

    public async Task<bool> InstallAsync() => await InstallAsync(true);

    public async Task<bool> InstallAsync(bool useCache)
    {
        // Prepare prefix
        if (!OperatingSystem.IsWindows() && NativeOS == "Windows")
        {
            Logger.Information("Setting up prefix...");
            string? prefix = Game.PrefixRoot;
            Logger.Debug($"Prefix: {prefix}");
            bool isPrefixValid = LinuxCompatibility.IsPrefixValid(prefix);
            Logger.Debug($"IsPrefixValid: {isPrefixValid}");
            if (Is64Bit)
            {
                bool isPrefixPatched = LinuxCompatibility.IsPrefixPatched(prefix);
                Logger.Debug($"IsPrefixPatched: {isPrefixPatched}");
                if (isPrefixValid && !isPrefixPatched)
                {
                    Logger.Information("Applying patches to prefix...");
                    await LinuxCompatibility.InstallRuntimeToPrefix(prefix);
                }
            }
            if (isPrefixValid)
            {
                Logger.Information("Applying overrides to prefix...");
                await LinuxCompatibility.AddDllOverride(prefix, FileName.Replace(".dll", ""));
            }
            else
            {
                Logger.Error("Prefix missing! Please run the game atleast once");
                // Abort
                return false;
            }
        }

        string path = GetCachePath();
        if (!await Download(null, path, useCache))
        {
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
                Directory.CreateDirectory(Path.GetDirectoryName(GetInstallPath())!);
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
