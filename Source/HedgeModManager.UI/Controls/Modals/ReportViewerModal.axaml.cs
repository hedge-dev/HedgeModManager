using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HedgeModManager.Diagnostics;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ReportViewerModal : WindowModal
{
    public ReportViewerViewModel ViewModel { get; set; } = new();

    public ReportViewerModal()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public ReportViewerModal(Report report)
    {
        AvaloniaXamlLoader.Load(this);
        ViewModel.CurrentReport = report;
    }

    public void Open(MainWindowViewModel viewModel, string title)
    {
        Title = title;
        Open(viewModel);
        ViewModel.Update();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCopyAllClick(object? sender, RoutedEventArgs e)
    {
        string reportText = ViewModel.FullReportText;
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.Clipboard?.SetTextAsync(reportText);
    }
}