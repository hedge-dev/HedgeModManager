using Avalonia.Controls;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.CLI.Commands;

[CliCommand("fullscreen", null, null, "Starts in fullscreen", "--fullscreen")]
public class CliCommandFullscreen : ICliCommand
{
    public bool Execute(List<CommandLine.Command> commands, CommandLine.Command command)
    {
        return true;
    }

    public Task ExecuteUI(MainWindowViewModel mainWindowViewModel)
    {
        mainWindowViewModel.WindowState = WindowState.FullScreen;
        return Task.CompletedTask;
    }
}
