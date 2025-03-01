using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using HedgeModManager.UI.Controls.Primitives;

namespace HedgeModManager.UI.Controls.Mods;

public partial class ModEntryFeatureButton : ButtonUserControl
{
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<ModEntryFeatureButton, Geometry?>(nameof(Icon));

    public static readonly StyledProperty<bool> EnabledProperty =
        AvaloniaProperty.Register<ModEntryFeatureButton, bool>(nameof(Enabled), 
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<ModEntryFeatureButton, IBrush?>(nameof(Fill));

    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool Enabled
    {
        get => GetValue(EnabledProperty);
        set => SetValue(EnabledProperty, value);
    }

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public ModEntryFeatureButton()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        Fill ??= App.GetResource<ImmutableSolidColorBrush>("ForegroundBrush");
        Icon ??= App.GetResource<Geometry>("Geometry.Gear");
    }
}