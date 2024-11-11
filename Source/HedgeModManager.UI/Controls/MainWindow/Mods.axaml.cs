using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Config;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.Controls.Mods;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.ViewModels.Mods;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HedgeModManager.UI.Controls.MainWindow;

public partial class Mods : UserControl
{
    public static readonly StyledProperty<UIGame> GameProperty =
        AvaloniaProperty.Register<Mods, UIGame>(nameof(Game));

    public static readonly StyledProperty<string?> SearchProperty =
        AvaloniaProperty.Register<Mods, string?>(nameof(Search),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public UIGame Game
    {
        get => GetValue(GameProperty);
        set => SetValue(GameProperty, value);
    }

    public string? Search
    {
        get => GetValue(SearchProperty);
        set => SetValue(SearchProperty, value);
    }

    public ObservableCollection<string> Authors { get; set; } = new();

    public ObservableCollection<ModEntryViewModel> ModsList { get; set; } = new();

    public Mods()
    {
        InitializeComponent();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var viewModel = (DataContext as MainWindowViewModel);
        if (viewModel == null)
            return;

        // Add buttons
        if (viewModel.CurrentTabInfo != null)
        {
            viewModel.CurrentTabInfo.Buttons.Clear();
            viewModel.CurrentTabInfo.Buttons.Add(new("Save and Play", Buttons.Y, async (s, e) =>
            {
                await viewModel.SaveAndRun();
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Menu", Buttons.B, (s, e) =>
            {
                Logger.Information("Menu Pressed");
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Options", Buttons.X, (s, e) =>
            {
                Logger.Information("Options Pressed");
            }));
            viewModel.CurrentTabInfo.Buttons.Add(new("Select", Buttons.A, (s, e) =>
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
                foreach (var mod in ModsList)
                    mod.IsFiltered = false;
            }
            else
            {
                foreach (var mod in ModsList)
                    mod.IsFiltered = !mod.Mod.Authors
                        .Any(x => x.Name == comboBox.SelectedItem as string);
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == GameProperty)
        {
            if (Game == null)
            {
                ModsList.Clear();
                Authors.Clear();
                return;
            }
            ModsList.Clear();
            Game.Game.ModDatabase.Mods
                .Select(x => new ModEntryViewModel(x))
                .ToList()
                .ForEach(ModsList.Add);

            Authors.Clear();
            Authors.Add("");
            Game.Game.ModDatabase.Mods
                .SelectMany(x => x.Authors)
                .Select(x => x.Name)
                .Distinct()
                .ToList()
                .ForEach(Authors.Add);
        }

        if (change.Property == SearchProperty)
        {
            if (string.IsNullOrEmpty(Search))
            {
                foreach (var mod in ModsList)
                    mod.Search = null;
            } else
            {
                try
                {
                    var regex = new Regex(Search, RegexOptions.IgnoreCase);
                    foreach (var mod in ModsList)
                        mod.Search = regex;
                }
                catch
                {
                    foreach (var mod in ModsList)
                        mod.Search = null;
                }
            }
        }

        base.OnPropertyChanged(change);
    }
}