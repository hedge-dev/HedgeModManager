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
        [ObservableProperty] private string? _centerText = "Mods.Text.Loading";
        [ObservableProperty] private int _selectedModIndex = -1;

        public ObservableCollection<string> Authors { get; set; } = [];
        public ObservableCollection<ModEntryViewModel> ModsList { get; set; } = [];

        public void UpdateText()
        {
            int visibleCount = ModsList.Count(x =>
            {
                bool isFiltered = x.IsFiltered || x.IsFeatureFiltered;
                if (x.Search != null)
                {
                    isFiltered |= !(x.Search.Matches(x.Mod.Title).Count != 0 || 
                    x.Search.Matches(x.Authors).Count != 0);
                }
                return !isFiltered;
            });
            int modCount = ModsList.Count;
            if (modCount == 0)
                CenterText = "Mods.Text.NoMods";
            else if (visibleCount == 0)
                CenterText = "Mods.Text.NoResults";
            else
                CenterText = null;
        }

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
                        bool isConfig = item.HasConfig;
                        bool isSave = item.HasSave;
                        bool isCode = item.HasCode;
                        bool isFavorite = item.Mod.Attributes.HasFlag(Foundation.ModAttribute.Favorite);
                        item.IsFeatureFiltered = !(
                            (ShowConfig && isConfig) ||
                            (ShowSave && isSave) ||
                            (ShowCode && isCode) ||
                            (ShowFavorite && isFavorite));
                    }
                }
                UpdateText();
            }

            if (e.PropertyName == nameof(SelectedModIndex))
            {
                var selectedMod = ModsList.Where(x => x.IsVisible).Skip(SelectedModIndex).FirstOrDefault();
                if (selectedMod != null)
                {
                    int index = ModsList.IndexOf(selectedMod);
                    for (int i = 0; i < ModsList.Count; i++)
                        ModsList[i].IsSelected = i == SelectedModIndex;
                }
            }
            base.OnPropertyChanged(e);
        }
    }
}
