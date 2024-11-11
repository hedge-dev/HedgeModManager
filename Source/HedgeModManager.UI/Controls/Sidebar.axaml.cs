using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HedgeModManager.UI.Controls;

public partial class Sidebar : UserControl
{

    public static readonly StyledProperty<int> SelectedTabIndexProperty =
        AvaloniaProperty.Register<Sidebar, int>(
            nameof(SelectedTabIndex),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsSetupCompletedProperty =
    AvaloniaProperty.Register<Sidebar, bool>(
        nameof(IsSetupCompleted),
        defaultBindingMode: BindingMode.TwoWay);

    public int SelectedTabIndex
    {
        get => GetValue(SelectedTabIndexProperty);
        set => SetValue(SelectedTabIndexProperty, value);
    }

    public bool IsSetupCompleted
    {
        get => GetValue(IsSetupCompletedProperty);
        set => SetValue(IsSetupCompletedProperty, value);
    }

    public Sidebar()
    {
        InitializeComponent();
    }

    public void UpdateButtons(List<Control> buttons, bool checkVisible = true)
    {
        // Update button layout
        var enabledButtons = buttons.Where(x => x.IsVisible).ToList();
        for (int i = 0; i < enabledButtons.Count; ++i)
        {
            if (enabledButtons[i] is SidebarButton button)
            {
                //button.Order = i switch
                //{
                //    0 => SidebarButton.ButtonOrder.Top,
                //    _ when i == enabledButtons.Count - 1 => SidebarButton.ButtonOrder.Bottom,
                //    _ => SidebarButton.ButtonOrder.Middle
                //};
                button.Order = SidebarButton.ButtonOrder.Normal;
                button.IsSelected = SelectedTabIndex == buttons.IndexOf(button);
            }
        };
    }

    private void OnTabChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control)
            SelectedTabIndex = TabButtons.Children.IndexOf(control);
        UpdateButtons(TabButtons.Children.ToList());
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateButtons(TabButtons.Children.ToList());
    }

    private void OnGameIconPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var viewModel = (DataContext as MainWindowViewModel)!;
        viewModel.Modals.Add(new Modal(new GameSelectModal()));
    }

    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.Save();
    }

    private async void OnRunClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SaveAndRun();
        }
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedTabIndexProperty || change.Property == IsSetupCompletedProperty)
            UpdateButtons(TabButtons.Children.ToList());
    }
}