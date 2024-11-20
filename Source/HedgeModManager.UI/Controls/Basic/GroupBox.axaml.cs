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

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
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