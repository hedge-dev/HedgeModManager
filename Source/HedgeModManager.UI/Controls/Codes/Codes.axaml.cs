using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.CodeCompiler;
using HedgeModManager.Foundation;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Codes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace HedgeModManager.UI.Controls.Codes;

public partial class Codes : UserControl
{
    public MainWindowViewModel? MainViewModel;

    public ObservableCollection<CodeCategoryViewModel> Cateories { get; set; } = [];
    public ObservableCollection<CodeEntryViewModel> CodesList { get; set; } = [];

    public Codes()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public CodeCategoryViewModel CreateCategory(string path)
    {
        var split = path.Split('/');
        CodeCategoryViewModel? currentCategory = null;
        if (!(split.Length == 1 && string.IsNullOrEmpty(split[0])))
        {
            for (int i = 0; i < split.Length; ++i)
            {
                if (currentCategory == null)
                {
                    currentCategory = Cateories.FirstOrDefault(x => x.Name == split[i]);
                    if (currentCategory == null)
                    {
                        currentCategory = new(null, split[i]);
                        Cateories.Add(currentCategory);
                    }
                }
                else
                {
                    var nextCategory = currentCategory.Categories.FirstOrDefault(x => x.Name == split[i]);
                    if (nextCategory == null)
                    {
                        nextCategory = new(currentCategory, split[i]);
                        currentCategory.Categories.Add(nextCategory);
                    }
                    currentCategory = nextCategory;
                }
            }
        }
        if (currentCategory == null)
        {
            currentCategory = new(null, "Uncategorized");
            Cateories.Add(currentCategory);
        }

        return currentCategory;
    }

    public void LogCodes(int level, CodeCategoryViewModel codeCategory)
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
            .Select(x => new CodeEntryViewModel(x))
            .ToList()
            .ForEach(CodesList.Add);

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

        // Generate categories
        Cateories.Clear();
        foreach (var code in CodesList)
        {
            var category = CreateCategory(code.Code.Category);
            category.Codes.Add(code);
        }

        // Test log
        Logger.Debug("Codes:");
        foreach (var category in Cateories)
            LogCodes(1, category);

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
}