using Avalonia;
using Avalonia.Controls;

namespace HedgeModManager.UI.Controls.Settings;

public partial class SettingsBase : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingsBase, string>(nameof(Title), "Untitled");

    public static readonly StyledProperty<Settings?> SettingsProperty =
        AvaloniaProperty.Register<SettingsBase, Settings?>(nameof(Settings));

    public static readonly StyledProperty<SettingsBase?> SettingsParentProperty =
        AvaloniaProperty.Register<SettingsBase, SettingsBase?>(nameof(SettingsParent));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public Settings? Settings
    {
        get => GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    public SettingsBase? SettingsParent
    {
        get => GetValue(SettingsParentProperty);
        set => SetValue(SettingsParentProperty, value);
    }
}