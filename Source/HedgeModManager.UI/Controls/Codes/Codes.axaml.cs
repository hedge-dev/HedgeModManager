using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.CodeCompiler;
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

    //public void LogCodes(int level, CodeCategoryViewModel codeCategory)
    //{
    //    if (codeCategory != null)
    //    {
    //        Logger.Debug($"{new string(' ', level * 4)}{codeCategory.Name}/");
    //        foreach (var category in codeCategory.Cateories)
    //            LogCodes(level + 1, category);
    //        foreach (var code in codeCategory.Codes)
    //            Logger.Debug($"{new string(' ', level * 4)}{code.Code.Name}");
    //    }
    //}

    //public ObservableCollection<CodeCategoryViewModel> MergeCategories(ObservableCollection<CodeCategoryViewModel> root)
    //{
    //    var categoryGroups = root.GroupBy(x => x.Name);
    //    var categories = new ObservableCollection<CodeCategoryViewModel>();
    //
    //    // Merge codes
    //    foreach (var group in categoryGroups)
    //    {
    //        var category = group.First();
    //        categories.Add(category);
    //
    //        if (group.Count() == 1)
    //            continue;
    //
    //        // Move Codes
    //        group.Skip(1)
    //             .SelectMany(x => x.Codes)
    //             .ToList()
    //             .ForEach(category.Codes.Add);
    //
    //        // Move sub categories
    //        group.Skip(1)
    //             .SelectMany(x => x.Cateories)
    //             .ToList()
    //             .ForEach(category.Cateories.Add);
    //
    //        //Merge sub categories
    //        category.Cateories = MergeCategories(category.Cateories);
    //    }
    //    return categories;
    //}

    public void RefreshUI()
    {
        // Switch to mods if codes are not supported
        if (MainViewModel?.GetModdableGameGeneric()?.SupportsCodes == false)
        {
            if (MainViewModel.SelectedTabIndex == MainViewModel.GetTabIndex("Codes"))
                MainViewModel.SelectedTabIndex -= 1;
            return;
        }

        // TODO: Implement notfication of CodesList changes. Adding is slow
        CodesList.Clear();
        if (MainViewModel == null ||
            MainViewModel.Codes == null)
            return;

        MainViewModel.Codes
            .Where(x => x.Type != Foundation.CodeType.Library && x is CSharpCode)
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

        //// Generate categories
        //Cateories.Clear();
        //MainViewModel.Codes
        //    .Select(x => x as CSharpCode)
        //    .Where(x => x != null)
        //    .DistinctBy(x => x!.Category)
        //    .Select(x => new CodeCategoryViewModel(null, x!.Category))
        //    .ToList()
        //    .ForEach(Cateories.Add);
        //
        //// Merge categories
        //var cateories = MergeCategories(Cateories);
        //
        //// Test log
        //foreach (var category in cateories)
        //    LogCodes(0, category);

        // Add buttons
        if (MainViewModel.CurrentTabInfo != null)
        {
            MainViewModel.CurrentTabInfo.Buttons.Clear();
            //MainViewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.SavePlay", Buttons.Y, async (b) =>
            //{
            //    await MainViewModel.SaveAndRun();
            //}));
            MainViewModel.CurrentTabInfo.Buttons.Add(new("Codes.Button.UpdateCodes", Buttons.X, async (b) =>
            {
                b.IsEnabled = false;
                await MainViewModel.UpdateCodes(true, false);
                b.IsEnabled = true;
            }));
            //MainViewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Select", Buttons.A, (b) =>
            //{
            //    Logger.Information("Select Pressed");
            //}));
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