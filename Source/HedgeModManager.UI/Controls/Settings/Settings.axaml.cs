using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using HedgeModManager.CoreLib;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Languages;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Settings;
using System.Diagnostics;

using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Settings;

public partial class Settings : UserControl
{
    public static readonly StyledProperty<UIGame?> GameProperty =
        AvaloniaProperty.Register<Settings, UIGame?>(nameof(Game));

    public static readonly StyledProperty<ModProfile?> ProfileProperty =
        AvaloniaProperty.Register<Settings, ModProfile?>(nameof(Profile));

    public UIGame? Game
    {
        get => GetValue(GameProperty);
        set => SetValue(GameProperty, value);
    }

    public ModProfile? Profile
    {
        get => GetValue(ProfileProperty);
        set => SetValue(ProfileProperty, value);
    }

    public SettingsViewModel ViewModel { get; set; } = new();

    public bool _isInstalling = false;

    public Settings()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var viewModel = (DataContext as MainWindowViewModel);
        if (viewModel == null)
            return;
        ViewModel.MainViewModel = viewModel;

        // Add buttons
        if (viewModel.CurrentTabInfo != null)
        {
            viewModel.CurrentTabInfo.Buttons.Clear();
            viewModel.CurrentTabInfo.Buttons.Add(new("Settings.Button.ToggleFullscreen", Buttons.X, (b) =>
            {
                if (viewModel.WindowState == WindowState.FullScreen)
                    viewModel.WindowState = WindowState.Normal;
                else
                    viewModel.WindowState = WindowState.FullScreen;
                viewModel.Config.LastWindowState = viewModel.WindowState;
            }));
        }
    }

    private void OnThemeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel mainViewModel &&
            Application.Current is App app &&
            sender is ComboBox comboBox &&
            comboBox.SelectedItem is SettingsViewModel.Theme theme)
        {
            app.RequestedThemeVariant = theme.Variant;
            mainViewModel.Config.Theme = theme.Variant.Key.ToString();
        }
    }

    private void OnProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel mainViewModel &&
            sender is ComboBox comboBox &&
            comboBox.SelectedItem is ModProfile profile)
        {
            mainViewModel.SelectedProfile = profile;
        }
    }

    private void OnLanguageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel mainViewModel &&
            sender is ComboBox comboBox &&
            comboBox.SelectedItem is LanguageEntry language)
        {
            App.ChangeLanguage(language);
            mainViewModel.Config.Language = language.Code;
            mainViewModel.SelectedLanguage = language;
        }
    }

    private async void OnModsDirectoryChangeClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainViewModel)
            return;

        if (TopLevel.GetTopLevel(this) is not TopLevel topLevel)
            return;

        var storageProvider = topLevel.StorageProvider;

        var picker = await storageProvider.OpenFolderPickerAsync(new()
        {
            Title = Localize("Modal.Title.SelectMods"),
            SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(ViewModel.ModsDirectory),
            AllowMultiple = false
        });

        if (picker.Count == 1)
        {
            string path = Path.GetDirectoryName(Uri.UnescapeDataString(picker[0].Path.AbsolutePath))!;
            if (path.StartsWith("/run/user"))
            {
                MessageBoxModal.CreateOK("Modal.Title.SelectModsFailed", "Modal.Message.SelectModsError")
                    .Open(mainViewModel);
                return;
            }

            ViewModel.ModsDirectory = path;
            await mainViewModel.Save();
            mainViewModel.RefreshUI();
        }
    }

    private void OnModsDirClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = ViewModel.ModsDirectory,
            UseShellExecute = true
        });
    }

    private void OnGameDirClick(object? sender, RoutedEventArgs e)
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

    private async void OnInstallMLClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainViewModel
            || mainViewModel.SelectedGame is not UIGame uiGame)
            return;

        if (_isInstalling)
            return;
        _isInstalling = true;
        await mainViewModel.InstallModLoader();
        ViewModel.Update();
        _isInstalling = false;
    }

    private void OnPrefixClearClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainViewModel)
            return;

        string prefixPath = mainViewModel.SelectedGame?.Game.PrefixRoot ?? string.Empty;
        if (string.IsNullOrEmpty(prefixPath))
            return;

        if (Directory.Exists(prefixPath))
        {
            var messageBox = new MessageBoxModal("Modal.Title.Confirm", "Settings.Message.ClearPrefix");
            messageBox.AddButton("Common.Button.Yes", (s, a) =>
            {
                if (Directory.Exists(prefixPath))
                    Directory.Delete(prefixPath, true);
                messageBox.Close();
            });
            messageBox.AddButton("Common.Button.No", (s, a) => messageBox.Close());
            messageBox.Open(mainViewModel);
        }
    }

    private async void OnPrefixReinstallClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainViewModel)
            return;

        mainViewModel.IsBusy = true;
        await MainWindowViewModel.CreateSimpleDownload("Download.Text.InstallRuntime", "Failed to install runtime",
            async (d, p, c) =>
            {
                if (mainViewModel.SelectedGame == null)
                    return;

                await LinuxCompatibility.InstallRuntimeToPrefix(
                    mainViewModel.SelectedGame.Game.PrefixRoot);
            }).OnFinally((d) =>
            {
                mainViewModel.IsBusy = false;
                return Task.CompletedTask;
            }).RunAsync(mainViewModel);
    }

    private void OnPrefixOpenClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainViewModel)
            return;

        if (mainViewModel.SelectedGame == null)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = mainViewModel.SelectedGame.Game.PrefixRoot ?? string.Empty,
            UseShellExecute = true
        });
    }

    private async void OnCheckManagerUpdatesClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.CheckManagerUpdatesText = "Settings.Button.CheckingUpdates";
        var button = sender as Basic.Button;
        if (button != null)
            button.IsEnabled = false;
        if (DataContext is MainWindowViewModel mainViewModel)
            await mainViewModel.CheckForManagerUpdates();
        if (button != null)
            button.IsEnabled = true;
        ViewModel.CheckManagerUpdatesText = "Settings.Button.CheckUpdates";
    }

    private async void OnCheckModLoaderUpdatesClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.CheckLoaderUpdatesText = "Settings.Button.CheckingUpdates";
        var button = sender as Basic.Button;
        if (button != null)
            button.IsEnabled = false;
        if (DataContext is MainWindowViewModel mainViewModel)
            await mainViewModel.CheckForModLoaderUpdates();
        if (button != null)
            button.IsEnabled = true;
        ViewModel.CheckLoaderUpdatesText = "Settings.Button.CheckUpdates";
    }

    private async void OnCheckModUpdatesClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.CheckModUpdatesText = "Settings.Button.CheckingUpdates";
        var button = sender as Basic.Button;
        if (button != null)
            button.IsEnabled = false;
        if (DataContext is MainWindowViewModel mainViewModel)
            await mainViewModel.CheckForAllModUpdates();
        if (button != null)
            button.IsEnabled = true;
        ViewModel.CheckModUpdatesText = "Settings.Button.CheckUpdates";
    }

    private async void OnCheckCodeUpdatesClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.CheckCodeUpdatesText = "Settings.Button.CheckingUpdates";
        var button = sender as Basic.Button;
        if (button != null)
            button.IsEnabled = false;
        if (DataContext is MainWindowViewModel mainViewModel)
            await mainViewModel.UpdateCodes(true, false);
        if (button != null)
            button.IsEnabled = true;
        ViewModel.CheckCodeUpdatesText = "Settings.Button.CheckUpdates";
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == GameProperty)
            ViewModel.Game = Game?.Game as ModdableGameGeneric;
        else if (e.Property == ProfileProperty)
            ViewModel.Update();
        base.OnPropertyChanged(e);
    }
}