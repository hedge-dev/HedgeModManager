using Avalonia;
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

    public void Open(MainWindowViewModel viewModel)
    {
        viewModel.Modals.Add(new Modal(this, new Thickness(0)));
    }

    public void Close()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var modalInstance = viewModel.Modals.FirstOrDefault(x => x.Control == this);
            if (modalInstance != null)
                viewModel.Modals.Remove(modalInstance);
        }
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        InfoViewModel.UpdateButtons();
    }

    private void OnFavoriteClick(object? sender, RoutedEventArgs e)
    {
        ModViewModel.Mod.Attributes ^= ModAttribute.Favorite;
        InfoViewModel.UpdateButtons();
        ModViewModel.UpdateFavorite(null);
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