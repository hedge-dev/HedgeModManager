using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls.Setup;

public partial class Setup : UserControl
{
    public TabInfo.TabButton? BackButton { get; set; }
    public TabInfo.TabButton? NextButton { get; set; }

    public Setup()
    {
        AvaloniaXamlLoader.Load(this);
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
            // TODO: Make a proper welcome screen
            viewModel.CurrentTabInfo.Buttons.Add(BackButton = new("Save Log File", ButtonsOLD.Y, async (b) =>
            {
                await MainWindowViewModel.ExportLogAsync(this);
            }));
            viewModel.CurrentTabInfo.Buttons.Add(BackButton = new("Common.Button.Back", ButtonsOLD.B, (b) =>
            {
                MainTabControl.SelectedIndex--;
            }, MainTabControl.SelectedIndex != 0));
            string nextButtonName = MainTabControl.SelectedIndex == MainTabControl.ItemCount - 1 ?
                "Common.Button.Finish" : "Common.Button.Next";
            viewModel.CurrentTabInfo.Buttons.Add(NextButton = new(nextButtonName, ButtonsOLD.A, async (b) =>
            {
                if (MainTabControl.SelectedIndex == MainTabControl.ItemCount - 1)
                {
                    viewModel.SelectedGame = viewModel.Games.FirstOrDefault();
                    if (viewModel.SelectedGame != null)
                    {
                        viewModel.Config.IsSetupCompleted = true;
                        viewModel.SelectedTabIndex = 2;
                    }
                    await viewModel.SaveAsync(false);
                }
                MainTabControl.SelectedIndex++;
            }));
        }
    }

    private async void ExportLog_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await MainWindowViewModel.ExportLogAsync(this);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (BackButton == null || NextButton == null)
            return;

        int tabCount = MainTabControl.ItemCount;
        int tabIndex = MainTabControl.SelectedIndex;
        if (tabIndex == 0)
            BackButton.IsEnabled = false;
        else
            BackButton.IsEnabled = true;

        if (tabIndex == tabCount - 1)
            NextButton.Name = "Common.Button.Finish";
        else
            NextButton.Name = "Common.Button.Next";
    }
}