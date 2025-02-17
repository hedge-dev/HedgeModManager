using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using HedgeModManager.UI.Controls.Mods;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Mods;
using SharpCompress;
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

    public MainWindowViewModel? MainViewModel = null;
    public ModEntryViewModel? LastDraggedModEntryViewModel = null;
    public ModsViewModel ModsViewModel { get; set; } = new();

    public Mods()
    {
        AvaloniaXamlLoader.Load(this);
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

        // TODO: Needs a better solution
        if (ModsViewModel.ModsList.Count != MainViewModel.Mods.Count)
        {
            ModsViewModel.ModsList.Clear();

            MainViewModel.Mods 
                .Select(x => new ModEntryViewModel(x, MainViewModel, ModsViewModel))
                .ToList()
                .ForEach(ModsViewModel.ModsList.Add);
        }
        else
        {
            for (int i = 0; i < MainViewModel.Mods.Count; ++i)
            {
                ModsViewModel.ModsList[i].Mod = MainViewModel.Mods[i];
                ModsViewModel.ModsList[i].ModEnabled = MainViewModel.Mods[i].Enabled;
            }    
        }

        ModsViewModel.Authors.Clear();
        ModsViewModel.Authors.Add("Show All");
        MainViewModel.Mods
            .SelectMany(x => x.Authors)
            .Select(x => x.Name)
            .Distinct()
            .ToList()
            .Order()
            .ForEach(ModsViewModel.Authors.Add);
        AuthorComboBox.SelectedIndex = 0;
        ModsViewModel.UpdateText();
    }

    private void OnInputPressed(Input.Buttons button)
    {
        int limit = ModsViewModel.ModsList.Count(x => x.IsVisible);

        switch (button)
        {
            case Input.Buttons.Up:
                if (limit == 0)
                    return;

                if (ModsViewModel.SelectedModIndex > 0)
                    ModsViewModel.SelectedModIndex--;
                else
                    ModsViewModel.SelectedModIndex = limit - 1;
                break;
            case Input.Buttons.Down:
                if (limit == 0)
                    return;

                if (ModsViewModel.SelectedModIndex < limit - 1)
                    ModsViewModel.SelectedModIndex++;
                else
                    ModsViewModel.SelectedModIndex = 0;
                break;
            case Input.Buttons.A:
                if (ModsViewModel.SelectedModIndex != -1)
                    ModsViewModel.ModsList[ModsViewModel.SelectedModIndex].ModEnabled = !ModsViewModel.ModsList[ModsViewModel.SelectedModIndex].ModEnabled;
                break;
            default:
                break;
        }
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

        // Register handler
        MainViewModel.CurrentInputPressedHandler = OnInputPressed;

        // Add buttons
        if (MainViewModel.CurrentTabInfo != null)
        {
            MainViewModel.CurrentTabInfo.Buttons.Clear();
            MainViewModel.CurrentTabInfo.Buttons.Add(new("Mods.Button.InstallMod", ButtonsOLD.Back, async (b) =>
            {
                await MainViewModel.InstallModAsync(this, null);
            }));
        }
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
        if (e.PropertyName == nameof(MainWindowViewModel.Mods))
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

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        // Unselect if cursor moved
        if (ModsViewModel.SelectedModIndex != -1)
            ModsViewModel.SelectedModIndex = -1;

        var container = ModsViewModel.ModsList
            .Select(x => ModItemControl.ContainerFromItem(x) as ContentPresenter)
            .FirstOrDefault(x => x?.Child?.DataContext is ModEntryViewModel { IsDraging: true });

        if (container?.Child is ModEntry modEntry)
        {
            if (modEntry.DataContext is not ModEntryViewModel draggingViewModel)
                return;

            if (DragModEntry.DataContext is not ModEntryViewModel modEntryViewModel ||
                modEntryViewModel.Mod != draggingViewModel.Mod)
            {
                var newViewModel = new ModEntryViewModel(draggingViewModel.Mod, MainViewModel, ModsViewModel);
                newViewModel.UpdateSearch();
                DragModEntry.DataContext = newViewModel;
            }

            DragModEntry.IsVisible = true;
            var newMargin = new Thickness(0,
                e.GetPosition(DragModEntry.GetVisualParent()).Y - draggingViewModel.DragOffset.Y,
                0, 0);
            DragModEntry.Margin = newMargin;
            LastDraggedModEntryViewModel = draggingViewModel;

            // Check position in list visually
            int oldIndex = ModsViewModel.ModsList.IndexOf(draggingViewModel);
            int newIndex = 0;
            while (newIndex < ModsViewModel.ModsList.Count - 1)
            {
                var container2 = ModItemControl.ContainerFromIndex(newIndex);
                if (container2 == null ||
                    DragModEntry.Bounds.Top - 24 <
                    container2.Bounds.Top)
                    break;
                ++newIndex;
            }

            if (oldIndex != newIndex)
                ModsViewModel.ModsList.Move(oldIndex, newIndex);
        }
        else
        {
            DragModEntry.IsVisible = false;
            if (DragModEntry.DataContext != null)
                DragModEntry.DataContext = null;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ModsViewModel.ModsList.ForEach(x => x.IsDraging = false);
        if (MainViewModel == null || LastDraggedModEntryViewModel == null)
            return;
        int oldIndex = MainViewModel.Mods.IndexOf(LastDraggedModEntryViewModel.Mod);
        int newIndex = ModsViewModel.ModsList.IndexOf(LastDraggedModEntryViewModel);

        // View is already updated
        MainViewModel.Mods.CollectionChanged -= OnModCollectionChanged;
        MainViewModel.Mods.Move(oldIndex, newIndex);
        MainViewModel.Mods.CollectionChanged += OnModCollectionChanged;
        
        // Apply to game
        if (MainViewModel.SelectedGame?.Game is ModdableGameGeneric gameGeneric)
        {
            var gameMods = gameGeneric.ModDatabase.Mods;
            var removedMod = gameMods[oldIndex];
            gameMods.RemoveAt(oldIndex);
            gameMods.Insert(newIndex, removedMod);
        }
        LastDraggedModEntryViewModel = null;
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