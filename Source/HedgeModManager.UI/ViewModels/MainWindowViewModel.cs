﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using HedgeModManager.IO;
using HedgeModManager.UI.CLI;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Languages;
using HedgeModManager.UI.Models;
using SharpCompress;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using static HedgeModManager.UI.Languages.Language;
using static HedgeModManager.UI.Models.Download;

namespace HedgeModManager.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string AppVersion => Program.ApplicationVersion;

    public ObservableCollection<UIGame> Games { get; set; } = [];
    public ObservableCollection<Download> Downloads { get; set; } = [];
    public ObservableCollection<LanguageEntry> Languages { get; set; } = [];
    public ProgramConfig Config { get; set; } = new();
    public int ServerStatus { get; set; } = 1;
    public CancellationTokenSource ServerCancellationTokenSource { get; set; } = new();
    public bool IsFullscreen => WindowState == WindowState.FullScreen;

    [ObservableProperty] private UIGame? _selectedGame;
    [ObservableProperty] private ModProfile _selectedProfile = ModProfile.Default;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private UILogger? _loggerInstance;
    [ObservableProperty] private string _lastLog = "";
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private TabInfo? _currentTabInfo;
    [ObservableProperty] private TabInfo[] _tabInfos = 
        [new ("Loading"), new("Setup"), new("Mods"), new("Codes"), new("Settings"), new("About"), new("Test")];
    [ObservableProperty] private ObservableCollection<Modal> _modals = [];
    [ObservableProperty] private ObservableCollection<IMod> _mods = [];
    [ObservableProperty] private ObservableCollection<ICode> _codes = [];
    [ObservableProperty] private ObservableCollection<ModProfile> _profiles = [];
    [ObservableProperty] private bool _isBusy = true;
    [ObservableProperty] private double _overallProgress = 0d;
    [ObservableProperty] private double _overallProgressMax = 0d;
    [ObservableProperty] private bool _showProgressBar = false;
    [ObservableProperty] private bool _progressIndeterminate = false;
    [ObservableProperty] private LanguageEntry? _selectedLanguage;
    [ObservableProperty] private WindowState _windowState = WindowState.Normal;

    // Preview only
    public MainWindowViewModel() { }

    public MainWindowViewModel(UILogger logger, List<LanguageEntry> languages)
    {
        // Setup languages
        LanguageEntry.TotalLineCount = languages.Max(x => x.Lines);
        Languages = new ObservableCollection<LanguageEntry>(languages);

        // Setup logger
        _loggerInstance = logger;
        _ = new Logger(logger);
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
        Logger.Debug($"RID: {RuntimeInformation.RuntimeIdentifier}");
        Logger.Debug($"FlatpakID: \"{Program.FlatpakID}\" ({!string.IsNullOrEmpty(Program.FlatpakID)})");
        Logger.Debug($"InstallLocation: {Program.InstallLocation}");
        Logger.Debug($"IsDebugBuild: {Program.IsDebugBuild}");
    }

    public async Task OnStartUp()
    {
        if (Config.LastUpdateCheck.AddMinutes(20) < DateTime.Now &&
            !Design.IsDesignMode && !Program.IsDebugBuild)
        {
            await CheckForManagerUpdates();
            await CheckForModLoaderUpdates();
            try
            {
                Config.LastUpdateCheck = DateTime.Now;
                await Config.SaveAsync();
            }
            catch { }
        }
    }

    public async Task CheckForManagerUpdates()
    {
        await new Download(Localize("Download.Text.CheckManagerUpdate"), true)
        .OnRun(async (d, c) =>
        {
            d.CreateProgress().ReportMax(-1);
            
            var (update, status) = await Updater.CheckForUpdates();
            if (update == null)
            {
                if (status == Updater.UpdateCheckStatus.NoUpdate)
                {
                    Logger.Information("No release found");
                } else
                {
                    OpenErrorMessage("Modal.Title.UpdateError", "Modal.Message.UpdateCheckError",
                        "Failed to check for updates", null);
                }
                d.Destroy();
                return;
            }
            d.Destroy();

            string message = $"Update found:\n{update.Title} - {update.Version}";
            Logger.Information(message);
            var messageBox = new MessageBoxModal("Modal.Title.UpdateManager", message);
            messageBox.AddButton("Common.Button.Cancel", (s, e) => messageBox.Close());
            messageBox.AddButton("Common.Button.Install", async (s, e) =>
            {
                Logger.Information("Install clicked for update");
                messageBox.Close();
                if (Program.FlatpakID != null)
                {
                    MessageBoxModal.CreateOK("Modal.Title.UpdateManager", "Modal.Message.PkgUpdate");
                    return;
                }
                await Updater.BeginUpdate(update, this);
            });
            //messageBox.AddButton("Test", async (s, e) =>
            //{
            //    messageBox.Close();
            //    await Updater.UpdateFromPackage("../update.zip", this);
            //});
            messageBox.Open(this);
        }).OnError((d, e) =>
        {
            Logger.Error(e);
            Logger.Error($"Unexpected error while checking for updates");
            d.Destroy();
            return Task.CompletedTask;
        }).RunAsync(this);
    }

    public Task CheckForModLoaderUpdates()
    {
        return Task.CompletedTask;
    }

    public async Task CheckForAllModUpdates()
    {
        if (SelectedGame == null)
            return;
        await new Download(Localize("Download.Text.CheckModUpdate", 0, 0), true)
        .OnRun(async (d, c) =>
        {
            var updatableMods = SelectedGame.Game.ModDatabase.Mods
                .Where(x => x.Updater != null)
                .ToList();

            var progress = d.CreateProgress();
            progress.ReportMax(updatableMods.Count);

            Logger.Debug($"Checking {updatableMods.Count} mod updates");
            foreach (var mod in updatableMods)
            {
                if (c.IsCancellationRequested)
                    break;
                await CheckForModUpdates(mod, false, c);
                progress.ReportAdd(1);
                d.Name = Localize("CheckModUpdate", progress.Progress, progress.ProgressMax);
            }
        }).OnError((d, e) =>
        {
            Logger.Error(e);
            Logger.Error($"Unexpected error while checking for mod updates");
            d.Destroy();
            return Task.CompletedTask;
        }).RunAsync(this);
    }

    public async Task<UpdateInfo?> CheckForModUpdates(IMod mod, bool promptUpdate = false, CancellationToken c = default)
    {
        try
        {
            if (await mod.Updater!.CheckForUpdatesAsync(c))
            {
                var info = await mod.Updater.GetUpdateInfoAsync(c);
                Logger.Debug($"Update found for {mod.Title}");
                Logger.Debug($"  Current: {mod.Version}");
                Logger.Debug($"  Latest: {info.Version}");
                if (promptUpdate)
                {
                    var messageBox = new MessageBoxModal("Modal.Title.UpdateMod", Localize("Modal.Message.UpdateMod", mod.Title));
                    messageBox.AddButton("Common.Button.Cancel", (s, e) => messageBox.Close());
                    messageBox.AddButton("Common.Button.Update", async (s, e) =>
                    {
                        // TODO: Look into threading issues
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            Modals.Where(x => x.Control is ModInfoModal).ToList().ForEach(x => x.Close());
                            Logger.Information($"Update clicked for {mod.Title}");
                            messageBox.Close();
                            await mod.Updater.PerformUpdateAsync(c);
                            Modals.Where(x => x.Control is ModInfoModal).ToList().ForEach(x => x.Close());
                            RefreshGame();
                        });
                    });
                    messageBox.Open(this);
                }
                return info;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to check for updates for {mod.Title}");
            Logger.Error(e);
        }
        return null;
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

            Logger.Debug($"Loading profiles...");
            if (game.ModDatabase is ModDatabaseGeneric modsDB)
                Profiles = new(await LoadProfiles(game) ?? []);
            SelectedProfile = Profiles.FirstOrDefault() ?? ModProfile.Default;
            Logger.Debug($"Loaded {Profiles.Count} profile(s)");

            Config.LastSelectedPath = Path.Combine(game.Root, game.Executable ?? "");

            _ = UpdateCodes(false, true);
            RefreshUI();
        }
        catch (Exception e)
        {
            OpenErrorMessage("Modal.Title.LoadError", "Modal.Message.GameLoadError",
                "Failed to load game/mod data", e);
        }
    }

    public async Task InstallModLoader(bool? install = null)
    {
        IsBusy = true;
        await CreateSimpleDownload("Download.Text.InstallModLoader", "Failed to install modloader",
            async (d, p, c) =>
            {
                if (SelectedGame == null)
                    return;
                if (install != null)
                {
                    var gameGeneric = GetModdableGameGeneric();
                    if (gameGeneric == null || gameGeneric.ModLoader == null)
                        return;
                    
                    if (install == true)
                        _ = await gameGeneric.ModLoader.InstallAsync();
                    else
                        _ = await gameGeneric.ModLoader.UninstallAsync();
                }
                else
                {
                    _ = await SelectedGame.Game.InstallModLoaderAsync();
                }
                IsBusy = false;
                Dispatcher.UIThread.Invoke(RefreshUI);
            }).RunAsync(this);
    }

    // TODO: Implement mod config switching
    public Task LoadProfile()
    {
        Logger.Debug($"Loaded profile {SelectedProfile?.Name}");
        return Task.CompletedTask;
    }

    public async Task<List<ModProfile>?> LoadProfiles(IModdableGame game)
    {
        string filePath = Path.Combine(game.Root, "profiles.json");
        // Profiles are only supported for ModDatabaseGeneric
        if (SelectedGame?.Game.ModDatabase is not ModDatabaseGeneric modsDB)
            return null;
        if (!File.Exists(filePath))
            return [ModProfile.Default];

        string json = await File.ReadAllTextAsync(filePath);
        var profiles = JsonSerializer.Deserialize<List<ModProfile>>(json);

        // Remove missing profiles
        if (profiles != null)
            profiles = profiles.Where(x => File.Exists(Path.Combine(modsDB.Root, x.ModDBPath))).ToList();

        if (profiles?.Count == 0)
            return [ModProfile.Default];
        return profiles ?? [ModProfile.Default];
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
                    // TODO: Resolve mod dependencies
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
            OpenErrorMessage("Modal.Title.SaveError", "Modal.Message.UnknownSaveError", "Failed to save", e);
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

    public async Task UpdateCodes(bool force, bool append)
    {
        if (SelectedGame != null && SelectedGame.Game is ModdableGameGeneric gameGeneric &&
            gameGeneric.ModLoaderConfiguration is ModLoaderConfiguration config)
        {
            string modsRoot = PathEx.GetDirectoryName(config.DatabasePath).ToString();
            string mainCodesPath = Path.Combine(modsRoot, ModDatabaseGeneric.MainCodesFileName);
            if (force || !File.Exists(mainCodesPath))
            {
                await CreateSimpleDownload("Download.Text.DownloadCodes", "Failed to download community codes",
                    async (d, p, c) =>
                    {
                        await gameGeneric.DownloadCodes(null);
                        if (append)
                            gameGeneric.ModDatabase.LoadSingleCodeFile(mainCodesPath);
                        else
                            await gameGeneric.InitializeAsync();
                        Dispatcher.UIThread.Invoke(RefreshUI);
                    }).RunAsync(this);
            }
        }
    }

    public void RefreshGame()
    {
        if (SelectedGame is not UIGame game)
            return;
        OnPropertyChanged(nameof(SelectedGame));
    }

    public void RefreshUI()
    {
        if (SelectedGame is not UIGame game)
            return;

        UpdateModsList();
        UpdateCodesList();

        Logger.Information($"Found {game.Game.ModDatabase.Mods.Count} mods");
    }

    public void UpdateModsList()
    {
        if (SelectedGame == null)
        {
            Logger.Error("Updating mods from null game!");
            Codes.Clear();
            return;
        }

        Mods = new(SelectedGame.Game.ModDatabase.Mods);
    }

    public void UpdateCodesList()
    {
        if (SelectedGame == null)
        {
            Logger.Error("Updating codes from null game!");
            Codes.Clear();
            return;
        }

        Codes = new(SelectedGame.Game.ModDatabase.Codes);
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

    public ModdableGameGeneric? GetModdableGameGeneric()
    {
        return SelectedGame?.Game as ModdableGameGeneric;
    }

    public int GetTabIndex(string name)
    {
        return Array.FindIndex(TabInfos, x => x.Name == name);
    }

    public static async Task ExportLog(Visual visual)
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
            InstallMod(file.Name, Utils.ConvertToPath(file.Path), game);
    }

    public void InstallMod(string name, string path, UIGame? game)
    {
        game ??= SelectedGame;

        if (game?.Game.ModDatabase is not ModDatabaseGeneric modsDB)
        {
            OpenErrorMessage("Modal.Title.InstallError", "Modal.Message.InstallError",
                "Only ModDatabaseGeneric is supported for installing mods");
            return;
        }

        new Download(name).OnRun(async (d, c) =>
        {
            var installProgress = d.CreateProgress();
            installProgress.ReportMax(1);
            await modsDB.InstallModFromArchive(path, installProgress);
            Logger.Information($"Finished installing {name}");
            if (game == SelectedGame)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    await SelectedGame.Game.InitializeAsync();
                    RefreshUI();
                });
            }
        }).OnError((d, e) =>
        {
            OpenErrorMessage("Modal.Title.InstallError", "Modal.Message.InstallError",
                $"Failed to install {name}");
            return Task.CompletedTask;
        }).OnFinally((d) =>
        {
            d.Destroy();
            return Task.CompletedTask;
        })
        .Run(this);
    }

    public void UpdateDownload()
    {
        double progress = 0;
        double progressMax = -1;

        for (int i = 0; i < Downloads.Count; i++)
        {
            if (Downloads[i].Destroyed)
            {
                Downloads.RemoveAt(i);
                i--;
                continue;
            }
            if (Downloads[i].ProgressMax != -1)
            {
                progress += Downloads[i].Progress;
                progressMax += Downloads[i].ProgressMax;
            }
        }

        if (Downloads.Count > 0)
        {
            OverallProgress = progress;
            OverallProgressMax = progressMax;
            if (Downloads.Count == 1)
                Message = Downloads[0].CustomTitle ?
                    Downloads[0].Name : Localize("Download.Text.InstallMod", Downloads[0].Name);
            else
                Message = Localize("Download.Text.InstallModMultiple", Downloads.Count);
        }
        ShowProgressBar = Downloads.Count > 0;
        ProgressIndeterminate = progressMax == -1;
    }

    public void AddDownload(Download download)
    {
        download.PropertyChanged += (s, e) => Dispatcher.UIThread.Invoke(UpdateDownload);
        Downloads.Add(download);
    }

    public static Download CreateSimpleDownload(string name, string errorMessage, Func<Download, DownloadProgress, CancellationToken, Task> callback)
    {
        return new Download(Localize(name), true)
            .OnRun(async (d, c) =>
            {
                var progress = d.CreateProgress();
                progress.ReportMax(-1);
                await callback(d, progress, c);
                d.Destroy();
            }).OnError((d, e) =>
            {
                Logger.Error(e);
                Logger.Error(errorMessage);
                d.Destroy();
                return Task.CompletedTask;
            });
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
            var c = ServerCancellationTokenSource.Token;
            while (ServerStatus == 1)
            {
                using var server = new NamedPipeServerStream(Program.PipeName, PipeDirection.In, 
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(c);
                Logger.Debug("Recieved connection");
                using var reader = new StreamReader(server);
                string message = await reader.ReadToEndAsync(c);
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
        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Logger.Error("Server stopped due to error");
            Logger.Error(e);
        }
        ServerStatus = 0;
    }

    public void StopServer()
    {
        ServerStatus = 2;
        ServerCancellationTokenSource.Cancel();
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedGame))
            await LoadGame();
        if (e.PropertyName == nameof(SelectedProfile))
            await LoadProfile();
        if (e.PropertyName == nameof(WindowState))
            OnPropertyChanged(nameof(IsFullscreen));
        base.OnPropertyChanged(e);
    }
}
