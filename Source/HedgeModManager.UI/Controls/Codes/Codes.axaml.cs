using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HedgeModManager.CodeCompiler;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Codes;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace HedgeModManager.UI.Controls.Codes;

public partial class Codes : UserControl
{
    public static readonly StyledProperty<UIGame?> GameProperty =
    AvaloniaProperty.Register<Codes, UIGame?>(nameof(Game));

    public UIGame? Game
    {
        get => GetValue(GameProperty);
        set => SetValue(GameProperty, value);
    }

    public ObservableCollection<CodeCategoryViewModel> Cateories { get; set; } = new();

    // Don't use
    public ObservableCollection<CodeEntryViewModel> CodesList { get; set; } = new();

    public Codes()
    {
        InitializeComponent();
    }

    public void LogCodes(int level, CodeCategoryViewModel codeCategory)
    {
        if (codeCategory != null)
        {
            Logger.Debug($"{new string(' ', level * 4)}{codeCategory.Name}/");
            foreach (var category in codeCategory.Cateories)
                LogCodes(level + 1, category);
            foreach (var code in codeCategory.Codes)
                Logger.Debug($"{new string(' ', level * 4)}{code.Code.Name}");
        }
    }

    public ObservableCollection<CodeCategoryViewModel> MergeCategories(ObservableCollection<CodeCategoryViewModel> root)
    {
        var categoryGroups = root.GroupBy(x => x.Name);
        var categories = new ObservableCollection<CodeCategoryViewModel>();

        // Merge codes
        foreach (var group in categoryGroups)
        {
            var category = group.First();
            categories.Add(category);

            if (group.Count() == 1)
                continue;

            // Move Codes
            group.Skip(1)
                 .SelectMany(x => x.Codes)
                 .ToList()
                 .ForEach(category.Codes.Add);

            // Move sub categories
            group.Skip(1)
                 .SelectMany(x => x.Cateories)
                 .ToList()
                 .ForEach(category.Cateories.Add);

            //Merge sub categories
            category.Cateories = MergeCategories(category.Cateories);
        }
        return categories;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var viewModel = (DataContext as MainWindowViewModel);
        if (viewModel == null)
            return;

        if (Game == null)
            return;

        // Generate categories
        Cateories.Clear();
        Game.Game.ModDatabase.Codes
            .Select(x => x as CSharpCode)
            .Where(x => x != null)
            .DistinctBy(x => x!.Category)
            .Select(x => new CodeCategoryViewModel(null, x!.Category))
            .ToList()
            .ForEach(Cateories.Add);

        // Add codes

        // Merge categories
        var cateories = MergeCategories(Cateories);

        // Test log
        foreach (var category in cateories)
            LogCodes(0, category);

        // Add buttons
        if (viewModel.CurrentTabInfo != null)
        {
            viewModel.CurrentTabInfo.Buttons.Clear();
            viewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.SavePlay", Buttons.Y, async (s, e) =>
            {
                await viewModel.SaveAndRun();
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Codes.Button.UpdateCodes", Buttons.X, (s, e) =>
            {
                Logger.Information("Update Codes Pressed");
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Select", Buttons.A, (s, e) =>
            {
                Logger.Information("Select Pressed");
            }));
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == GameProperty)
        {
            CodesList.Clear();
            if (Game == null)
            {
                Cateories.Clear();
                return;
            }
            Game.Game.ModDatabase.Codes
                .Select(x => new CodeEntryViewModel(x))
                .ToList()
                .ForEach(CodesList.Add);
        }

        base.OnPropertyChanged(change);
    }
}