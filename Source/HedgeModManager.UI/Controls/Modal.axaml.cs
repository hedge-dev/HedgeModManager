using Avalonia;
using Avalonia.Controls;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls;

public partial class Modal : UserControl
{
    public object? Control { get; set; }
    public Thickness BorderPadding { get; set; } = new(20);

    public Modal()
    {
        InitializeComponent();
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