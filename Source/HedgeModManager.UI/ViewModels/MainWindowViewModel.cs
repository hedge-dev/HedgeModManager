using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
                LastLog = logger.Logs
                .LastOrDefault(x => x.Type != LogType.Debug)!.Message;
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

        public async Task Save()
        {
            try
            {
                await Config.SaveAsync();
                if (SelectedGame != null)
                {
                    if (!SelectedGame.Game.IsModLoaderInstalled())
                        await SelectedGame.Game.InstallModLoaderAsync();
                    try
                    {
                        await SelectedGame.Game.ModDatabase.Save();
                        if (SelectedGame.Game.ModLoaderConfiguration is ModLoaderConfiguration config)
                            await config.Save(Path.Combine(SelectedGame.Game.Root, "cpkredir.ini"));
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        MessageBoxModal.CreateOK("Modal.Title.SaveError", "Modal.Message.GameNoAccess").Open(this);
                        Logger.Error(e);
                        Logger.Error("Failed to save mod database and config");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBoxModal.CreateOK("Modal.Title.SaveError", "Modal.Message.UnknownSaveError").Open(this);
                Logger.Error(e);
                Logger.Error("Failed to save");
                return;
            }
            Logger.Information("Saved");
        }

        public async Task RunGame()
        {
            if (SelectedGame != null)
                await SelectedGame.Game.Run(null, true);
        }

        public async Task SaveAndRun()
        {
            await Save();
            await RunGame();
        }

        public void StartSetup()
        {
            Logger.Debug("Entered setup");
            Config.IsSetupCompleted = false;
            SelectedTabIndex = 1;
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedGame))
                await OnGameChange();
            base.OnPropertyChanged(e);
        }
    }
}
