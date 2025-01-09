using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.About;
using System.Diagnostics;

namespace HedgeModManager.UI.Controls.About;

public partial class About : UserControl
{
    public AboutViewModel ViewModel { get; set; } = new();

    public About()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        await ViewModel.Update();
        var viewModel = (DataContext as MainWindowViewModel);
        if (viewModel == null)
            return;

        // Add buttons
        if (viewModel.CurrentTabInfo != null)
        {
            viewModel.CurrentTabInfo.Buttons.Clear();
            viewModel.CurrentTabInfo.Buttons.Add(new("About.Button.GitHub", Buttons.Y, (b) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"https://github.com/{Program.GitHubRepoOwner}/{Program.GitHubRepoName}",
                    UseShellExecute = true
                });
            }));
        }
    }
}