using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using HedgeModManager.Text;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls;
using HedgeModManager.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        [ObservableProperty] private Logger? _loggerInstance;
        [ObservableProperty] private string _lastLog = "";
        [ObservableProperty] private TabInfo? _currentTabInfo;
        [ObservableProperty] private TabInfo[] _tabInfos = 
            [new ("Loading"), new("Setup"), new("Mods"), new("Codes"), new("Settings"), new("About"), new("Test")];
        [ObservableProperty] private ObservableCollection<Modal> _modals = new ();

        public MainWindowViewModel(Logger logger)
        {
            _loggerInstance = logger;
            logger.Logs.CollectionChanged += (sender, args) =>
            {
                if (logger.Logs.Count == 0)
                    return;
                LastLog = logger.Logs
                .LastOrDefault(x => x.Type != Logger.LogType.Debug)!.Message;
            };
            Logger.Information($"Starting HedgeModManager {AppVersion}...");
            Logger.Debug($"IsWindows: {OperatingSystem.IsWindows()}");
            Logger.Debug($"IsLinux: {OperatingSystem.IsLinux()}");
        }

        public void OnGameChange()
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
            game.InitializeAsync().Wait();
            Logger.Debug($"Initialised game");
            Logger.Information($"Found {game.ModDatabase.Mods.Count} mods");
        }

        public async Task Save()
        {
            await Config.SaveAsync();
            if (SelectedGame != null)
            {
                if (!SelectedGame.Game.IsModLoaderInstalled())
                    await SelectedGame.Game.InstallModLoaderAsync();
                await SelectedGame.Game.ModDatabase.Save();
            }
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

        public void OnLoad()
        {

        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedGame))
                OnGameChange();
            base.OnPropertyChanged(e);
        }
    }
}
