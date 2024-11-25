using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        string[] sections = uri.Split(':');
        if (sections.Length < 2)
        {
            Console.WriteLine("Invalid URI format (sections < 2)");
            return true;
        }
        string[] parts = uri.Split(',');
        if (parts.Length != 3)
        {
            Console.WriteLine("Invalid URI format (parts != 3)");
            return true;
        }

        SchemaName = sections[0];
        DownloadURL = parts[0];
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

        return Task.CompletedTask;
    }
}
