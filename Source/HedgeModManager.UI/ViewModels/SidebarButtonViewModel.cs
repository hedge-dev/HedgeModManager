
using CommunityToolkit.Mvvm.ComponentModel;

namespace HedgeModManager.UI.ViewModels;

public partial class SidebarButtonViewModel : ObservableObject
{
    [ObservableProperty] private bool _showDisabled;
}
