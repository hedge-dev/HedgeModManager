using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls.Modals;

public partial class GameSelectModal : UserControl
{
    // For displaying if the game is missing while on Flatpak
    public bool IsGameMissing
    {
        get
        {
            if (!Helpers.IsFlatpak)
                return false;

            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null)
                return true;

            return !viewModel.Games.Any(x => x.Game.Name == "UnleashedRecompiled");
        }
    }

    public GameSelectModal()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnGameClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is not UIGame game ||
            DataContext is not MainWindowViewModel viewModel)
            return;
        viewModel.SelectedGame = game;
        if (viewModel.Modals.Count > 0)
            viewModel.Modals.RemoveAt(viewModel.Modals.Count - 1);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Preview
        if (DataContext == null && Design.IsDesignMode)
        {
            var viewModel = new MainWindowViewModel();
            var dummyGames = ModdableGameLocator.ModdableGameList
                .Select(gameInfo => new GameSimple("", gameInfo.ID, gameInfo.ID, "", null, "", null, null, null))
                .Select(x => new ModdableGameGeneric(x))
                .Cast<IModdableGame>();
            viewModel.Games = new(Games.GetUIGames(dummyGames));

            DataContext = viewModel;
        }
    }

    // TODO: Handle pointer down 
    private void OnGameMissingPointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        Utils.OpenURL("https://github.com/hedge-dev/HedgeModManager/issues/44");
    }
}