using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.CLI.Commands;

[CliCommand("help", "h", null, "Lists all commands", "--help OR -h")]
public class CliCommandHelp : ICliCommand
{
    public bool Execute(List<CommandLine.Command> commands, CommandLine.Command command)
    {
        CommandLine.ShowHelp();
        return false;
    }

    public Task ExecuteUI(MainWindowViewModel mainWindowViewModel) => Task.CompletedTask;
}
