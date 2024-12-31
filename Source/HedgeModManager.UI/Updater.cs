using Avalonia;
using Avalonia.Markup.Xaml.Templates;
using HedgeModManager.GitHub;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI;

public class Updater
{
    public static string GetUpdatePackageName()
    {
        return $"HedgeModManager-{RuntimeInformation.RuntimeIdentifier}.zip";
    }

    public static async Task<GitHubRelease?> CheckForUpdates()
    {
        var (release, _) = await GitHubAPI.GetRelease(Program.GitHubRepoOwner, Program.GitHubRepoName);
        return release;
    }

    public static async Task BeginUpdate(GitHubRelease release, MainWindowViewModel? mainViewModel)
    {
        string tempPath = Cache.GetTempPath();
        string packageFilePath = Path.Combine(tempPath, $"update.zip");

        await new Download(Localize("Download.Text.UpdateManager0"), true)
        .OnRun(async (d, c) =>
        {
            var progress = d.CreateProgress();
            progress.ReportMax(-1);

            // Find update file
            Logger.Debug($"Looking for asset {GetUpdatePackageName()} - {RuntimeInformation.RuntimeIdentifier}");
            var asset = release.Assets.FirstOrDefault(x => x!.Name.Equals(GetUpdatePackageName()), 
                release.Assets.FirstOrDefault(x => x.Name.Contains(RuntimeInformation.RuntimeIdentifier)));
            if (asset == null)
            {
                Logger.Error($"No Assets found!");
                d.Destroy();
                return;
            }

            Logger.Debug($"Found asset {asset.Name} - {asset.BrowserDownloadURL}");
            Logger.Debug($"Downloading to {packageFilePath}");
            bool completed = await Network.DownloadFile(asset.BrowserDownloadURL, packageFilePath, null, progress, c);
            if (!completed)
            {
                Logger.Error($"Failed to download asset");
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
        string tempPath = Cache.GetTempPath();
        string updateDirPath = Path.Combine(tempPath, $"Update");
        if (Directory.Exists(updateDirPath))
            Directory.Delete(updateDirPath, true);
        Directory.CreateDirectory(updateDirPath);

        Logger.Information($"Starting update from package \"{packagePath}\"");
        Logger.Debug($"Extracting to {updateDirPath}");
        // TODO: Extract async
        ZipFile.ExtractToDirectory(packagePath, updateDirPath);

        string args = $"--continue-update";
        args += $" {Process.GetCurrentProcess().Id}";
        args += $" 0";
        args += $" \"{Program.InstallLocation}\"";
        string exePath = Path.Combine(updateDirPath, "HedgeModManager.UI.exe");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            exePath = Path.Combine(updateDirPath, "HedgeModManager.UI");

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
}
