using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Controls;
using System.ComponentModel;

namespace HedgeModManager.UI.ViewModels;

public partial class SidebarViewModel : ObservableObject
{
    public Sidebar? SidebarElement = null;

    [ObservableProperty] private MainWindowViewModel? _mainViewModel = null;

    public int SelectedTabIndex
    {
        get => MainViewModel?.SelectedTabIndex ?? 0;
        set
        {
            if (MainViewModel != null)
                MainViewModel.SelectedTabIndex = value;
            OnPropertyChanged();
        }
    }
    public bool IsSetupCompleted => MainViewModel?.Config.IsSetupCompleted == true;
    public bool IsCodesTabVisible
    {
        get
        {
            var game = MainViewModel?.GetModdableGameGeneric();
            if (game != null)
                return game.SupportsCodes;
            return false;
        }
    }

    public void UpdateButtons(List<Control> buttons)
    {
        // Update button layout
        for (int i = 0; i < buttons.Count; ++i)
            if (buttons[i] is SidebarButton button)
                button.IsSelected = SelectedTabIndex == buttons.IndexOf(button);
    }

    public void Update()
    {
        OnPropertyChanged(nameof(SelectedTabIndex));
        OnPropertyChanged(nameof(IsSetupCompleted));
        OnPropertyChanged(nameof(IsCodesTabVisible));
        if (SidebarElement != null)
            UpdateButtons(SidebarElement.TabButtons.Children.ToList());
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel))
            Update();
        base.OnPropertyChanged(e);
    }
}
