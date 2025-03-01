using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace HedgeModManager.UI;

public partial class TabInfo : ObservableObject
{
    [ObservableProperty] private string _name = "Unnamed Tab";
    [ObservableProperty] private ObservableCollection<TabButton> _buttons = [];

    public TabInfo(string name)
    {
        Name = name;
    }

    public partial class TabButton : ObservableObject
    {
        private Action<TabButton>? _onPress;

        [ObservableProperty] private string _name = "Button";
        [ObservableProperty] private ButtonsOLD _button = UI.ButtonsOLD.A;
        [ObservableProperty] private bool _isEnabled = true;

        public TabButton(string name, ButtonsOLD button, Action<TabButton>? onPress, bool enabled = true)
        {
            Name = name;
            Button = button;
            _onPress = onPress;
            IsEnabled = enabled;
        }

        public void RaisePressed()
        {
            _onPress?.Invoke(this);
        }
    }
}
