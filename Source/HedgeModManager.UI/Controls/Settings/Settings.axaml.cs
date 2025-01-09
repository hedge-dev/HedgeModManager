using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
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

    public UIGame? Game
    {
        get => GetValue(GameProperty);
        set => SetValue(GameProperty, value);
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
        if (_isInstalling)
            return;
        _isInstalling = true;
        if (DataContext is not MainWindowViewModel mainViewModel
            || mainViewModel.SelectedGame is not UIGame uiGame)
            return;

        await uiGame.Game.InstallModLoaderAsync();
        ViewModel.Update();
        _isInstalling = false;
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
        base.OnPropertyChanged(e);
    }
}