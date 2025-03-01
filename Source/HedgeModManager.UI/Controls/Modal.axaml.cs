using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls;

public partial class Modal : UserControl
{
    public object? Control { get; set; }
    public Thickness BorderPadding { get; set; } = new(20);

    public static readonly StyledProperty<Color> AltBackgroundColorProperty =
        AvaloniaProperty.Register<Modal, Color>(nameof(AltBackgroundColor),
            Color.FromArgb(0x7F, 0, 0, 0));

    public Color AltBackgroundColor
    {
        get => GetValue(AltBackgroundColorProperty);
        set => SetValue(AltBackgroundColorProperty, value);
    }

    public Modal()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public Modal(object? content, Thickness? padding = null) : this()
    {
        Control = content;
        if (padding != null)
            BorderPadding = padding.Value;
    }

    public void Close()
    {
        (DataContext as MainWindowViewModel)?.Modals.Remove(this);
    }

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Close();
    }
}