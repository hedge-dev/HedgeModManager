using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using HedgeModManager.UI.Controls.Primitives;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls;

[PseudoClasses(":top", ":middle", ":bottom", ":selected")]
public partial class SidebarButton : ButtonUserControl
{
    public static readonly StyledProperty<ButtonType> TypeProperty =
        AvaloniaProperty.Register<SidebarButton, ButtonType>(nameof(Type), defaultValue: ButtonType.Normal);

    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<SidebarButton, Geometry>(nameof(Icon));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<SidebarButton, bool>(nameof(IsSelected), defaultValue: false);

    public SidebarButtonViewModel ViewModel { get; set; } = new();

    public DispatcherTimer? Timer;

    public ButtonType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set
        {
            PseudoClasses.Set(":selected", value);
            SetValue(IsSelectedProperty, value);
        }
    }

    public SidebarButton()
    {
        AvaloniaXamlLoader.Load(this);

        Timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        Timer.Tick += (sender, e) =>
        {
            ViewModel.ShowDisabled = true;
            Timer.Stop();
        };
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        ViewModel.ShowDisabled = !IsEnabled;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsEnabledProperty)
        {
            if (change.NewValue as bool? == false)
                Timer?.Start();
            else
            {
                Timer?.Stop();
                ViewModel.ShowDisabled = false;
            }
        }
        base.OnPropertyChanged(change);
    }

    public enum ButtonType
    {
        Normal,
        Tab
    }
}