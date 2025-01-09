using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls.Modals;

public partial class GameSelectModal : UserControl
{
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
        viewModel.Modals.RemoveAt(viewModel.Modals.Count - 1);
    }
}