using CommunityToolkit.Mvvm.ComponentModel;

namespace HedgeModManager.UI.ViewModels
{
    public partial class SidebarViewModel : ObservableObject
    {
        [ObservableProperty] private int _selectedTabIndex;
        [ObservableProperty] private bool _isSetupCompleted;

    }
}
