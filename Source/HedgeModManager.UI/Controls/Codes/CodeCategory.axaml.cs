using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.CodeCompiler;
using HedgeModManager.UI.Controls.Primitives;
using HedgeModManager.UI.Events;
using HedgeModManager.UI.ViewModels.Codes;

namespace HedgeModManager.UI.Controls.Codes;

public partial class CodeCategory : ButtonUserControl
{
    public static readonly StyledProperty<bool> PressedProperty =
    AvaloniaProperty.Register<CodeCategory, bool>(nameof(Pressed));

    public static readonly StyledProperty<double> RotationAngleProperty =
    AvaloniaProperty.Register<CodeCategory, double>(nameof(RotationAngle));

    public bool Pressed
    {
        get => GetValue(PressedProperty);
        set => SetValue(PressedProperty, value);
    }

    public double RotationAngle
    {
        get => GetValue(RotationAngleProperty);
        set => SetValue(RotationAngleProperty, value);
    }

    public CodeCategory()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        Click += OnClick;
        if (DataContext is CodeCategoryViewModel viewModel)
        {
            RotationAngle = viewModel.Expanded ? 90 : 0;
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CodeCategoryViewModel)
        {
            // Preview
            var category = new CodeCategoryViewModel(null, "Example Root Category", null);
            category.Expanded = true;
            for (int i = 0; i < 3; i++)
            {
                category.Codes.Add(new(new CSharpCode()
                {
                    Name = $"Code {i}",
                    Enabled = i == 0
                }, new()));
                var subCategory = new CodeCategoryViewModel(category, $"Sub Category {i}", null);
                for (int i2 = 0; i2 < 3; i2++)
                {
                    subCategory.Codes.Add(new(new CSharpCode()
                    {
                        Name = $"Code {i2}",
                        Enabled = i2 == 1
                    }, new()));
                }
                subCategory.Expanded = i == 1;
                category.Categories.Add(subCategory);
            }
            Codes.UpdateCategories(category);
            DataContext = category;
        }
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
        if (DataContext is CodeCategoryViewModel viewModel && viewModel.MainViewModel != null)
            viewModel.MainViewModel.CodeDescription = string.Empty;
    }

    public void OnClick(object? sender, ButtonClickEventArgs e)
    {
        e.Handled = true;
        if (DataContext is CodeCategoryViewModel viewModel)
        {
            viewModel.Expanded = !viewModel.Expanded;
            RotationAngle = viewModel.Expanded ? 90 : 0;
        }
    }
}