using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.Foundation;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Mods;
using System.Diagnostics;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ModInfoModal : WindowModal
{
    public ModEntryViewModel ModViewModel { get; set; }
    public ModInfoViewModel InfoViewModel { get; set; }

    // Preview only
    public ModInfoModal()
    {
        ModViewModel = new ModEntryViewModel();
        InfoViewModel = new ModInfoViewModel(ModViewModel);
        AvaloniaXamlLoader.Load(this);
    }

    public ModInfoModal(ModEntryViewModel modEntryViewModel)
    {
        ModViewModel = modEntryViewModel;
        InfoViewModel = new ModInfoViewModel(ModViewModel);
        AvaloniaXamlLoader.Load(this);
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        Bind(TitleProperty, new Binding("Title")
        {
            Source = InfoViewModel
        });
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

    private void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = ModViewModel.Mod.Root,
            UseShellExecute = true
        });
    }

    private async void OnUpdateClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;
        Close();
        await viewModel.CheckForModUpdatesAsync(ModViewModel.Mod, true);
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        if (viewModel == null)
            return;

        string modTitle = ModViewModel.Mod.Title;
        if (modTitle.Length > 30)
            modTitle = modTitle[..30] + "c";
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
        modal.SetDanger();

        modal.Open(viewModel);
    }
}