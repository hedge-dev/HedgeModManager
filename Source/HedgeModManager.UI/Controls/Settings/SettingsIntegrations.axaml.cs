using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Settings;

namespace HedgeModManager.UI.Controls.Settings;

public partial class SettingsIntegrations : SettingsBase
{
    public static readonly StyledProperty<UIGame?> GameProperty =
        AvaloniaProperty.Register<SettingsIntegrations, UIGame?>(nameof(Game));

    public static readonly StyledProperty<ModProfile?> ProfileProperty =
        AvaloniaProperty.Register<SettingsIntegrations, ModProfile?>(nameof(Profile));

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

    public SettingsIntegrationsViewModel ViewModel { get; set; } = new();

    public bool _isInstalling = false;

    private void OnTestClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel)
            return;

        Settings?.SwitchPanel(null);
    }

    public SettingsIntegrations()
    {
        AvaloniaXamlLoader.Load(this);
        Title = "Settings.Integrations.Header.Text";
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;

        ViewModel.MainViewModel = viewModel;
        if (ViewModel.MainViewModel?.Config is ProgramConfig config)
        {
            ViewModel.LocalGameBananaRemoteDLID = config.Integrations.GameBananaRemoteDLMemberID;
            ViewModel.LocalGameBananaRemoteDLKey = config.Integrations.GameBananaRemoteDLSecretKey;
        }
        // Add buttons
        if (viewModel.CurrentTabInfo != null)
        {
            viewModel.CurrentTabInfo.Buttons.Clear();
            viewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Back", ButtonsOLD.X, (b) =>
            {
                Settings?.SwitchPanel(null);
            }));
        }
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainViewModel?.Config is ProgramConfig config)
        {
            config.Integrations.GameBananaRemoteDLMemberID = ViewModel.LocalGameBananaRemoteDLID;
            config.Integrations.GameBananaRemoteDLSecretKey = ViewModel.LocalGameBananaRemoteDLKey;
            config.Integrations.GameBananaRemoteDLEnabled = !config.Integrations.GameBananaRemoteDLEnabled;
            _ = config.SaveAsync();

            if (config.Integrations.GameBananaRemoteDLEnabled)
                _ = ViewModel.MainViewModel.GameBananaRemoteDLServerInstance.StartServerAsync();
            else
                ViewModel.MainViewModel.GameBananaRemoteDLServerInstance.StopServer();

            ViewModel.Update();
        }
    }

    private void OnReloadClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.MainViewModel != null)
           ViewModel.MainViewModel.GameBananaRemoteDLServerInstance.LastPoll = DateTime.MinValue;
    }
}