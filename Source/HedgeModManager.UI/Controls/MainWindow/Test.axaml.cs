using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using System.Diagnostics;

namespace HedgeModManager.UI.Controls.MainWindow;

public partial class Test : UserControl
{
    public Test()
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
            viewModel.CurrentTabInfo.Buttons.Add(new("Save and Play", Buttons.Y, async (b) =>
            {
                await viewModel.SaveAndRun();
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Change Theme", Buttons.Y, (b) =>
            {
                if (Application.Current != null)
                {
                    if (Application.Current.RequestedThemeVariant == ThemeVariant.Dark)
                        Application.Current.RequestedThemeVariant = Themes.Themes.Darker;
                    else
                        Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                }
            }));
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

    private void LoadGame_Click(object? sender, RoutedEventArgs e)
    {
        // Trigger game load
        if (DataContext is MainWindowViewModel viewModel)
        {
            var game = viewModel.SelectedGame;
            viewModel.SelectedGame = null;
            viewModel.SelectedGame = game;
        }
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

    private void OpenGame_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel ||
            viewModel.SelectedGame is not UIGame game)
            return;
        Process.Start(new ProcessStartInfo
        {
            FileName = game.Game.Root,
            UseShellExecute = true
        });
    }

    private void ClearLog_Click(object? sender, RoutedEventArgs e)
    {
        UILogger.Clear();
    }

    private async void ExportLog_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await MainWindowViewModel.ExportLog(this);
    }

    private void CreateDownload_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var download = new Download("Download", false, 1000);

            download.OnRun(async (d, c) =>
            {
                for (int i = 0; i <= 1000; i++)
                {
                    if (c.IsCancellationRequested)
                        break;
                    d.Progress = i;
                    await Task.Delay(10, c);
                }
            }).OnError((d, e) =>
            {
                Logger.Debug("Download failed");
                return Task.CompletedTask;
            }).OnCancel((d) =>
            {
                Logger.Debug("Download cancelled");
                return Task.CompletedTask;
            }).OnFinally((d) =>
            {
                Logger.Debug("Download finalised");
                return Task.CompletedTask;
            }).Run(viewModel);
        }
    }

    private async void InstallMod_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.InstallMod(this, null);
    }

    private void DownloadCancel_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is Download download)
            download.Cancel();
    }

    private void DownloadDelete_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control && control.DataContext is Download download)
            download.Destroy();
    }
}