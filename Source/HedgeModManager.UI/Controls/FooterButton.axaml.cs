using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HedgeModManager.UI.Controls.Primitives;

namespace HedgeModManager.UI.Controls;

public partial class FooterButton : ButtonUserControl
{
    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<FooterButton, Geometry>(nameof(Icon));

    public static readonly StyledProperty<ButtonsOLD> ButtonProperty =
        AvaloniaProperty.Register<FooterButton, ButtonsOLD>(nameof(Button));

    public static Dictionary<ButtonsOLD, string> ButtonMappingXbox = new()
    {
        { ButtonsOLD.A, "Geometry.A" },
        { ButtonsOLD.B, "Geometry.B" },
        { ButtonsOLD.X, "Geometry.X" },
        { ButtonsOLD.Y, "Geometry.Y" },
    };

    public static Dictionary<ButtonsOLD, string> ButtonMappingPS = new()
    {
        { ButtonsOLD.A, "Geometry.PSCross" },
        { ButtonsOLD.B, "Geometry.PSCircle" },
        { ButtonsOLD.X, "Geometry.PSSquare" },
        { ButtonsOLD.Y, "Geometry.PSTriangle" },
    };

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ButtonsOLD Button
    {
        get => GetValue(ButtonProperty);
        set => SetValue(ButtonProperty, value);
    }

    public FooterButton()
    {
        AvaloniaXamlLoader.Load(this);
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