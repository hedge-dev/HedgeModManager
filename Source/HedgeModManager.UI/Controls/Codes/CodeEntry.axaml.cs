using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using HedgeModManager.UI.Controls.Primitives;
using HedgeModManager.UI.Events;
using HedgeModManager.UI.ViewModels.Codes;

namespace HedgeModManager.UI.Controls.Codes;

[PseudoClasses(":enabled")]
public partial class CodeEntry : ButtonUserControl
{
    public static readonly StyledProperty<bool> PressedProperty =
        AvaloniaProperty.Register<CodeCategory, bool>(nameof(Pressed));

    public bool Pressed
    {
        get => GetValue(PressedProperty);
        set => SetValue(PressedProperty, value);
    }

    public CodeEntry()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        Click += OnClick;
    }

    protected new void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);
        Pressed = PseudoClasses.Contains(":pressed");
    }

    protected new void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(sender, e);
        Pressed = PseudoClasses.Contains(":pressed");
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is CodeEntryViewModel viewModel)
            viewModel.MainViewModel.SelectedCode = viewModel.Code;
    }

    public void OnClick(object? sender, ButtonClickEventArgs e)
    {
        e.Handled = true;
        if (DataContext is CodeEntryViewModel viewModel)
            viewModel.Enabled = !viewModel.Enabled;
    }
}