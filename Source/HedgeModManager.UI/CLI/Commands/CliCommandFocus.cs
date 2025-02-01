using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.CLI.Commands;

[CliCommand("focus", null, null, "Focuses the existing HMM window", "--focus")]
public class CliCommandFocus : ICliCommand
{
    public bool Execute(List<CommandLine.Command> commands, CommandLine.Command command) => true;

    public Task ExecuteUI(MainWindowViewModel mainWindowViewModel)
    {
        if (Application.Current?.ApplicationLifetime 
            is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is Window mainWindow)
        {
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;
        }

        return Task.CompletedTask;
    }
}
