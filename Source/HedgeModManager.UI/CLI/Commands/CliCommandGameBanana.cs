using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.CLI.Commands;

[CliCommand("gamebanana", "gb", [typeof(string)], "Downloads mod from GameBanana (UI)", "--gamebanana \"URI\"")]
public class CliCommandGameBanana : ICliCommand
{
    public string? SchemaName;
    public string? DownloadURL;
    public string? ItemType;
    public string? ItemID;

    public bool Execute(List<CommandLine.Command> commands, CommandLine.Command command)
    {
        string uri = (string)command.Inputs[0];

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
        return true;
    }

    public Task ExecuteUI(MainWindowViewModel mainWindowViewModel)
    {
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
