using Avalonia.Controls;
using HedgeModManager.CodeCompiler;
using HedgeModManager.UI.ViewModels;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.IO;

namespace HedgeModManager.UI.Views
{
    public partial class MainWindow : Window
    {

        public MainWindowViewModel? ViewModel => (MainWindowViewModel?)DataContext;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
                return;

            Logger.Information($"Loading config...");
            await ViewModel.Config.LoadAsync();

            Logger.Information($"Initialising codes...");
            CodeProvider.TryLoadRoslyn();

            Logger.Information($"Locating games...");
            ViewModel.Games = new(Games.GetUIGames(ModdableGameLocator.LocateGames()));
            //ViewModel.Games = new();
            if (ViewModel.Games.Count == 0)
            {
                Logger.Information($"No games found!");
                ViewModel.StartSetup();
            }
            else
            {
                // Select the last selected game or first game
                ViewModel.SelectedGame = ViewModel.Games
                    .FirstOrDefault(x => x != null && Path.Combine(x.Game.Root, x.Game.Executable ?? "") == ViewModel.Config.LastSelectedPath, ViewModel.Games.FirstOrDefault());
            }


            if (ViewModel.Config.IsSetupCompleted)
                ViewModel.SelectedTabIndex = 2; // Mods
            else
                ViewModel.SelectedTabIndex = 1; // Setup
            ViewModel.IsBusy = false;
        }

        private void OnTabChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
                return;

            ViewModel.CurrentTabInfo = ViewModel.TabInfos[ViewModel.SelectedTabIndex];
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (ViewModel == null)
                return;

            switch (e.Key)
            {
                case Key.F3:
                    ViewModel.Config.TestModeEnabled = !ViewModel.Config.TestModeEnabled;
                    Logger.Debug($"Set test mode to {ViewModel.Config.TestModeEnabled}");
                    break;
                default:
                    break;
            }
        }

    }
}