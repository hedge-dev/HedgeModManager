using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Controls.Mods;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Mods;
using System.Collections.ObjectModel;
using System.Linq;
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

    public ModsViewModel ModsViewModel { get; set; } = new();
    public ObservableCollection<IMod>? ModList { get; set; }

    public Mods()
    {
        InitializeComponent();
    }

    public void UpdateModList()
    {
        if (ModList == null)
        {
            ModsViewModel.ModsList.Clear();
            ModsViewModel.Authors.Clear();
            ModsViewModel.UpdateText();
            return;
        }

        ModsViewModel.ModsList.Clear();
        ModList
            .Select(x => new ModEntryViewModel(x, DataContext as MainWindowViewModel, ModsViewModel))
            .ToList()
            .ForEach(ModsViewModel.ModsList.Add);

        ModsViewModel.Authors.Clear();
        ModsViewModel.Authors.Add("Show All");
        ModList
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
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        // Subscribe to changes
        if (ModList == null)
        {
            ModList = viewModel.Mods;
            ModList.CollectionChanged += (s, e) => UpdateModList();
            UpdateModList();
        }

        AuthorComboBox.SelectedIndex = 0;

        // Add buttons
        if (viewModel.CurrentTabInfo != null)
        {
            viewModel.CurrentTabInfo.Buttons.Clear();
            viewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.SavePlay", Buttons.Y, async (s, e) =>
            {
                await viewModel.SaveAndRun();
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Menu", Buttons.B, (s, e) =>
            {
                Logger.Information("Menu Pressed");
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Options", Buttons.X, (s, e) =>
            {
                Logger.Information("Options Pressed");
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Common.Button.Select", Buttons.A, (s, e) =>
            {
                Logger.Information("Select Pressed");
            }));
        }
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