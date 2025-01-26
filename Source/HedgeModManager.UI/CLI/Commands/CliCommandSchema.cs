using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.CLI.Commands;

[CliCommand("schema", "gb", [typeof(string)], "Process hedgemm or GameBanana schema", "--schema \"URI\"")]
public class CliCommandSchema : ICliCommand
{
    // ModDownload Parameters
    public string? ManifestURL;

    // GameBana Parameters
    public string? SchemaName;
    public string? DownloadURL;
    public string? ItemType;
    public string? ItemID;


    public bool Execute(List<CommandLine.Command> commands, CommandLine.Command command)
    {
        string uri = (string)command.Inputs[0];
        if (uri.StartsWith("hedgemm://"))
        {
            string path = uri["hedgemm://".Length..];
            if (path.StartsWith("install/"))
                ManifestURL = uri["hedgemm://install/".Length..];
        }
        else
        {
            int schemaIndex = uri.IndexOf(":");
            if (schemaIndex == -1)
            {
                Console.WriteLine("Invalid URI format (schemaIndex == -1)");
                return true;
            }
            string[] parts = uri[(schemaIndex + 1)..].Split(',');
            if (parts.Length != 3)
            {
                Console.WriteLine("Invalid URI format (parts != 3)");
                return true;
            }

            SchemaName = uri[0..schemaIndex];
            DownloadURL = parts[0].Replace("https//", "https://");
            ItemType = parts[1];
            ItemID = parts[2];
        }
        return true;
    }

    public Task ExecuteUI(MainWindowViewModel mainWindowViewModel)
    {
        // ModDownload
        if (ManifestURL != null)
        {
            new ModDownloaderModal(
                async () => await Network.Get<ModDownloadInfo>(ManifestURL))
                .Open(mainWindowViewModel);
        }
        else
        {
            // GameBanana
            if (SchemaName == null || DownloadURL == null || ItemType == null || ItemID == null)
            {
                Logger.Error("Invalid command state");
                return Task.CompletedTask;
            }
            new ModDownloaderModal(async () =>
            {
                // Load GameBanana data
                return await GameBanana.GetDownloadInfo(SchemaName, DownloadURL, ItemType, ItemID);
            }).Open(mainWindowViewModel);
        }
        // Put window on top
        if (Application.Current?.ApplicationLifetime 
            is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is Window mainWindow)
        {
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;
        }

        return Task.CompletedTask;
    }
}
