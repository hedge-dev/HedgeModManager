using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HedgeModManager.UI.Controls.Primitives;
using System;
using ValveKeyValue;

namespace HedgeModManager.UI.Controls;

[PseudoClasses(":top", ":middle", ":bottom", ":selected")]
public partial class SidebarButton : ButtonUserControl
{
    public static readonly StyledProperty<ButtonType> TypeProperty =
     AvaloniaProperty.Register<SidebarButton, ButtonType>(nameof(Type), defaultValue: ButtonType.Normal);

    public static readonly StyledProperty<ButtonOrder> OrderProperty =
     AvaloniaProperty.Register<SidebarButton, ButtonOrder>(nameof(Order), defaultValue: ButtonOrder.Normal);

    public static readonly StyledProperty<Geometry> IconProperty =
     AvaloniaProperty.Register<SidebarButton, Geometry>(nameof(Icon));

    public static readonly StyledProperty<bool> IsSelectedProperty =
     AvaloniaProperty.Register<SidebarButton, bool>(nameof(IsSelected), defaultValue: false);

    public ButtonType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public ButtonOrder Order
    {
        get => GetValue(OrderProperty);
        set {
            PseudoClasses.Add($":{value.ToString().ToLower()}");
            SetValue(OrderProperty, value);
        }
    }

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set
        {
            PseudoClasses.Set(":selected", value);
            SetValue(IsSelectedProperty, value);
        }
    }

    public SidebarButton()
    {
        InitializeComponent();
    }

    public enum ButtonType
    {
        Normal,
        Tab
    }

    public enum ButtonOrder
    {
        Normal,
        Top,
        Middle,
        Bottom
    }
}