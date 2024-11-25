using Avalonia.Data;
using Avalonia;
using Avalonia.Controls.Documents;

namespace HedgeModManager.UI.Controls.Basic;

public class TextBlock : Avalonia.Controls.TextBlock
{
    public static readonly StyledProperty<InlineCollection?> InlineSourceProperty =
        AvaloniaProperty.Register<TextBlock, InlineCollection?>(nameof(InlineSource),
            defaultBindingMode: BindingMode.TwoWay);

    public InlineCollection? InlineSource
    {
        get => GetValue(InlineSourceProperty);
        set => SetValue(InlineSourceProperty, value);
    }

    public TextBlock() : base() { }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == InlineSourceProperty)
        {
            var textBlock = e.Sender as TextBlock;
            if (textBlock == null)
                return;

            textBlock.Inlines = InlineSource;
        }
        base.OnPropertyChanged(e);
    }
}
