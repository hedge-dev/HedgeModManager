using Avalonia.Input;
using Avalonia.Interactivity;

namespace HedgeModManager.UI.Events;

public class ButtonClickEventArgs : RoutedEventArgs
{
    public MouseButton MouseButton { get; set; }

    public ButtonClickEventArgs(RoutedEvent routedEvent, PointerReleasedEventArgs args) : base(routedEvent)
    {
        MouseButton = args.InitialPressMouseButton;
    }
}
