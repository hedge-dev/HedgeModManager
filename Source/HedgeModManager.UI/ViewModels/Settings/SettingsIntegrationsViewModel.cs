using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.ViewModels.Settings;

public partial class SettingsIntegrationsViewModel : ViewModelBase
{
    [ObservableProperty] private MainWindowViewModel? _mainViewModel;
    [ObservableProperty] private string _localGameBananaRemoteDLID = string.Empty;
    [ObservableProperty] private string _localGameBananaRemoteDLKey = string.Empty;

    public bool GameBananaEnabled
    {
        get => MainViewModel?.Config?.Integrations?.GameBananaEnabled == true;
        set
        {
            if (MainViewModel?.Config != null)
            {
                MainViewModel.Config.Integrations.GameBananaEnabled = value;

                // Update server state
                if (value && MainViewModel.Config.Integrations.GameBananaRemoteDLEnabled)
                    _ = MainViewModel.GameBananaRemoteDLServerInstance.StartServerAsync();
                else
                    MainViewModel.GameBananaRemoteDLServerInstance.StopServer();

                _ = MainViewModel.Config.SaveAsync();
            }

            OnPropertyChanged(nameof(GameBananaEnabled));
        }
    }

    public bool GameBananaRemoteDLEnabled
    {
        get => MainViewModel?.Config?.Integrations?.GameBananaRemoteDLEnabled == true;
        set
        {
            if (MainViewModel?.Config != null)
            {
                MainViewModel.Config.Integrations.GameBananaRemoteDLEnabled = value;
                _ = MainViewModel.Config.SaveAsync();
            }

            OnPropertyChanged(nameof(GameBananaRemoteDLEnabled));
        }
    }

    public string LoginButtonText
    {
        get => Localize(MainViewModel?.Config?.Integrations?.GameBananaRemoteDLEnabled == true ?
            "Common.Button.Logout" : "Common.Button.Login");
    }

    public void Update()
    {
        OnPropertyChanged(nameof(GameBananaEnabled));
        OnPropertyChanged(nameof(GameBananaRemoteDLEnabled));
        OnPropertyChanged(nameof(LoginButtonText));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel))
            Update();
        base.OnPropertyChanged(e);
    }
}
