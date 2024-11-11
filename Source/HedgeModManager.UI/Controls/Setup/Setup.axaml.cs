using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace HedgeModManager.UI.Controls.Setup;

public partial class Setup : UserControl
{

    public TabInfo.TabButton? BackButton { get; set; }
    public TabInfo.TabButton? NextButton { get; set; }

    public Setup()
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
            viewModel.CurrentTabInfo.Buttons.Add(BackButton = new("Back", Buttons.B, (s, e) =>
            {
                MainTabControl.SelectedIndex--;
            }, MainTabControl.SelectedIndex != 0));
            viewModel.CurrentTabInfo.Buttons.Add(NextButton = new("Next", Buttons.A, (s, e) =>
            {
                if (MainTabControl.SelectedIndex == MainTabControl.ItemCount - 1)
                {
                    viewModel.SelectedGame = viewModel.Games.FirstOrDefault();
                    if (viewModel.SelectedGame != null)
                    {
                        viewModel.Config.IsSetupCompleted = true;
                        viewModel.SelectedTabIndex = 2;
                    }
                }
                MainTabControl.SelectedIndex++;
            }));
        }
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
            NextButton.Name = "Finish";
        else
            NextButton.Name = "Next";

    }
}