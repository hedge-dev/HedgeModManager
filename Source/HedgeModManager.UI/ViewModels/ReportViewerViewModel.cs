using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Diagnostics;
using HedgeModManager.Foundation;
using System.Collections.ObjectModel;
using System.Text;

namespace HedgeModManager.UI.ViewModels;

public partial class ReportViewerViewModel : ViewModelBase
{
    public Report CurrentReport { get; set; } = new();
    public ObservableCollection<ReportMessage> Messages { get; set; } = [];

    [ObservableProperty] private string _fullReportText = string.Empty;

    public void Update()
    {
        StringBuilder sb = new();
        Messages.Clear();
        Logger.Information("Report view:");
        foreach (var block in CurrentReport.Blocks)
        {
            AddMessage(Severity.Information, "Block", block.Key, 0);
            Logger.Information($"  Block: {block.Key}");
            sb.AppendLine($"- {block.Key}");
            foreach (var message in block.Value)
            {
                AddMessage(message.Severity, message.Severity.ToString(), message.Message, 1);
                sb.AppendLine($"    [{message.Severity.ToString()[0]}] {message.Message}");
                switch (message.Severity)
                {
                    case Severity.Information:
                        Logger.Information($"    {message.Message}");
                        break;
                    case Severity.Warning:
                        Logger.Warning($"    {message.Message}");
                        break;
                    case Severity.Error:
                        Logger.Error($"    {message.Message}");
                        break;
                    default:
                        Logger.Debug($"    {message.Message}");
                        break;
                }
            }
        }
        FullReportText = sb.ToString();
    }
    public void AddMessage(Severity severity, string name, string message, int steps)
    {
        string colorKey = severity switch
        {
            Severity.Warning => "ReportViewer.WarningBrush",
            Severity.Error => "ReportViewer.ErrorBrush",
            _ => "ReportViewer.InformationBrush",
        };

        IBrush color = SolidColorBrush.Parse("#00000000");
        color = App.GetWindowResource<IBrush>(colorKey) ?? color;

        Messages.Add(new ReportMessage(name, message, color, new(steps * 16 + 4, 4, 4, 4)));
    }

    public class ReportMessage(string name, string message, IBrush brush, Thickness margin)
    {
        public string Name { get; set; } = name;
        public string Message { get; set; } = message;
        public IBrush Brush { get; set; } = brush;
        public Thickness Margin { get; set; } = margin;
    }
}
