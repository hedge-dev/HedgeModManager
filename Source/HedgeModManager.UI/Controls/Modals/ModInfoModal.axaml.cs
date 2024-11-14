using Avalonia.Controls;
using Avalonia.Interactivity;
using HedgeModManager.Foundation;
using HedgeModManager.UI.ViewModels.Mods;
using System;
using System.Linq;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ModInfoModal : UserControl
{
    public ModEntryViewModel ModViewModel { get; set; }
    public ModInfoViewModel InfoViewModel { get; set; }

    public string HeaderText
    {
        get
        {
            if (ModViewModel.Mod.Authors.Count == 0)
                return ModViewModel.Mod.Title;
            if (ModViewModel.Mod is ModGeneric mod)
                return $"{mod.Title} by {mod.AuthorShort}";
            else
                return $"{ModViewModel.Mod.Title} by {ModViewModel.Mod.Authors.FirstOrDefault()}";
        }
    }

    // Preview only
    public ModInfoModal()
    {
        ModViewModel = new ModEntryViewModel();
        InfoViewModel = new ModInfoViewModel(ModViewModel);
        InitializeComponent();
    }

    public ModInfoModal(ModEntryViewModel modEntryViewModel)
    {
        ModViewModel = modEntryViewModel;
        InfoViewModel = new ModInfoViewModel(ModViewModel);
        InitializeComponent();
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        InfoViewModel.UpdateButtons();
    }

    private void OnFavoriteClick(object? sender, RoutedEventArgs e)
    {
        ModViewModel.Mod.Attributes ^= ModAttribute.Favorite;
        InfoViewModel.UpdateButtons();
    }
}