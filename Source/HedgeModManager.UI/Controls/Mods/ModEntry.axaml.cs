using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Controls.Primitives;
using HedgeModManager.UI.Events;
using HedgeModManager.UI.ViewModels.Mods;
using System;

namespace HedgeModManager.UI.Controls.Mods;

[PseudoClasses(":enabled")]
public partial class ModEntry : ButtonUserControl
{

    public ModEntry()
    {
        InitializeComponent();
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        Click += (s, e) => OnClick(s, (ButtonClickEventArgs)e);

        if (DataContext is ModEntryViewModel viewModel)
            viewModel.UpdateSearch();
    }

    public void OnClick(object? sender, ButtonClickEventArgs e)
    {
        if (DataContext is ModEntryViewModel viewModel)
        {
            if (e.MouseButton == MouseButton.Left)
                viewModel.ModEnabled = !viewModel.ModEnabled;
            else if (e.MouseButton == MouseButton.Right && viewModel.MainViewModel != null)
                viewModel.MainViewModel.Modals.Add(new Modal(new ModInfoModal(viewModel)));
        }
    }

    public void OnFavoriteClick(object? sender, ButtonClickEventArgs e)
    {
        if (DataContext is ModEntryViewModel viewModel)
        {
            viewModel.UpdateFavorite(!viewModel.Mod.Attributes.HasFlag(Foundation.ModAttribute.Favorite));
            e.Handled = true;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
    }
}