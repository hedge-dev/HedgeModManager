using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Controls.Primitives;
using HedgeModManager.UI.Events;
using HedgeModManager.UI.ViewModels.Mods;

namespace HedgeModManager.UI.Controls.Mods;

[PseudoClasses(":enabled")]
public partial class ModEntry : ButtonUserControl
{
    public DispatcherTimer? HoldTimer;
    public bool HoldPointerOver = false;
    public bool HoldHandled = false;
    public bool StartDragging = false;

    public ModEntry()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        Click += OnClick;
        if (DataContext is ModEntryViewModel viewModel && viewModel.MainViewModel != null)
        {
            HoldTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            HoldTimer.Tick += (sender, e) =>
            {
                HoldTimer.Stop();
                if (HoldPointerOver)
                {
                    new ModInfoModal(viewModel).Open(viewModel.MainViewModel);
                    HoldHandled = true;
                    StartDragging = false;
                }
            };

            viewModel.UpdateSearch();
        }
    }

    public void OnClick(object? sender, ButtonClickEventArgs e)
    {
        if (DataContext is ModEntryViewModel viewModel)
        {
            if (e.MouseButton == MouseButton.Left)
                viewModel.ModEnabled = !viewModel.ModEnabled;
            else if (e.MouseButton == MouseButton.Right && viewModel.MainViewModel != null)
                new ModInfoModal(viewModel).Open(viewModel.MainViewModel);
        }
    }

    public new void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        HoldPointerOver = true;
        HoldHandled = false;
        StartDragging = true;
        HoldTimer?.Start();
        if (DataContext is ModEntryViewModel viewModel)
            viewModel.DragOffset = e.GetPosition(this);
        base.OnPointerPressed(sender, e);
    }

    public new void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        HoldTimer?.Stop();
        e.Handled = HoldHandled;
        StartDragging = false;
        base.OnPointerReleased(sender, e);
    }

    public void OnPointerExited(object? sender, PointerEventArgs e)
    {
        HoldTimer?.Stop();
    }

    public void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (HoldTimer != null || StartDragging)
        {
            HoldPointerOver = this.GetVisualsAt(e.GetPosition(this))
                .Any(c => this == c || this.IsVisualAncestorOf(c));

            if (StartDragging && !HoldPointerOver)
            {
                StartDragging = false;
                if (DataContext is ModEntryViewModel viewModel)
                    viewModel.IsDraging = true;
            }
        }
    }

    public void OnConfigClick(object? sender, ButtonClickEventArgs e)
    {
        e.Handled = true;
        if (DataContext is ModEntryViewModel viewModel && 
            viewModel.MainViewModel is not null)
        {
            var mainViewModel = viewModel.MainViewModel;

            if (!viewModel.HasConfig)
                return;

            var modal = new ModConfigModal(viewModel);
            modal.Open(mainViewModel);
        }
    }

    public void OnSaveClick(object? sender, ButtonClickEventArgs e)
    {
        e.Handled = true;
    }

    public void OnCodeClick(object? sender, ButtonClickEventArgs e)
    {
        e.Handled = true;
    }

    public void OnFavoriteClick(object? sender, ButtonClickEventArgs e)
    {
        e.Handled = true;
        if (DataContext is ModEntryViewModel viewModel)
            viewModel.UpdateFavorite(!viewModel.Mod.Attributes.HasFlag(Foundation.ModAttribute.Favorite));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
    }
}