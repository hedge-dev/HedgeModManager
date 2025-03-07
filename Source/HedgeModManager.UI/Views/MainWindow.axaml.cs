using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using HedgeModManager.CodeCompiler;
using HedgeModManager.UI.ViewModels;
using Avalonia.Markup.Xaml;
using HedgeModManager.UI.Input;
using HedgeModManager.Foundation;
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

    public void LoadGames()
    {
        if (ViewModel == null)
            return;

        Logger.Information($"Locating games...");
        ViewModel.Games = new(Games.GetUIGames(ModdableGameLocator.LocateGames()));
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

        // Set to true until we create a setup
        if (ViewModel.SelectedGame != null)
            ViewModel.Config.IsSetupCompleted = true;

        if (ViewModel.Config.IsSetupCompleted)
            ViewModel.SelectedTabIndex = 2; // Mods
        else
            ViewModel.SelectedTabIndex = 1; // Setup
        ViewModel.IsBusy = false;

        // Enable fullscreen if running in gamescope
        if (ViewModel.IsGamescope)
            ViewModel.WindowState = WindowState.FullScreen;
        else
            ViewModel.WindowState = ViewModel.Config.LastWindowState;
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
            return;

        Logger.Information($"Initialising codes...");
        CodeProvider.TryLoadRoslyn();

        Logger.Information($"Loading URI handlers...");
        Program.InstallURIHandler();

        LoadGames();

        _ = Dispatcher.UIThread.Invoke(async () =>
        {
            if (Program.StartupCommands.Count > 0)
                await ViewModel.ProcessCommandsAsync(Program.StartupCommands);
            await ViewModel.OnStartUpAsync();
            await ViewModel.StartServerAsync();
        });

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
        bool toggleFullscreen = e.KeyModifiers == KeyModifiers.Alt && e.Key == Key.Enter;
        bool isShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        bool isAlt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        if (ViewModel == null)
            return;

        switch (e.Key)
        {
            case Key.F1:
                if (isShift)
                    _ = Dispatcher.UIThread.Invoke(() => MainWindowViewModel.ExportLogAsync(this));
                break;
            case Key.F3:
                ViewModel.Config.TestModeEnabled = !ViewModel.Config.TestModeEnabled;
                Logger.Debug($"Set test mode to {ViewModel.Config.TestModeEnabled}");
                break;
            case Key.F5:

                // Reload if holding shift
                if (isShift)
                {
                    if (ViewModel.IsBusy)
                        break;

                    ViewModel.SelectedTabIndex = 0; // Loading

                    ViewModel.Modals.Clear();
                    ViewModel.IsBusy = true;

                    Logger.Information($"Loading config...");
                    _ = Dispatcher.UIThread.Invoke(ViewModel.Config.LoadAsync);

                    Logger.Information($"Loading URI handlers...");
                    Program.InstallURIHandler();

                    LoadGames();
                    ViewModel.IsBusy = false;

                    Logger.Debug($"Reloaded");
                }
                else
                {
                    if (ViewModel.SelectedGame != null)
                    {
                        _ = Dispatcher.UIThread.Invoke(async () =>
                        {
                            await ViewModel.SelectedGame.Game.InitializeAsync();
                            ViewModel.RefreshUI();
                            Logger.Debug($"Refreshed game");
                        });
                    }
                }
                break;
            case Key.F11:
                toggleFullscreen = true;
                break;
            case Key.Escape:
                if (ViewModel.Modals.Count > 0)
                    ViewModel.Modals.RemoveAt(ViewModel.Modals.Count - 1);
                break;
            default:
                if (ViewModel.Config.TestKeyboardInput)
                {
                    var button = Buttons.None;

                    switch (e.Key)
                    {
                        case Key.Left:
                            if (isShift)
                                button = Buttons.X;
                            else if (isAlt)
                                button = Buttons.LB;
                            else
                                button = Buttons.Left;
                            break;
                        case Key.Right:
                            if (isShift)
                                button = Buttons.B;
                            else if (isAlt)
                                button = Buttons.RB;
                            else
                                button = Buttons.Right;
                            break;
                        case Key.Up:
                            if (isShift)
                                button = Buttons.Y;
                            else
                                button = Buttons.Up;
                            break;
                        case Key.Down:
                            if (isShift)
                                button = Buttons.A;
                            else
                                button = Buttons.Down;
                            break;
                        case Key.Enter:
                            if (isShift)
                                button = Buttons.Start;
                            else
                                button = Buttons.A;
                            break;
                        case Key.Back:
                            button = Buttons.Back;
                            break;
                        default:
                            break;
                    }

                    if (button != Buttons.None)
                        _ = Dispatcher.UIThread.Invoke(async () => await ViewModel.OnInputDownAsync(button));
                }
                break;
        }

        if (toggleFullscreen)
        {
            if (ViewModel.WindowState == WindowState.FullScreen)
                ViewModel.WindowState = WindowState.Normal;
            else
                ViewModel.WindowState = WindowState.FullScreen;
            ViewModel.Config.LastWindowState = ViewModel.WindowState;
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