using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using HedgeModManager.UI.Controls.Primitives;

namespace HedgeModManager.UI.Controls.Mods;

public partial class ModEntryFeatureButton : ButtonUserControl
{
    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<ModEntryFeatureButton, Geometry>(nameof(Icon));

    public static readonly StyledProperty<bool> EnabledProperty =
        AvaloniaProperty.Register<ModEntryFeatureButton, bool>(nameof(Enabled));

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool Enabled
    {
        get => GetValue(EnabledProperty);
        set => SetValue(EnabledProperty, value);
    }

    public ModEntryFeatureButton()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            object? icon = null;
            Application.Current?.TryFindResource("Geometry.Gear", out icon);
            if (icon is Geometry geometry)
                Icon = geometry;
        }
    }
}