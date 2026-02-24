using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HedgeModManager.Foundation;
using HedgeModManager.GameBanana;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using static HedgeModManager.UI.CLI.CommandLine;

namespace HedgeModManager.UI.CLI.Commands;

[CliCommand("schema", "gb", [typeof(string)], "Process hedgemm or GameBanana schema", "--schema \"URI\"")]
public class CliCommandSchema : ICliCommand
{
    // ModDownload Parameters
    public string? ManifestURL;

    // GameBanana Parameters
    public string? GameID;
    public string? DownloadURL;
    public string? ItemType;
    public string? ItemID;

    public Dictionary<string, Tuple<Func<List<Command>, Command, bool>?, Func<MainWindowViewModel, string, Task<bool>>?>> EndPoints = [];
    public Dictionary<string, Func<MainWindowViewModel, string, Task<bool>>> ToExecuteUI = [];

    // hedgemm://gamebanana/remotedl/register/1605939/TEST

    public CliCommandSchema()
    {
        EndPoints["hedgemm://install"] = new(ExecuteHMMInstall, null);
        EndPoints["hedgemm://gamebanana/pair"] = new(null, ExecuteUIGameBananaPair);
        EndPoints["hedgemm://gamebanana/install"] = new(ExecuteGameBananaInstall, ExecuteUIGameBanana);
        foreach (var mapping in GameBanana.GameIDMappings)
            EndPoints[$"{mapping.Key}:"] = new(ExecuteGameBanana, ExecuteUIGameBanana);
    }

    public bool ExecuteGameBanana(List<Command> commands, Command command)
    {
        string uri = (string)command.Inputs[0];
        int schemaIndex = uri.IndexOf(':');
        if (schemaIndex == -1)
        {
            Console.WriteLine("Invalid URI format (schemaIndex == -1)");
            return false;
        }
        string[] parts = uri[(schemaIndex + 1)..].Split(',');
        if (parts.Length != 3)
        {
            Console.WriteLine("Invalid URI format (parts != 3)");
            return false;
        }

        GameID = uri[0..schemaIndex];
        DownloadURL = parts[0].Replace("https//", "https://");
        ItemType = parts[1];
        ItemID = parts[2];
        return true;
    }

    public bool ExecuteGameBananaInstall(List<Command> commands, Command command)
    {
        string uri = (string)command.Inputs[0];
        int dataIndex = uri.IndexOf("install/");
        if (dataIndex == -1)
        {
            Console.WriteLine("Invalid URI format (dataIndex == -1)");
            return false;
        }
        string[] parts = uri[(dataIndex + 8)..].Split(',');
        if (parts.Length != 4)
        {
            Console.WriteLine("Invalid URI format (parts != 4)");
            return false;
        }

        GameID = parts[0];
        DownloadURL = parts[1].Replace("https//", "https://");
        ItemType = parts[2];
        ItemID = parts[3];

        // Convert GameBanana ID to HMM
        if (GameBananaAPI.GameIDMappings.TryGetValue(GameID, out var gameID))
            GameID = gameID;

        return true;
    }

    public Task<bool> ExecuteUIGameBanana(MainWindowViewModel mainWindowViewModel, string _)
    {
        if (GameID == null || DownloadURL == null || ItemType == null || ItemID == null)
        {
            Logger.Error("Invalid command state");
            return Task.FromResult(false);
        }
        new ModDownloaderModal(async () =>
        {
            // Load GameBanana data
            return await GameBanana.GetDownloadInfo(GameID ?? string.Empty, DownloadURL, ItemType, ItemID);
        }).Open(mainWindowViewModel);
        return Task.FromResult(true);
    }

    public async Task<bool> ExecuteUIGameBananaPair(MainWindowViewModel mainWindowViewModel, string uri)
    {
        var splits = uri.TrimEnd('/').Split('/');
        if (splits.Length < 2)
        {
            Logger.Error("Invalid command state");
            return false;
        }
        string id = splits[^2];
        string key = splits[^1];

        // Set config
        if (mainWindowViewModel?.Config is ProgramConfig config)
        {
            config.Integrations.GameBananaRemoteDLMemberID = id;
            config.Integrations.GameBananaRemoteDLSecretKey = key;
            config.Integrations.GameBananaRemoteDLEnabled = true;
            _ = mainWindowViewModel.GameBananaRemoteDLServerInstance.StartServerAsync();
            await config.SaveAsync();
        }
        return true;
    }

    public bool ExecuteHMMInstall(List<Command> commands, Command command)
    {
        string uri = (string)command.Inputs[0];
        string path = uri["hedgemm://".Length..];
        if (path.StartsWith("install/"))
            ManifestURL = uri["hedgemm://install/".Length..];
        else
            return false;
        return true;
    }

    public bool Execute(List<Command> commands, Command command)
    {
        string uri = (string)command.Inputs[0];
        foreach (var endPoint in EndPoints)
        {
            if (uri.StartsWith(endPoint.Key) || endPoint.Key == "*")
            {
                if (endPoint.Value.Item1 != null && !endPoint.Value.Item1(commands, command))
                    break;
                if (endPoint.Value.Item2 != null)
                    ToExecuteUI.Add(uri, endPoint.Value.Item2);
            }
        }
        return true;
    }

    public Task ExecuteUI(MainWindowViewModel mainWindowViewModel)
    {
        foreach (var pair in ToExecuteUI)
            pair.Value(mainWindowViewModel, pair.Key);

        // ModDownload
        if (ManifestURL != null)
        {
            new ModDownloaderModal(
                async () => await Network.Get<ModDownloadInfo>(ManifestURL))
                .Open(mainWindowViewModel);
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
