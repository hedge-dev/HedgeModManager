using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using HedgeModManager.CodeCompiler;
using HedgeModManager.UI.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace HedgeModManager.UI.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel? ViewModel => (MainWindowViewModel?)DataContext;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private async void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
            return;

        Logger.Information($"Initialising codes...");
        CodeProvider.TryLoadRoslyn();

        Logger.Information($"Loading URI handlers...");
        Program.InstallURIHandler();

        Logger.Information($"Locating games...");
        ViewModel.Games = new(Games.GetUIGames(ModdableGameLocator.LocateGames()));
        //ViewModel.Games = new();
        if (ViewModel.Games.Count == 0)
        {
            Logger.Information($"No games found!");
            ViewModel.StartSetup();
        }
        else
        {
            // Select the last selected game or first game
            ViewModel.SelectedGame = ViewModel.Games
                .FirstOrDefault(x => x != null && Path.Combine(x.Game.Root, x.Game.Executable ?? "")
                == ViewModel.Config.LastSelectedPath, ViewModel.Games.FirstOrDefault());
        }

        if (ViewModel.Config.IsSetupCompleted)
            ViewModel.SelectedTabIndex = 2; // Mods
        else
            ViewModel.SelectedTabIndex = 1; // Setup
        ViewModel.IsBusy = false;

        if (Program.StartupCommands.Count > 0)
            await ViewModel.ProcessCommands(Program.StartupCommands);

        _ = Dispatcher.UIThread.InvokeAsync(ViewModel.StartServer);
        await ViewModel.OnStartUp();

        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    private void OnTabChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ViewModel == null)
            return;

        ViewModel.CurrentTabInfo = ViewModel.TabInfos[ViewModel.SelectedTabIndex];
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel == null)
            return;

        switch (e.Key)
        {
            case Key.F3:
                ViewModel.Config.TestModeEnabled = !ViewModel.Config.TestModeEnabled;
                Logger.Debug($"Set test mode to {ViewModel.Config.TestModeEnabled}");
                break;
            case Key.F5:
                ViewModel.RefreshUI();
                Logger.Debug($"Refreshed UI");
                break;
            case Key.F6:
                ViewModel.RefreshGame();
                Logger.Debug($"Refreshed game");
                break;
            default:
                break;
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (ViewModel == null || !e.Data.Contains(DataFormats.Files))
            return;
        e.DragEffects = DragDropEffects.Copy;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (ViewModel == null || !e.Data.Contains(DataFormats.Files))
            return;
        foreach (var file in e.Data.GetFiles() ?? [])
            ViewModel.InstallMod(file.Name, Utils.ConvertToPath(file.Path), null);
    }
}