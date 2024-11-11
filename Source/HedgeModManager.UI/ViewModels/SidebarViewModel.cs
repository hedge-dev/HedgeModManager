using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeModManager.UI.ViewModels
{
    public partial class SidebarViewModel : ObservableObject
    {
        [ObservableProperty] private int _selectedTabIndex;
        [ObservableProperty] private bool _isSetupCompleted;

    }
}
