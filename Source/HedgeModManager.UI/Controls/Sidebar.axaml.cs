using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls;

public partial class Sidebar : UserControl
{
    public SidebarViewModel ViewModel { get; set; } = new();

    public Sidebar()
    {
        AvaloniaXamlLoader.Load(this);
        ViewModel.SidebarElement = this;
    }

    private void OnTabChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is Control control)
            ViewModel.SelectedTabIndex = TabButtons.Children.IndexOf(control);
        ViewModel.UpdateButtons(TabButtons.Children.ToList());
    }
    private void OnSettingsClicked(object? sender, RoutedEventArgs e)
    {
        int oldTabIndex = ViewModel.SelectedTabIndex;
        if (sender is Control control)
        {
            ViewModel.SelectedTabIndex = TabButtons.Children.IndexOf(control);

            if (oldTabIndex == ViewModel.SelectedTabIndex)
            {
                var mainWindow = control.FindAncestorOfType<Views.MainWindow>();
                var settings = mainWindow.FindDescendantOfType<Settings.Settings>();
                settings?.SwitchPanel(null);
            }
        }
        ViewModel.UpdateButtons(TabButtons.Children.ToList());
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ViewModel.MainViewModel = DataContext as MainWindowViewModel;
        if (ViewModel.MainViewModel != null)
        {
            ViewModel.MainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SelectedTabIndex" ||
                    e.PropertyName == "IsSetupCompleted" ||
                    e.PropertyName == "SelectedGame")
                    ViewModel.Update();
            };
        }
        ViewModel.UpdateButtons(TabButtons.Children.ToList());
    }

    private void OnGameIconPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var viewModel = (DataContext as MainWindowViewModel)!;
        viewModel.Modals.Add(new Modal(new GameSelectModal()));
    }

    private void OnExitClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        var messageBox = new MessageBoxModal("Modal.Title.Confirm", "Modal.Message.ExitConfirm");
        messageBox.AddButton("Common.Button.Yes", (s, a) => viewModel.Close());
        messageBox.AddButton("Common.Button.No", (s, a) => messageBox.Close());
        messageBox.Open(viewModel);
    }

    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.SaveAsync();
    }

    private async void OnRunClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.SaveAndRunAsync();
    }
}