using Avalonia;
using Avalonia.Markup.Xaml;
using HedgeModManager.UI.Controls.Primitives;
using Material.Icons;

namespace HedgeModManager.UI.Controls.Basic;

public partial class Button : ButtonUserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<Button, string>(nameof(Text), string.Empty);

    public static readonly StyledProperty<bool> UseIconProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(UseIcon), false);

    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<Button, MaterialIconKind>(nameof(Icon), MaterialIconKind.Abacus);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool UseIcon
    {
        get => GetValue(UseIconProperty);
        set => SetValue(UseIconProperty, value);
    }

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Button()
    {
        AvaloniaXamlLoader.Load(this);
    }
}