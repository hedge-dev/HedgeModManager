using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.CLI;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HedgeModManager.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string AppVersion => App.GetAppVersion();

    public ObservableCollection<UIGame> Games { get; set; } = new();
    public ProgramConfig Config { get; set; } = new();

    [ObservableProperty] private UIGame? _selectedGame;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private UILogger? _loggerInstance;
    [ObservableProperty] private string _lastLog = "";
    [ObservableProperty] private TabInfo? _currentTabInfo;
    [ObservableProperty] private TabInfo[] _tabInfos = 
        [new ("Loading"), new("Setup"), new("Mods"), new("Codes"), new("Settings"), new("About"), new("Test")];
    [ObservableProperty] private ObservableCollection<Modal> _modals = new ();
    [ObservableProperty] private bool _isBusy = true;

    // Preview only
    public MainWindowViewModel() { }

    public MainWindowViewModel(UILogger logger)
    {
        // Setup logger
        _loggerInstance = logger;
        new Logger(logger);
        logger.Logs.CollectionChanged += (sender, args) =>
        {
            if (logger.Logs.Count == 0)
                return;
            var lastLog = logger.Logs.LastOrDefault(x => x.Type != LogType.Debug);
            LastLog = lastLog == null ? "" : lastLog.Message;
        };
        Logger.Information($"Starting HedgeModManager {AppVersion}...");
        Logger.Debug($"IsWindows: {OperatingSystem.IsWindows()}");
        Logger.Debug($"IsLinux: {OperatingSystem.IsLinux()}");
    }

    public async Task LoadGame()
    {
        try
        {
            if (SelectedGame == null || SelectedGame.Game == null)
                return;

            var game = SelectedGame.Game;

            Logger.Debug($"Selected game:");
            Logger.Debug($"    ID: {game.ID}");
            Logger.Debug($"  Name: {game.Name}");
            Logger.Debug($"  Plat: {game.Platform}");
            Logger.Debug($"  Root: {game.Root}");
            Logger.Debug($"  Exec: {game.Executable}");
            Logger.Debug($"  N OS: {game.NativeOS}");
            await game.InitializeAsync();
            Logger.Debug($"Initialised game");

            Config.LastSelectedPath = Path.Combine(game.Root, game.Executable ?? "");

            Logger.Information($"Found {game.ModDatabase.Mods.Count} mods");
        }
        catch (Exception e)
        {
            OpenErrorMessage("Modal.Title.LoadError", "Modal.Message.GameLoadError",
                "Failed to load game/mod data", e);
        }
    }

    public async Task Save(bool setBusy = true)
    {
        if (setBusy)
            IsBusy = true;
        try
        {
            await Config.SaveAsync();
            if (SelectedGame != null)
            {
                try
                {
                    await SelectedGame.Game.ModDatabase.Save();
                    if (SelectedGame.Game.ModLoaderConfiguration is ModLoaderConfiguration config)
                        await config.Save(Path.Combine(SelectedGame.Game.Root, "cpkredir.ini"));
                }
                catch (UnauthorizedAccessException e)
                {
                    OpenErrorMessage("Modal.Title.SaveError", "Modal.Message.GameNoAccess",
                        "Failed to save mod database and config", e);
                    IsBusy = false;
                    return;
                }
                if (!SelectedGame.Game.IsModLoaderInstalled())
                    await SelectedGame.Game.InstallModLoaderAsync();
                else
                    Logger.Information("Saved");
            }
        }
        catch (Exception e)
        {
            OpenErrorMessage("Modal.Title.SaveError", "Modal.Message.UnknownSaveError",
                "Failed to save", e);

            MessageBoxModal.CreateOK("Modal.Title.SaveError", "Modal.Message.UnknownSaveError").Open(this);
            Logger.Error(e);
            Logger.Error("Failed to save");
        }
        if (setBusy)
            IsBusy = false;
    }

    public async Task RunGame()
    {
        if (SelectedGame != null)
        {
            try
            {
                await SelectedGame.Game.Run(null, true);
                // Add delay
                await Task.Delay(5000);
            }
            catch (Exception e)
            {
                OpenErrorMessage("Modal.Title.RunError", "Modal.Message.UnknownRunError",
                    "Failed to run game", e);
            }
        }
    }

    public async Task SaveAndRun()
    {
        IsBusy = true;
        await Save(false);
        await RunGame();
        IsBusy = false;
    }

    public void StartSetup()
    {
        Logger.Debug("Entered setup");
        Config.IsSetupCompleted = false;
        SelectedTabIndex = 1;
    }

    public void OpenErrorMessage(string title, string message, string logMessage, Exception? exception = null)
    {
        if (exception != null)
            Logger.Error(exception);
        Logger.Error(logMessage);
        var messageBox = new MessageBoxModal(title, message);
        messageBox.AddButton("Common.Button.OK", (s, e) => messageBox.Close());
        messageBox.AddButton("Modal.Button.ExportLog", async (s, e) =>
        {
            await ExportLog(messageBox);
            messageBox.Close();
        });
        messageBox.Open(this);
    }

    public async Task ExportLog(Visual visual)
    {
        string log = UILogger.Export();

        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null)
            return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
        {
            Title = "Save Log File",
            SuggestedFileName = "HedgeModManager.log",
            DefaultExtension = ".log",
            FileTypeChoices =
            [
                new("Log Files")
                {
                    Patterns = ["*.log"],
                    MimeTypes = ["text/plain"]
                }
            ]
        });

        if (file != null)
        {
            using var stream = await file.OpenWriteAsync();
            await stream.WriteAsync(Encoding.Default.GetBytes(log));
        }
    }

    public async Task ProcessCommands(List<ICliCommand> commands)
    {
        Logger.Debug($"Processing {commands.Count} command(s)");
        foreach (var command in commands)
        {
            Logger.Debug($"  Processing command: {command.GetType().Name}");
            await command.ExecuteUI(this);
        }
    }

    public async Task StartServer()
    {
        try
        {
            while (true)
            {
                using var server = new NamedPipeServerStream(Program.PipeName, PipeDirection.In, 
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync();
                Logger.Debug("Recieved connection");
                using var reader = new StreamReader(server);
                string message = await reader.ReadToEndAsync();
                Logger.Debug("Message read");
                var argsStr = JsonSerializer.Deserialize<string[]>(message);
                if (argsStr == null)
                    continue;
                Logger.Debug("Message deserialised");
                var args = CommandLine.ParseArguments(argsStr);
                var (continueStartup, commands) = CommandLine.ExecuteArguments(args);
                await ProcessCommands(commands);
                Logger.Debug("Message processed");
            }
        }
        catch (Exception e)
        {
            Logger.Error("Server stopped due to error");
            Logger.Error(e);
        }
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedGame))
            await LoadGame();
        base.OnPropertyChanged(e);
    }
}
