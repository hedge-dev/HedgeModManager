using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Controls.Basic;
using HedgeModManager.UI.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace HedgeModManager.UI.Controls;

public partial class FooterButton : ButtonUserControl
{
    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<FooterButton, Geometry>(nameof(Icon));

    public static readonly StyledProperty<Buttons> ButtonProperty =
        AvaloniaProperty.Register<FooterButton, Buttons>(nameof(Button));

    public static Dictionary<Buttons, string> ButtonMappingXbox = new()
    {
        { Buttons.A, "Geometry.A" },
        { Buttons.B, "Geometry.B" },
        { Buttons.X, "Geometry.X" },
        { Buttons.Y, "Geometry.Y" },
    };

    public static Dictionary<Buttons, string> ButtonMappingPS = new()
    {
        { Buttons.A, "Geometry.PSCross" },
        { Buttons.B, "Geometry.PSCircle" },
        { Buttons.X, "Geometry.PSSquare" },
        { Buttons.Y, "Geometry.PSTriangle" },
    };

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Buttons Button
    {
        get => GetValue(ButtonProperty);
        set => SetValue(ButtonProperty, value);
    }

    public FooterButton()
    {
        InitializeComponent();
    }

    public void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TabInfo.TabButton button)
        {
            Click += (s, e) => button.RaisePressed();

            if (ButtonMappingPS.ContainsKey(button.Button))
                Bind(IconProperty, Resources.GetResourceObservable(ButtonMappingPS[button.Button]));
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ButtonProperty)
        {
            if (DataContext is TabInfo.TabButton button && ButtonMappingPS.ContainsKey(button.Button))
                Bind(IconProperty, Resources.GetResourceObservable(ButtonMappingPS[button.Button]));
        }

        base.OnPropertyChanged(change);
    }
}