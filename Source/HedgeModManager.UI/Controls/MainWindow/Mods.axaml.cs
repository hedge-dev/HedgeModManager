using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using HedgeModManager.UI.Controls.Mods;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Mods;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace HedgeModManager.UI.Controls.MainWindow;

public partial class Mods : UserControl
{
    public static readonly StyledProperty<string?> SearchProperty =
        AvaloniaProperty.Register<Mods, string?>(nameof(Search),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string? Search
    {
        get => GetValue(SearchProperty);
        set => SetValue(SearchProperty, value);
    }

    public MainWindowViewModel? MainViewModel;
    public ModsViewModel ModsViewModel { get; set; } = new();

    public Mods()
    {
        InitializeComponent();
    }

    public void UpdateModList()
    {
        if (MainViewModel == null ||
            MainViewModel.Mods == null)
        {
            ModsViewModel.ModsList.Clear();
            ModsViewModel.Authors.Clear();
            ModsViewModel.UpdateText();
            return;
        }

        ModsViewModel.ModsList.Clear();
        MainViewModel.Mods
            .Select(x => new ModEntryViewModel(x, MainViewModel, ModsViewModel))
            .ToList()
            .ForEach(ModsViewModel.ModsList.Add);

        ModsViewModel.Authors.Clear();
        ModsViewModel.Authors.Add("Show All");
        MainViewModel.Mods
            .SelectMany(x => x.Authors)
            .Select(x => x.Name)
            .Distinct()
            .ToList()
            .ForEach(ModsViewModel.Authors.Add);
        AuthorComboBox.SelectedIndex = 0;
        ModsViewModel.UpdateText();
    }

    private void OnFilterClick(object? sender, RoutedEventArgs e)
    {
        if (sender is ModEntryFeatureButton button)
            button.Enabled = !button.Enabled;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        MainViewModel = DataContext as MainWindowViewModel;
        if (MainViewModel == null)
            return;

        // Subscribe to changes
        MainViewModel.Mods.CollectionChanged += OnModCollectionChanged;
        MainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        UpdateModList();

        AuthorComboBox.SelectedIndex = 0;

        //// Add buttons
        //if (MainViewModel.CurrentTabInfo != null)
        //{
        //    MainViewModel.CurrentTabInfo.Buttons.Clear();
        //    MainViewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.SavePlay", Buttons.Y, async (s, e) =>
        //    {
        //        await MainViewModel.SaveAndRun();
        //    }));
        //    MainViewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Menu", Buttons.B, (s, e) =>
        //    {
        //        Logger.Information("Menu Pressed");
        //    }));
        //    MainViewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Options", Buttons.X, (s, e) =>
        //    {
        //        Logger.Information("Options Pressed");
        //    }));
        //    MainViewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Select", Buttons.A, (s, e) =>
        //    {
        //        Logger.Information("Select Pressed");
        //    }));
        //}
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (MainViewModel == null)
            return;
        MainViewModel.Mods.CollectionChanged -= OnModCollectionChanged;
        MainViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
    }

    private void OnModCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateModList();
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.Codes))
            UpdateModList();
    }

    private void OnAuthorSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            if (comboBox.SelectedIndex <= 0)
            {
                foreach (var mod in ModsViewModel.ModsList)
                    mod.IsFiltered = false;
            }
            else
            {
                foreach (var mod in ModsViewModel.ModsList)
                    mod.IsFiltered = !mod.Mod.Authors
                        .Any(x => x.Name == comboBox.SelectedItem as string);
            }
            ModsViewModel.UpdateText();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SearchProperty)
        {
            if (string.IsNullOrEmpty(Search))
            {
                foreach (var mod in ModsViewModel.ModsList)
                    mod.Search = null;
            } else
            {
                try
                {
                    var regex = new Regex(Search, RegexOptions.IgnoreCase);
                    foreach (var mod in ModsViewModel.ModsList)
                        mod.Search = regex;
                }
                catch
                {
                    foreach (var mod in ModsViewModel.ModsList)
                        mod.Search = null;
                }
            }
            ModsViewModel.UpdateText();
        }

        base.OnPropertyChanged(change);
    }
}