using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using HedgeModManager.UI.Controls.Primitives;

namespace HedgeModManager.UI.Controls.Basic;

public partial class CheckBox : ButtonUserControl
{
    public static readonly StyledProperty<bool?> IsCheckedProperty =
    AvaloniaProperty.Register<CheckBox, bool?>(nameof(IsChecked), false,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> TextProperty =
    AvaloniaProperty.Register<CheckBox, string?>(nameof(Text), "",
        defaultBindingMode: BindingMode.TwoWay);

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public CheckBox()
    {
        InitializeComponent();
    }

    public void OnLoaded(object sender, RoutedEventArgs e)
    {
        Click += OnClick;
    }

    public void OnClick(object? sender, RoutedEventArgs e)
    {
        IsChecked = !IsChecked;
    }
}