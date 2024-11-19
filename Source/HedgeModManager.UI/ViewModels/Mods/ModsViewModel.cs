using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HedgeModManager.UI.ViewModels.Mods
{
    public partial class ModsViewModel : ViewModelBase
    {

        [ObservableProperty] private bool _showConfig = false;
        [ObservableProperty] private bool _showSave = false;
        [ObservableProperty] private bool _showCode = false;
        [ObservableProperty] private bool _showFavorite = false;
        [ObservableProperty] private bool _hasMods = false;

        public ObservableCollection<string> Authors { get; set; } = new();
        public ObservableCollection<ModEntryViewModel> ModsList { get; set; } = new();

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShowConfig) ||
                e.PropertyName == nameof(ShowSave) ||
                e.PropertyName == nameof(ShowCode) ||
                e.PropertyName == nameof(ShowFavorite))
            {
                if (!(ShowConfig || ShowSave || ShowCode || ShowFavorite))
                {
                    // Remove filter
                    foreach (var item in ModsList)
                        item.IsFeatureFiltered = false;
                }
                else
                {
                    foreach (var item in ModsList)
                    {
                        bool isConfig = false;
                        bool isSave = false;
                        bool isCode = item.Mod.Codes.Count != 0;
                        bool isFavorite = item.Mod.Attributes.HasFlag(Foundation.ModAttribute.Favorite);
                        item.IsFeatureFiltered = !(
                            (ShowConfig && isConfig) ||
                            (ShowSave && isSave) ||
                            (ShowCode && isCode) ||
                            (ShowFavorite && isFavorite));
                    }
                }

            }
            base.OnPropertyChanged(e);
        }

    }
}
