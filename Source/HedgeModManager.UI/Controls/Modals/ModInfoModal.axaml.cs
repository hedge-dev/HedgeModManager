using Avalonia.Controls;
using Avalonia.Interactivity;
using HedgeModManager.Foundation;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Mods;
using System;
using System.Linq;
using static HedgeModManager.UI.Languages.Language;

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

    private void OnConfigureClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;

        var modal = new ModConfigModal(ModViewModel);
        modal.Open(viewModel);
    }

    private void OnUpdateClick(object? sender, RoutedEventArgs e)
    {
        Logger.Information("Update Clicked");
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;

        var modal = new MessageBoxModal("Modal.Title.Confirm",
            Localize("Modal.Message.DeleteMod", ModViewModel.Mod.Title));

        modal.AddButton("Common.Button.Delete", (s, e) =>
        {
            if (ModViewModel.Mod is ModGeneric mod)
            {
                var database = viewModel.SelectedGame?.Game.ModDatabase as ModDatabaseGeneric;
                database?.DeleteMod(mod);
                ModViewModel.ModsViewModel?.ModsList.Remove(ModViewModel);
                ModViewModel.ModsViewModel?.UpdateText();
            }

            viewModel.Modals.RemoveAt(viewModel.Modals.Count - 1);
            viewModel.Modals.RemoveAt(viewModel.Modals.Count - 1);
        });
        modal.AddButton("Common.Button.Cancel", (s, e) =>
        {
            viewModel.Modals.RemoveAt(viewModel.Modals.Count - 1);
        });

        modal.Open(viewModel);
    }

}