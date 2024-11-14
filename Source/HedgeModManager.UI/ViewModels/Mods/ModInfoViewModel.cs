using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HedgeModManager.UI.ViewModels.Mods
{
    public partial class ModInfoViewModel : ViewModelBase
    {
        public ModEntryViewModel ModViewModel { get; set; }

        [ObservableProperty] private string _favoriteButtonText = "Mods.Button.Favorite";

        public ModInfoViewModel(ModEntryViewModel modEntryViewModel)
        {
            ModViewModel = modEntryViewModel;
        }

        public void UpdateButtons()
        {
            FavoriteButtonText = ModViewModel.Mod.Attributes.HasFlag(ModAttribute.Favorite)
                ? "Mods.Button.Favorited" : "Mods.Button.Favorite";
        }
    }
}
