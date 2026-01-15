using Avalonia.Interactivity;
using Avalonia.Threading;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls.Modals;

public partial class GameMissingInfoModal : WindowModal
{
    private DispatcherTimer? _timer;

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;

        _timer?.Stop();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (_, _) =>
        {
            // Check if the modal is closed
            if (!viewModel.Modals.Any(x => x.Control == this))
            {
                _timer.Stop();
                return;
            }

            var uiGames = Games.GetUIGames(ModdableGameLocator.LocateGames());
            var missingGames = uiGames.Where(x => !viewModel.Games.Any(y => y.Game.ID == x.Game.ID));
            if (missingGames.Any())
            {
                _timer.Stop();
                missingGames.ForEach(x => viewModel.Games.Add(x));
                viewModel.SelectedGame = viewModel.Games.LastOrDefault();
                Close();

                // Close game selector
                var gameSelector = viewModel.Modals.FirstOrDefault(x => x.Control is GameSelectModal);
                if (gameSelector != null)
                    viewModel.Modals.Remove(gameSelector);
            }
        };
        _timer.Start();
    }

    private void OnStopClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}