using Avalonia;
using HedgeModManager.UI.Controls.Primitives;

namespace HedgeModManager.UI.Controls.Modals;

public partial class SimpleModalButton : ButtonUserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SimpleModalButton, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public SimpleModalButton()
    {
        InitializeComponent();
    }
}