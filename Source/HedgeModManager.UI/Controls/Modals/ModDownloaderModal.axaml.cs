using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ModDownloaderModal : UserControl
{
    public ModDownloaderViewModel ViewModel { get; set; }

    // Preview only
    public ModDownloaderModal()
    {
        ViewModel = new(async () =>
        {
            await Task.Delay(5000);
            return new()
            {
                Name = "Test Mod"
            };
        });

        InitializeComponent();
    }

    public ModDownloaderModal(Func<Task<ModDownloadInfo?>> downloadInfoCallback)
    {
        ViewModel = new(downloadInfoCallback);

        InitializeComponent();
    }

    public void Open(MainWindowViewModel viewModel)
    {
        viewModel.Modals.Add(new Modal(this, new Thickness(0)));
    }

    public void Close()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var modalInstance = viewModel.Modals.FirstOrDefault(x => x.Control == this);
            if (modalInstance != null)
                viewModel.Modals.Remove(modalInstance);
        }
    }

    private async void OnInitialized(object? sender, EventArgs e)
    {
        await ViewModel.StartInfoDownload();
    }

    private void OnDownloadClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Ready = false;
        if (DataContext is MainWindowViewModel viewModel)
            MessageBoxModal.CreateOK("TODO", "Download mod").Open(viewModel);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}