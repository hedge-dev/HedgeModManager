using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls.Modals;

public partial class WindowModal : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WindowModal, string>(nameof(Title), "Title");

    public static readonly StyledProperty<bool> UseTitlePaddingProperty =
        AvaloniaProperty.Register<WindowModal, bool>(nameof(UseTitlePadding), false);

    public static readonly StyledProperty<bool> LargeWindowProperty =
        AvaloniaProperty.Register<WindowModal, bool>(nameof(LargeWindow), false);

    public static readonly StyledProperty<object?> DataProperty =
        AvaloniaProperty.Register<WindowModal, object?>(nameof(Data));

    public static readonly StyledProperty<object?> ButtonsProperty =
        AvaloniaProperty.Register<WindowModal, object?>(nameof(Buttons));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool UseTitlePadding
    {
        get => GetValue(UseTitlePaddingProperty);
        set => SetValue(UseTitlePaddingProperty, value);
    }

    public bool LargeWindow
    {
        get => GetValue(LargeWindowProperty);
        set => SetValue(LargeWindowProperty, value);
    }

    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public object? Buttons
    {
        get => GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    public WindowModal()
    {
        InitializeComponent();
    }

    public Modal? GetBaseModal()
    {
        if (DataContext is MainWindowViewModel viewModel)
            return viewModel.Modals.FirstOrDefault(x => x.Control == this);
        return null;
    }

    public void Open(MainWindowViewModel viewModel)
    {
        Modal baseModal = new(this, new Thickness(0));
        if (LargeWindow)
        {
            baseModal.SizeChanged += (object? sender, SizeChangedEventArgs e) =>
            {
                double ratio = 16 / 9d;
                Size baseSize = new Size(baseModal.Bounds.Width, baseModal.Bounds.Height).Inflate(new(-80));
                double newWidth = 0;
                double newHeight = 0;
                if (baseSize.Width / baseSize.Height > ratio)
                {
                    Height = newHeight = Math.Round(baseSize.Height);
                    Width = Math.Round(newHeight * ratio);
                }
                else
                {
                    Width = newWidth = Math.Round(baseSize.Width);
                    Height = Math.Round(newWidth / ratio);
                }
            };
        }
        viewModel.Modals.Add(baseModal);
    }

    public void Close()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var modalInstance = GetBaseModal();
            if (modalInstance != null)
                viewModel.Modals.Remove(modalInstance);
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == UseTitlePaddingProperty)
        {
            if (change.NewValue is bool value)
                ContentBorder.Padding = new Thickness(0, value ? 32 : 0, 0, 0);
        }
        if (change.Property == ButtonsProperty)
        {
            if (change.NewValue is StackPanel stackPanel)
            {
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                stackPanel.Spacing = 20;
            }
        }
        base.OnPropertyChanged(change);
    }
}