using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace HedgeModManager.UI.Controls.Basic;

public partial class GroupBox : UserControl
{
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<GroupBox, string>(nameof(Header), "",
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object> DataProperty =
        AvaloniaProperty.Register<GroupBox, object>(nameof(Content));

    public static readonly StyledProperty<Thickness> DataPaddingProperty =
    AvaloniaProperty.Register<GroupBox, Thickness>(nameof(DataPadding));

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public Thickness DataPadding
    {
        get => GetValue(DataPaddingProperty);
        set => SetValue(DataPaddingProperty, value);
    }

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public GroupBox()
    {
        if (Design.IsDesignMode)
        {
            if (string.IsNullOrEmpty(Header))
                Header = "Header Text";
            Data = "Content";
        }
        InitializeComponent();
    }
}