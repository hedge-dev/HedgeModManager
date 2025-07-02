using CommunityToolkit.Mvvm.ComponentModel;

namespace HedgeModManager.UI.ViewModels;

public partial class ProfileManagerViewModel : ViewModelBase
{
    [ObservableProperty] private MainWindowViewModel? _mainWindowViewModel;
    [ObservableProperty] private ModProfile? _selectedProfile = null;

    public bool CanSelect => MainWindowViewModel?.IsBusy == false && SelectedProfile != null;
    public bool CanDuplicate => MainWindowViewModel?.IsBusy == false && SelectedProfile != null;
    public bool CanDelete => MainWindowViewModel?.IsBusy == false && SelectedProfile != MainWindowViewModel?.SelectedProfile && MainWindowViewModel?.Profiles.Count > 1;
    public bool CanRename => MainWindowViewModel?.IsBusy == false && SelectedProfile != null;

    public ProfileManagerViewModel() { }

    public void Update()
    {
        OnPropertyChanged(nameof(CanSelect));
        OnPropertyChanged(nameof(CanDuplicate));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(CanRename));
    }
}
