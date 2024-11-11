using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeModManager.UI
{
    public partial class TabInfo : ObservableObject
    {
        [ObservableProperty] private string _name = "Unnamed Tab";
        [ObservableProperty] private ObservableCollection<TabButton> _buttons = new();

        public TabInfo(string name)
        {
            Name = name;
        }

        public partial class TabButton : ObservableObject
        {
            [ObservableProperty] private string _name = "Button";
            [ObservableProperty] private Buttons _button = UI.Buttons.A;
            [ObservableProperty] private bool _isEnabled = true;
            private event EventHandler? _pressed;

            public event EventHandler? Pressed
            {
                add
                {
                    _pressed += value;
                }
                remove
                {
                    _pressed -= value;
                }
            }

            public TabButton(string name, Buttons button, EventHandler onPress, bool enabled = true)
            {
                Name = name;
                Button = button;
                Pressed += onPress;
                IsEnabled = enabled;
            }

            public void RaisePressed()
            {
                _pressed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
