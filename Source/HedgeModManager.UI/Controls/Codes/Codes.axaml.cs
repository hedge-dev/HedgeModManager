using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.CodeCompiler;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Codes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace HedgeModManager.UI.Controls.Codes;

public partial class Codes : UserControl
{
    public static readonly StyledProperty<string> SearchProperty =
    AvaloniaProperty.Register<Codes, string>(nameof(Search),
        defaultValue: string.Empty,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<ObservableCollection<CodeCategoryViewModel>> CategoryViewProperty =
    AvaloniaProperty.Register<Codes, ObservableCollection<CodeCategoryViewModel>>(nameof(CategoryView),
        defaultValue: [],
        defaultBindingMode: BindingMode.TwoWay);

    public string Search
    {
        get => GetValue(SearchProperty);
        set => SetValue(SearchProperty, value);
    }

    public ObservableCollection<CodeCategoryViewModel> CategoryView
    {
        get => GetValue(CategoryViewProperty);
        set => SetValue(CategoryViewProperty, value);
    }

    public MainWindowViewModel? MainViewModel;

    public ObservableCollection<CodeCategoryViewModel> Cateories { get; set; } = [];
    public ObservableCollection<CodeEntryViewModel> CodesList { get; set; } = [];

    public List<CodeCategoryViewModel> CategoryList { get; set; } = [];

    public Codes()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public CodeCategoryViewModel CreateCategory(string path, IList<CodeCategoryViewModel> cateories, bool shouldUpdate = true, bool shouldExpand = false)
    {
        var split = path.Split('/');
        CodeCategoryViewModel? currentCategory = null;
        if (!(split.Length == 1 && string.IsNullOrEmpty(split[0])))
        {
            for (int i = 0; i < split.Length; ++i)
            {
                if (currentCategory == null)
                {
                    currentCategory = cateories.FirstOrDefault(x => x.Name == split[i]);
                    if (currentCategory == null)
                    {
                        currentCategory = new(null, split[i], shouldUpdate ? MainViewModel : null)
                        {
                            Expanded = shouldExpand
                        };
                        cateories.Add(currentCategory);
                    }
                }
                else
                {
                    var nextCategory = currentCategory.Categories.FirstOrDefault(x => x.Name == split[i]);
                    if (nextCategory == null)
                    {
                        nextCategory = new(currentCategory, split[i], shouldUpdate ? MainViewModel : null)
                        {
                            Expanded = shouldExpand
                        };

                        currentCategory.Categories.Add(nextCategory);
                    }
                    currentCategory = nextCategory;
                }
            }
        }
        if (currentCategory == null)
        {
            currentCategory = new(null, "Codes.Text.Uncategorized", shouldUpdate ? MainViewModel : null)
            {
                Expanded = shouldExpand
            };
            cateories.Add(currentCategory);
        }

        return currentCategory;
    }

    public static void UpdateCategories(CodeCategoryViewModel? category)
    {
        if (category == null)
            return;

        // Merge empty categories
        if (category.Categories.Count == 1 && category.Codes.Count == 0)
        {
            var oldCategory = category.Categories[0];
            category.Categories.Clear();
            category.Name += $" / {oldCategory.Name}";
            category.Categories = oldCategory.Categories;
            category.Codes = oldCategory.Codes;
        }

        foreach (var subCategory in category.Categories)
            UpdateCategories(subCategory);

        foreach (var code in category.Codes)
            if (code.IsLastElement)
                code.IsLastElement = false;

        if (category.Codes.Count == 0)
        {
            if (category.Categories.Count != 0)
                category.Categories.Last().IsLastElement = true;
            else
                Logger.Debug($"Attempted to update empty category with name \"{category.Name}\"");
        }
        else
        {
            category.Codes.Last().IsLastElement = true;
        }
    }

    public static List<CodeCategoryViewModel> GetCategories(CodeCategoryViewModel? category)
    {
        var categories = new List<CodeCategoryViewModel>();
        if (category == null)
            return categories;
        categories.Add(category);
        foreach (var subCategory in category.Categories)
            categories.AddRange(GetCategories(subCategory));
        return categories;
    }

    public static void LogCodes(int level, CodeCategoryViewModel codeCategory)
    {
        if (codeCategory != null)
        {
            Logger.Debug($"{new string(' ', level * 2)}{codeCategory.Name}/");
            foreach (var category in codeCategory.Categories)
                LogCodes(level + 1, category);
            foreach (var code in codeCategory.Codes)
                Logger.Debug($"{new string(' ', (level + 1) * 2)}{code.Code.Name}");
        }
    }

    public void RefreshUI()
    {
        // TODO: Implement notification of CodesList changes. Adding is slow
        CodesList.Clear();
        if (MainViewModel == null ||
            MainViewModel.Codes == null)
            return;

        MainViewModel.Codes
            .Where(x => x.Type != CodeType.Library && x is CSharpCode)
            .Cast<CSharpCode>()
            .Select(x => new CodeEntryViewModel(x, MainViewModel))
            .ToList()
            .ForEach(CodesList.Add);

        var expandedCategories = CategoryList
            .Where(x => x.Expanded)
            .Select(x => x.ToString())
            .ToList();
        if (MainViewModel.SelectedGameConfig is GameConfig gameConfig)
            expandedCategories.AddRange(gameConfig.ExpandedCodes);

        Cateories.Clear();
        CategoryList.Clear();
        foreach (var code in CodesList)
        {
            var category = CreateCategory(code.Code.Category, Cateories);
            category.Codes.Add(code);
        }

        foreach (var category in Cateories)
        {
            UpdateCategories(category);
            CategoryList.AddRange([category, ..GetCategories(category)]);
        }

        foreach (var categoryPath in expandedCategories)
        {
            var category = CategoryList.FirstOrDefault(x => x.ToString() == categoryPath);
            if (category != null)
                category.Expanded = true;
        }
        CategoryView = Cateories;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        MainViewModel = DataContext as MainWindowViewModel;
        if (MainViewModel == null)
            return;

        // Subscribe to changes
        MainViewModel.Codes.CollectionChanged += OnCodesCollectionChanged;
        MainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        RefreshUI();
        MainViewModel.CodeDescription = "Codes.Text.NoCodeSelected";

        // Add buttons
        if (MainViewModel.CurrentTabInfo != null)
        {
            MainViewModel.CurrentTabInfo.Buttons.Clear();
            MainViewModel.CurrentTabInfo.Buttons.Add(new("Codes.Button.UpdateCodes", ButtonsOLD.X, async (b) =>
            {
                b.IsEnabled = false;
                await MainViewModel.UpdateCodesAsync(true, false);
                b.IsEnabled = true;
            }));
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (MainViewModel == null)
            return;
        MainViewModel.Codes.CollectionChanged -= OnCodesCollectionChanged;
        MainViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
    }

    private void OnCodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshUI();
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.Codes))
            RefreshUI();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SearchProperty)
        {
            if (string.IsNullOrEmpty(Search))
            {
                foreach (var category in Cateories)
                    UpdateCategories(category);
                CategoryView = Cateories;
                return;
            }

            var searchedCodes = CodesList
                .Where(x => x.Code.Name.Contains(Search, StringComparison.CurrentCultureIgnoreCase)
                || x.Code.Category.Contains(Search, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
            var seatchedCategories = new List<CodeCategoryViewModel>();
            foreach (var code in searchedCodes)
            {
                var category = CreateCategory(code.Code.Category, seatchedCategories, false, true);
                category.Codes.Add(code);
            }
            foreach (var category in seatchedCategories)
                UpdateCategories(category);
            CategoryView = new(seatchedCategories);
        }
    }
}