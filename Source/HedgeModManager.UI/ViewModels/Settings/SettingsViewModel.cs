using Avalonia.Controls;
using Avalonia.Controls.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Controls.Settings;
using System.ComponentModel;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.ViewModels.Settings;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private MainWindowViewModel? _mainViewModel;
    [ObservableProperty] private InlineCollection _title = [];

    public void UpdateTitle(StackPanel panel)
    {
        var runs = new InlineCollection();
        foreach (var child in panel.Children)
        {
            if (child is not SettingsBase settings)
                continue;
            if (runs.Count > 0)
                runs.Add(new Run() { Text = " > " });
            runs.Add(new Run() { Text = Localize(settings.Title),  });
        }
        Title = runs;
    }
}
