using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Settings;

namespace HedgeModManager.UI.Controls.Settings;

public partial class Settings : UserControl
{
    public static readonly StyledProperty<UIGame?> GameProperty =
        AvaloniaProperty.Register<Settings, UIGame?>(nameof(Game));

    public static readonly StyledProperty<ModProfile?> ProfileProperty =
        AvaloniaProperty.Register<Settings, ModProfile?>(nameof(Profile));

    public SettingsBase? MainSettingsControl { get; set; }
    public SettingsViewModel ViewModel { get; set; } = new ();

    public UIGame? Game
    {
        get => GetValue(GameProperty);
        set => SetValue(GameProperty, value);
    }

    public ModProfile? Profile
    {
        get => GetValue(ProfileProperty);
        set => SetValue(ProfileProperty, value);
    }

    public Settings()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;

        var settingsMain = new SettingsMain();
        settingsMain.ViewModel.MainViewModel = viewModel;
        settingsMain.Settings = this;

        settingsMain.Bind(Layoutable.WidthProperty, new Binding()
        {
            Source = ScrollViewerPanel,
            Path = "Viewport.Width"
        });
        settingsMain.Bind(SettingsMain.GameProperty, new Binding()
        {
            Source = viewModel,
            Path = "SelectedGame"
        });
        settingsMain.Bind(SettingsMain.ProfileProperty, new Binding()
        {
            Source = viewModel,
            Path = "SelectedProfile"
        });

        MainSettingsControl = settingsMain;
        Panels.Children.Clear();
        Panels.Children.Add(MainSettingsControl);
        UpdateTitle();
    }

    // TODO: Needs better handling
    public async Task SwitchPanel(SettingsBase? control)
    {
        if (control != null && control == Panels.Children.LastOrDefault())
            return;

        if (control == null && MainSettingsControl == Panels.Children.LastOrDefault())
            return;

        if (MainSettingsControl != null &&
            (control == MainSettingsControl || control == null))
        {
            Panels.Children.Insert(0, MainSettingsControl);
            MainSettingsControl.Bind(Layoutable.WidthProperty, new Binding()
            {
                Source = ScrollViewerPanel,
                Path = "Viewport.Width"
            });
            double xTranslate = ScrollViewerPanel.Viewport.Width * -(Panels.Children.Count - 1);
            Panels.Margin = new Thickness(xTranslate, 0, 0, 0);
            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new LinearEasing(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {

                        Cue = new Cue(1.0),
                        Setters =
                        {
                            new Setter(Panel.MarginProperty, new Thickness(0))
                        }
                    }
                 }
            };

            await animation.RunAsync(Panels);
            Panels.Margin = new Thickness(0);
            Panels.Children.RemoveAt(1);
            UpdateTitle();
        }else if (control != null)
        {
            Panels.Children.Add(control);
            control.Settings = this;
            control.SettingsParent = Panels.Children.ElementAt(0) as SettingsBase;

            control.Bind(Layoutable.WidthProperty, new Binding()
            {
                Source = ScrollViewerPanel,
                Path = "Viewport.Width"
            });
            double xTranslate = ScrollViewerPanel.Viewport.Width * -(Panels.Children.Count - 1);
            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new LinearEasing(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {

                        Cue = new Cue(1.0),
                        Setters =
                        {
                            new Setter(Panel.MarginProperty, new Thickness(xTranslate, 0, 0, 0))
                        }
                    }
                 }
            };

            await animation.RunAsync(Panels);
            // TODO
            UpdateTitle();
            Panels.Margin = new Thickness(0);
            Panels.Children.RemoveAt(0);
        }
    }

    public void UpdateTitle()
    {
        TextBlock directoryTextBlock = new()
        {
            Text = ">",
            Margin = new Thickness(0, 0, 18, 0),
            FontSize = 30,
            FontWeight = FontWeight.Bold
        };
        TitlePanel.Children.Clear();
        foreach (var child in Panels.Children)
        {
            if (child is not SettingsBase settings)
                continue;

            TextBlock textBlock = new()
            {
                Margin = new Thickness(0, 0, 18, 0),
                FontSize = 30,
                FontWeight = FontWeight.Bold
            };
            textBlock.Bind(
                TextBlock.TextProperty,
                new DynamicResourceExtension(settings.Title)
            );
            textBlock.PointerReleased += async (s, e) =>
            {
                await SwitchPanel(settings);
            };

            if (Panels.Children.IndexOf(child) != 0)
                TitlePanel.Children.Add(directoryTextBlock);
            TitlePanel.Children.Add(textBlock);
        }
    }
}