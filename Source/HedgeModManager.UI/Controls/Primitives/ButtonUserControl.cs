using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using HedgeModManager.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeModManager.UI.Controls.Primitives
{
    [PseudoClasses(":pressed")]
    public class ButtonUserControl : UserControl
    {
        public static readonly RoutedEvent<ButtonClickEventArgs> ClickEvent =
            RoutedEvent.Register<GameSelectButton, ButtonClickEventArgs>(nameof(Click),
                RoutingStrategies.Bubble);

        public event EventHandler<ButtonClickEventArgs>? Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        protected void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
            PseudoClasses.Set(":pressed", true);
        }

        protected void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            bool wasHandled = e.Handled;
            e.Handled = true;
            if (PseudoClasses.Contains(":pressed") &&
                        this.GetVisualsAt(e.GetPosition(this))
                         .Any(c => this == c || this.IsVisualAncestorOf(c)) &&
                         !wasHandled)
                RaiseEvent(new ButtonClickEventArgs(ClickEvent, e));

            if (PseudoClasses.Contains(":pressed"))
                PseudoClasses.Remove(":pressed");
        }
    }
}
