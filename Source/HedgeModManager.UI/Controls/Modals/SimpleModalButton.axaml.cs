using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HedgeModManager.UI.Controls.Primitives;

namespace HedgeModManager.UI.Controls.Modals;

public partial class SimpleModalButton : ButtonUserControl
{
    public static readonly StyledProperty<Geometry> IconProperty =
    AvaloniaProperty.Register<FooterButton, Geometry>(nameof(Icon));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SimpleModalButton, string?>(nameof(Text));

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public SimpleModalButton()
    {
        AvaloniaXamlLoader.Load(this);
    }
}