using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
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
        var viewModel = (DataContext as MainWindowViewModel)!;
        viewModel.Modals.Remove(this);
    }

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Close();
    }
}