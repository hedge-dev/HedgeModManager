﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using HedgeModManager.UI.CLI;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Languages;
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
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string AppVersion => App.GetAppVersion();

    public ObservableCollection<UIGame> Games { get; set; } = [];
    public ObservableCollection<Download> Downloads { get; set; } = [];
    public ObservableCollection<IMod> Mods { get; set; } = [];
    public ObservableCollection<LanguageEntry> Languages { get; set; } = [];
    public ProgramConfig Config { get; set; } = new();

    [ObservableProperty] private UIGame? _selectedGame;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private UILogger? _loggerInstance;
    [ObservableProperty] private string _lastLog = "";
    [ObservableProperty] private TabInfo? _currentTabInfo;
    [ObservableProperty] private TabInfo[] _tabInfos = 
        [new ("Loading"), new("Setup"), new("Mods"), new("Codes"), new("Settings"), new("About"), new("Test")];
    [ObservableProperty] private ObservableCollection<Modal> _modals = [];
    [ObservableProperty] private bool _isBusy = true;
    [ObservableProperty] private double _overallProgress = 0d;
    [ObservableProperty] private double _overallProgressMax = 0d;
    [ObservableProperty] private bool _showProgressBar = false;
    [ObservableProperty] private LanguageEntry? _selectedLanguage;

    // Preview only
    public MainWindowViewModel() { }

    public MainWindowViewModel(UILogger logger, List<LanguageEntry> languages)
    {
        // Setup languages
        LanguageEntry.TotalLineCount = languages.Max(x => x.Lines);
        Languages = new ObservableCollection<LanguageEntry>(languages);

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

            RefreshUI();
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

    public async void RefreshUI()
    {
        if (SelectedGame is not UIGame game)
            return;

        await game.Game.InitializeAsync();
        Mods.Clear();
        foreach (var mod in game.Game.ModDatabase.Mods)
            Mods.Add(mod);
        Logger.Information($"Found {game.Game.ModDatabase.Mods.Count} mods");
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

    public async Task InstallMod(Visual visual, UIGame? game)
    {
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Select Mod...",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new("Archive Files")
                {
                    Patterns = ["*.zip", "*.7z", "*.rar"],
                    MimeTypes =
                    [
                        "application/zip",
                        "application/x-rar-compressed",
                        "application/x-7z-compressed"
                    ]
                }
            ]
        });

        if (files == null)
            return;

        foreach (var file in files)
            InstallMod(file.Name, Uri.UnescapeDataString(file.Path.AbsolutePath), game);
    }


    public void InstallMod(string name, string path, UIGame? game)
    {
        game ??= SelectedGame;

        if (game?.Game.ModDatabase is not ModDatabaseGeneric modsDB)
        {
            Logger.Error("Only ModDatabaseGeneric is supported for installing mods");
            return;
        }

        new Download(name).OnRun(async (d, c) =>
        {
            var installProgress = d.CreateProgress();
            installProgress.ReportMax(1);
            await modsDB.InstallModFromArchive(path, installProgress);

        }).OnComplete((d) =>
        {
            Logger.Information($"Finished installing {name}");
            if (game == SelectedGame)
                Dispatcher.UIThread.Invoke(RefreshUI);
            d.Destroy();
            return Task.CompletedTask;
        }).OnError(async (d, e) =>
        {
            Logger.Error(e);
            Logger.Error($"Failed to install {name}");
            await Task.Delay(5000);
            d.Destroy();
        })
        .Run(this);
    }

    public void UpdateDownload()
    {
        double progress = 0;
        double progressMax = 0;

        for (int i = 0; i < Downloads.Count; i++)
        {
            if (Downloads[i].Destroyed)
            {
                Downloads.RemoveAt(i);
                i--;
                continue;
            }
            progress += Downloads[i].Progress;
            progressMax += Downloads[i].ProgressMax;
        }

        if (Downloads.Count > 0)
        {
            OverallProgress = progress;
            OverallProgressMax = progressMax;
            if (Downloads.Count == 1)
                LastLog = Localize("Download.Text.InstallMod", Downloads[0].Name);
            else
                LastLog = Localize("Download.Text.InstallModMultiple", Downloads.Count);
        }
        ShowProgressBar = Downloads.Count > 0;
    }

    public void AddDownload(Download download)
    {
        download.PropertyChanged += (s, e) => Dispatcher.UIThread.Invoke(UpdateDownload);
        Downloads.Add(download);
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
