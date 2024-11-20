using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeModManager.UI.ViewModels
{
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
        public MainWindowViewModel()
        {

        }

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

        public async Task OnGameChange()
        {
            if (SelectedGame == null || SelectedGame.Game == null)
            {
                Logger.Error($"Selected game is null! Returning to setup");
                // Return to setup if game is missing
                StartSetup();
                return;
            }

            var game = SelectedGame.Game;

            Logger.Debug($"Selected game changed:");
            Logger.Debug($"    {game.Name} - {game.Platform}");
            Logger.Debug($"    {game.Root}");
            Logger.Debug($"    {game.Executable}");
            await game.InitializeAsync();
            Logger.Debug($"Initialised game");

            Config.LastSelectedPath = Path.Combine(game.Root, game.Executable ?? "");

            Logger.Information($"Found {game.ModDatabase.Mods.Count} mods");
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
                FileTypeChoices = new List<FilePickerFileType>()
                {
                    new("Log Files")
                    {
                        Patterns = new List<string>() { "*.log" },
                        MimeTypes = new List<string>() { "text/plain" }
                    }
                }
            });

            if (file != null)
            {
                using var stream = await file.OpenWriteAsync();
                await stream.WriteAsync(Encoding.Default.GetBytes(log));
            }
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedGame))
                await OnGameChange();
            base.OnPropertyChanged(e);
        }
    }
}
