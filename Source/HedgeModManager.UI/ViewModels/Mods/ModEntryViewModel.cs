using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.CoreLib;
using HedgeModManager.Foundation;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace HedgeModManager.UI.ViewModels.Mods;

public partial class ModEntryViewModel : ViewModelBase
{

    public Geometry? GeometryStarSolid = App.GetResource<Geometry>("Geometry.StarSolid");
    public Geometry? GeometryStarOutline = App.GetResource<Geometry>("Geometry.StarOutline");

    [ObservableProperty] private IMod _mod;
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private InlineCollection _modTitle = [];
    [ObservableProperty] private InlineCollection _modAuthor = [];
    [ObservableProperty] private MainWindowViewModel? _mainViewModel;
    [ObservableProperty] private ModsViewModel? _modsViewModel;
    [ObservableProperty] private Geometry? _favoriteGeometry;
    [ObservableProperty] private IBrush? _favoriteBrush;
    [ObservableProperty] private bool _isDraging = false;
    [ObservableProperty] private Point _dragOffset = new();
    [ObservableProperty] private bool _isSelected = false;

    private Regex? _search;

    public Regex? Search
    {
        get => _search;
        set
        {
            _search = value;
            UpdateSearch();
        }
    }

    private bool _isFiltered;

    public bool IsFiltered
    {
        get => _isFiltered;
        set
        {
            _isFiltered = value;
            UpdateSearch();
        }
    }

    private bool _isFeatureFiltered;

    public bool IsFeatureFiltered
    {
        get => _isFeatureFiltered;
        set
        {
            _isFeatureFiltered = value;
            UpdateSearch();
        }
    }

    public bool ModEnabled
    {
        get => Mod.Enabled;
        set
        {
            Mod.Enabled = value;
            OnPropertyChanged(nameof(ModEnabled));
        }
    }

    public string Authors => string.Join(", ", Mod.Authors.Select(x => x.Name));
    public bool HasConfig => Mod is ModGeneric modGeneric && !string.IsNullOrEmpty(modGeneric.ConfigSchemaFile);
    public bool HasSave => Mod is ModGeneric modGeneric && !string.IsNullOrEmpty(modGeneric.SaveFile);
    public bool HasCode => Mod.Codes.Count != 0;

    // Preview only
    public ModEntryViewModel()
    {
        Mod = new ModGeneric()
        {
            Title = "Mod Title",
            Authors =
            [
                new()
                {
                    Name = "Author Name",
                    Url = "https://hedgedocs.com/"
                },
                new()
                {
                    Name = "Author Name 2"
                }
            ],
            AuthorShort = "Team Name",
            Version = "1.0",
            Date = DateTime.Now.ToShortDateString(),
            Enabled = true
        };
        MainViewModel = null;
        UpdateFavorite(null);
    }

    public ModEntryViewModel(IMod mod, MainWindowViewModel? mainViewModel, ModsViewModel? modsViewModel)
    {
        Mod = mod;
        MainViewModel = mainViewModel;
        _modsViewModel = modsViewModel;
        UpdateFavorite(null);
    }

    public void UpdateFavorite(bool? favorite)
    {
        if (favorite == true)
            Mod.Attributes |= ModAttribute.Favorite;
        else if (favorite == false)
            Mod.Attributes &= ~ModAttribute.Favorite;

        FavoriteGeometry = Mod.Attributes.HasFlag(ModAttribute.Favorite) 
            ? GeometryStarSolid : GeometryStarOutline;
        FavoriteBrush = App.GetResource<ImmutableSolidColorBrush>(Mod.Attributes.HasFlag(ModAttribute.Favorite)
            ? "Mods.FavoriteBrush" : "ForegroundBrush");
    }

    public void UpdateSearch()
    {
        bool updateInlines(InlineCollection inlines, Regex? regex, string str)
        {
            inlines.Clear();

            if (regex == null)
            {
                inlines.Add(new Run(str));
                return true;
            }
            var matches = regex.Matches(str);

            var lastIndex = 0;
            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                    inlines.Add(new Run(str[lastIndex..match.Index]));

                inlines.Add(new Run(str[match.Index..(match.Index + match.Length)])
                {
                    FontWeight = FontWeight.Bold
                });
                lastIndex = match.Index + match.Length;
            }
            if (lastIndex < str.Length)
                inlines.Add(new Run(str[lastIndex..]));

            return matches.Count > 0;
        }

        Dispatcher.UIThread.Post(() =>
        {
            bool hasMatch = false;
            hasMatch |= updateInlines(ModTitle, Search, Mod.Title);
            hasMatch |= updateInlines(ModAuthor, Search, Authors);
            IsVisible = hasMatch && !IsFiltered && !IsFeatureFiltered;
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Mod))
        {
            OnPropertyChanged(nameof(ModEnabled));
            OnPropertyChanged(nameof(Authors));
            OnPropertyChanged(nameof(HasConfig));
            OnPropertyChanged(nameof(HasSave));
            OnPropertyChanged(nameof(HasCode));
        }
        base.OnPropertyChanged(e);
    }
}
