using Avalonia;
using Avalonia.Controls;

namespace HedgeModManager.UI.Controls.Settings;

public partial class SettingsEntry : UserControl
{
    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<SettingsEntry, string>(nameof(Icon),
            defaultValue: string.Empty);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingsEntry, string>(nameof(Title),
            defaultValue: string.Empty);

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<SettingsEntry, string>(nameof(Description),
            defaultValue: string.Empty);

    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<SettingsEntry, string>(nameof(Value),
            defaultValue: string.Empty);

    public static readonly StyledProperty<object?> DataProperty =
        AvaloniaProperty.Register<SettingsEntry, object?>(nameof(Content));

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public SettingsEntry()
    {
        InitializeComponent();
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Title))
        {
            Title = "<Title>";
            if (string.IsNullOrEmpty(Icon))
                Icon = "ab-testing";
            if (string.IsNullOrEmpty(Description))
                Description = "<Description>";
            if (string.IsNullOrEmpty(Value))
                Value = "<Value>";
        }
    }
}