using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using HedgeModManager.UI.Controls.Primitives;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels.Mods;
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Tmds.DBus.SourceGenerator;
using ValveKeyValue;

namespace HedgeModManager.UI.Controls.Mods;

[PseudoClasses(":enabled")]
public partial class ModEntry : ButtonUserControl
{

    public ModEntry()
    {
        InitializeComponent();
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        Click += OnClick;

        if (DataContext is ModEntryViewModel viewModel)
            viewModel.UpdateSearch();
    }

    public void OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ModEntryViewModel viewModel)
            viewModel.ModEnabled = !viewModel.ModEnabled;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // What did I just write?
        if (change.Property == IsPointerOverProperty)
        {
            base.OnPropertyChanged(change);
            return;
        }
        base.OnPropertyChanged(change);
    }
}