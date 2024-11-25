using HedgeModManager.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HedgeModManager.UI.CLI;

public interface ICliCommand
{
    /// <summary>
    /// Executes the command
    /// </summary>
    /// <param name="commands">All parsed commands</param>
    /// <param name="command">Current executing command</param>
    /// <returns>Should the application continue startup?</returns>
    bool Execute(List<CommandLine.Command> commands, CommandLine.Command command);

    /// <summary>
    /// Executes the command
    /// 
    /// Function is to be called on the UI thread
    /// </summary>
    Task ExecuteUI(MainWindowViewModel mainWindowViewModel);
}
