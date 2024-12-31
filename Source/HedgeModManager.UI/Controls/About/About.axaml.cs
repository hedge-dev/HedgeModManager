using Avalonia.Controls;
using Avalonia.Interactivity;
using HedgeModManager.UI.ViewModels;
using System.Diagnostics;

namespace HedgeModManager.UI.Controls.About;

public partial class About : UserControl
{
    public About()
    {
        InitializeComponent();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
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