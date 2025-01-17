using Avalonia;
using HedgeModManager.GitHub;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI;

public class Updater
{
    public static string GetUpdatePackageName()
    {
        if (OperatingSystem.IsWindows())
            return $"HedgeModManager.exe";
        else if (OperatingSystem.IsLinux())
            return $"HedgeModManager-linux-x64.zip";
        else
            return $"HedgeModManager-{RuntimeInformation.RuntimeIdentifier}.zip";
    }

    public static async Task<(Update?, UpdateCheckStatus)> CheckForUpdates()
    {
        // Check Flathub if running under a Flatpak
        if (!string.IsNullOrEmpty(Program.FlatpakID))
        {
            Logger.Debug($"Checking for updates for {Program.FlatpakID}");
            var app = await Network.Get<FlathubApp>($"https://flathub.org/api/v1/apps/{Program.FlatpakID}");
            if (app == null)
            {
                Logger.Error("Failed to check for updates");
                // TODO: Swap these nearing release
                //return (null, UpdateCheckStatus.Error);
                return (null, UpdateCheckStatus.NoUpdate);
            }
            Logger.Debug("Got Flathub data");
            Logger.Debug($"Current Version: {Program.ApplicationVersion}");
            Logger.Debug($"New Version: {app.CurrentReleaseVersion}");

            if (IsNewer(app.CurrentReleaseVersion, Program.ApplicationVersion))
            {
                Logger.Debug("Current version is older");
                return (new Update()
                {
                    Version = app.CurrentReleaseVersion!,
                    Title = app.Name,
                    Description = app.CurrentReleaseDescription,
                    IsSingleExecutable = false
                }, UpdateCheckStatus.UpdateAvailable);
            }

            Logger.Debug("Current version is newer");
            return (null, UpdateCheckStatus.NoUpdate);
        }
        else
        {
            var release = await GitHubAPI.GetRelease(Program.GitHubRepoOwner, Program.GitHubRepoName);

            if (release == null)
            {
                Logger.Error("Failed to check for updates");
                // TODO: Swap these nearing release
                //return (null, UpdateCheckStatus.Error);
                return (null, UpdateCheckStatus.NoUpdate);
            }

            Logger.Debug($"Current Version: {Program.ApplicationVersion}");
            Logger.Debug($"New Version: {release.TagName}");

            if (IsNewer(release.TagName, Program.ApplicationVersion))
            {
                Logger.Debug("Current version is older");
                // Find Asset
                var asset = release.Assets.FirstOrDefault(x => x!.Name.Equals(GetUpdatePackageName()),
                    release.Assets.FirstOrDefault(x => x.Name.Contains(RuntimeInformation.RuntimeIdentifier)));
                if (asset == null)
                {
                    Logger.Error($"No Assets found!");
                    return (null, UpdateCheckStatus.Error);
                }

                Logger.Debug($"Found asset {asset.Name} - {asset.BrowserDownloadURL}");

                return (new Update()
                {
                    Version = release.TagName,
                    Title = release.Name,
                    Description = release.Body,
                    DownloadURI = asset.BrowserDownloadURL,
                    IsSingleExecutable = OperatingSystem.IsWindows()
                }, UpdateCheckStatus.UpdateAvailable);
            }
            else
            {
                Logger.Debug("Current version is newer");
                return (null, UpdateCheckStatus.NoUpdate);
            }
        }
    }

    public static async Task BeginUpdate(Update update, MainWindowViewModel? mainViewModel)
    {
        if (update.DownloadURI == null)
        {
            Logger.Error("No download URI found!");
            return;
        }
        string tempPath = Paths.GetTempPath();
        string packageFileName = update.IsSingleExecutable ? "update.exe" : "update.zip";
        string packageFilePath = Path.Combine(tempPath, packageFileName);

        await new Download(Localize("Download.Text.UpdateManager0"), true)
        .OnRun(async (d, c) =>
        {
            var progress = d.CreateProgress();
            progress.ReportMax(-1);

            Logger.Debug($"Downloading to {packageFilePath}");
            bool completed = await Network.DownloadFile(update.DownloadURI, packageFilePath, null, progress, c);
            if (!completed)
            {
                Logger.Error($"Failed to download package");
                d.Destroy();
                return;
            }

            d.Name = Localize("Download.Text.UpdateManager1");
            progress.ReportMax(-1);

            await UpdateFromPackage(packageFilePath, mainViewModel);

            d.Destroy();
        }).OnError((d, e) =>
        {
            if (mainViewModel != null)
            {
                mainViewModel.OpenErrorMessage("Modal.Title.UpdateError", "Modal.Message.UnknownSaveError", "Unexpected error while preparing for update", e);
            }
            else
            {
                Logger.Error(e);
                Logger.Error($"Unexpected error while preparing for update");
            }
            d.Destroy();
            return Task.CompletedTask;
        }).RunAsync(mainViewModel);
    }

    public static async Task UpdateFromPackage(string packagePath, MainWindowViewModel? mainViewModel)
    {
        string tempPath = Paths.GetTempPath();
        string updateDirPath = Path.Combine(tempPath, $"Update");
        if (Directory.Exists(updateDirPath))
            Directory.Delete(updateDirPath, true);
        Directory.CreateDirectory(updateDirPath);

        Logger.Information($"Starting update from package \"{packagePath}\"");
        bool isSingleExecutable = packagePath.EndsWith(".exe");

        string args = $"--continue-update";
        args += $" {Process.GetCurrentProcess().Id}";
        args += $" 0";
        args += $" \"{Program.InstallLocation}\"";

        string exePath = Path.Combine(updateDirPath, "HedgeModManager.UI.exe");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            exePath = Path.Combine(updateDirPath, "HedgeModManager.UI");

        if (isSingleExecutable)
        {
            exePath = Path.Combine(updateDirPath, "update.exe");
            File.Copy(packagePath, exePath, true);
        }
        else
        {
            Logger.Debug($"Extracting to {updateDirPath}");
            // TODO: Extract async
            ZipFile.ExtractToDirectory(packagePath, updateDirPath);
            args += $" \"{Program.InstallLocation}\"";
        }

        if (mainViewModel != null)
        {
            Logger.Debug("Stopping server...");
            mainViewModel.StopServer();
            // Wait for it to stop
            while (mainViewModel.ServerStatus != 0)
                await Task.Delay(100);
        }
        Logger.Debug("Releasing Mutex...");
        Program.CurrentMutex?.ReleaseMutex();
        Program.CurrentMutex?.Dispose();
        Program.CurrentMutex = null;

        Logger.Debug($"Calling {exePath} {args}");
        Process.Start(new ProcessStartInfo(exePath, args) { UseShellExecute = true });

        // Exit
        if (Application.Current is App app && app.MainWindow != null)
            app.MainWindow.Close();
    }

    public static Version ConvertVersion(string formattedVersion)
    {
        Version defaultVersion = new Version(0, 0);
        string pattern = @"(\d+)\.(\d+)-(\d+)";
        var regex = new Regex(pattern);
        var match = regex.Match(formattedVersion);

        if (!match.Success)
        {
            Logger.Error($"Failed to process regex on string: {formattedVersion}");
            return defaultVersion;
        }
        if (match.Groups.Count != 4)
        {
            Logger.Error($"Invalid group count from regex on string: {formattedVersion}");
            return defaultVersion;
        }

        _ = Version.TryParse($"{match.Groups[1].Value}.{match.Groups[2].Value}.0.{match.Groups[3].Value}", out Version? version);
        return version ?? defaultVersion;
    }

    public static bool IsNewer(string? newFormattedVersion, string? oldFormattedVersion = null)
    {
        if (newFormattedVersion == null)
            return false;

        oldFormattedVersion ??= Program.ApplicationVersion;

        var oldVersion = ConvertVersion(oldFormattedVersion);
        var newVersion = ConvertVersion(newFormattedVersion);

        return newVersion > oldVersion;
    }

    public enum UpdateCheckStatus
    {
        NoUpdate,
        UpdateAvailable,
        Error
    }

    public class Update
    {
        public string Version { get; set; } = "0.0-0";
        public string Title { get; set; } = "Update Title";
        public string? Description { get; set; }
        public Uri? DownloadURI { get; set; }
        public bool IsSingleExecutable { get; set; }
    }

    public class FlathubApp
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "No Name";
        [JsonPropertyName("currentReleaseDescription")]
        public string? CurrentReleaseDescription { get; set; }
        [JsonPropertyName("currentReleaseVersion")]
        public string? CurrentReleaseVersion { get; set; }
    }
}
