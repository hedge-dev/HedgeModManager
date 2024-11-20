using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.ViewModels;
using System.Collections.Generic;
using System.Text;

namespace HedgeModManager.UI.Controls.MainWindow;

public partial class Test : UserControl
{
    public Test()
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
            viewModel.CurrentTabInfo.Buttons.Add(new("Save and Play", Buttons.Y, async (s, e) =>
            {
                await viewModel.SaveAndRun();
            }));
            for (int i = 0; i < 10; i++)
            {
                viewModel.CurrentTabInfo.Buttons.Add(new($"Button {i}", Buttons.B, (s, e) =>
                {
                    if (s is TabInfo.TabButton button)
                        viewModel.CurrentTabInfo.Buttons.Remove(button);
                }));
            }
        }


        void createCheckbox(string name)
        {
            var checkbox = new CheckBox()
            {
                Content = name
            };

            var binding = new Binding
            {
                Source = viewModel.Config,
                Path = name
            };

            checkbox.Bind(CheckBox.IsCheckedProperty, binding);

            ConfigProps.Children.Add(checkbox);
        }

        void createStringEditor(string name)
        {
            var panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            var label = new TextBlock()
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center
            };
            var textbox = new TextBox()
            {
                MaxWidth = 400
            };

            // Add items
            panel.Children.Add(label);
            panel.Children.Add(textbox);

            var binding = new Binding
            {
                Source = viewModel.Config,
                Path = name
            };

            textbox.Bind(TextBox.TextProperty, binding);

            ConfigProps.Children.Add(panel);
        }

        ConfigProps.Children.Clear();
        foreach (var property in viewModel.Config.GetType().GetProperties())
        {
            if (property.CanWrite)
            {
                switch (property.PropertyType.Name)
                {
                    case "Boolean":
                        createCheckbox(property.Name);
                        break;
                    case "String":
                        createStringEditor(property.Name);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private async void SaveConfig_Click(object? sender, RoutedEventArgs e)
    {
        await (DataContext as MainWindowViewModel)!.Config.SaveAsync();
    }

    private async void LoadConfig_Click(object? sender, RoutedEventArgs e)
    {
        await (DataContext as MainWindowViewModel)!.Config.LoadAsync();
    }

    private void ResetConfig_Click(object? sender, RoutedEventArgs e)
    {
        var viewModel = (DataContext as MainWindowViewModel)!;
        viewModel.Config.Reset();
        // Switch to Setup
        viewModel.SelectedTabIndex = 1;
    }

    private async void SaveGame_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel
            && viewModel.SelectedGame != null)
            await viewModel.SelectedGame.Game.ModDatabase.Save();
    }

    private async void LoadGame_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel
            && viewModel.SelectedGame != null)
            await viewModel.SelectedGame.Game.InitializeAsync();
    }

    private async void RunGame_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel
            && viewModel.SelectedGame != null)
            await viewModel.SelectedGame.Game.Run(null, true);
    }

    private void ChangeGame_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.Modals.Add(new Modal(new GameSelectModal()));
    }
    
    private void ClearGame_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.SelectedGame = null;
    }

    private void ClearLog_Click(object? sender, RoutedEventArgs e)
    {
        UILogger.Clear();
    }

    private async void ExportLog_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.ExportLog(this);
    }


}