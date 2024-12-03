using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.ViewModels.Mods;

public partial class ModInfoViewModel : ViewModelBase
{
    public ModEntryViewModel ModViewModel { get; set; }

    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _modText = "";
    [ObservableProperty] private string _authorText = "";
    [ObservableProperty] private string _favoriteButtonText = "Mods.Button.Favorite";
    [ObservableProperty] private string _description = "";

    public ModInfoViewModel(ModEntryViewModel modEntryViewModel)
    {
        ModViewModel = modEntryViewModel;
        Title = Localize("Modal.Title.AboutMod", modEntryViewModel.Mod.Title);
        ModText = Localize("Modal.Header.AboutMod", modEntryViewModel.Mod.Title, modEntryViewModel.Mod.Version);
        string desc = modEntryViewModel.Mod.Description.Replace("\\n", "\n\n ");
        if (ModViewModel.Mod is ModGeneric mod)
        {
            AuthorText = Localize("Modal.Header.AboutModAuthor", mod.AuthorShort, mod.Date);
            if (!string.IsNullOrEmpty(mod.Markdown))
                desc = mod.Markdown;
        }
        else
            AuthorText = Localize("Modal.Header.AboutModAuthor", modEntryViewModel.Authors, modEntryViewModel.Mod.Date);
        Description = Utils.ConvertToHTML(desc);
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        FavoriteButtonText = ModViewModel.Mod.Attributes.HasFlag(ModAttribute.Favorite)
            ? Localize("Mods.Button.Favorited") : Localize("Mods.Button.Favorite");
    }
}
