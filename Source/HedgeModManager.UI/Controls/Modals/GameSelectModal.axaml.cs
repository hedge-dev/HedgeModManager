using Avalonia.Controls;
using Avalonia.Interactivity;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls.Modals;

public partial class GameSelectModal : UserControl
{
    public GameSelectModal()
    {
        InitializeComponent();
    }

    private void OnGameClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        var game = (sender as Control)?.DataContext as UIGame;
        if (game == null || viewModel == null)
            return;
        viewModel.SelectedGame = game;
        viewModel.Modals.RemoveAt(viewModel.Modals.Count - 1);
    }
}