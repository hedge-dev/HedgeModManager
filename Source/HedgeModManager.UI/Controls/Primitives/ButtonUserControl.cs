using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
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
        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<GameSelectButton, RoutedEventArgs>(nameof(Click),
                RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs>? Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        protected void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            PseudoClasses.Set(":pressed", true);
        }

        protected void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (PseudoClasses.Contains(":pressed") &&
                        this.GetVisualsAt(e.GetPosition(this))
                         .Any(c => this == c || this.IsVisualAncestorOf(c)))
                RaiseEvent(new RoutedEventArgs(ClickEvent));

            if (PseudoClasses.Contains(":pressed"))
                PseudoClasses.Remove(":pressed");
        }
    }
}
