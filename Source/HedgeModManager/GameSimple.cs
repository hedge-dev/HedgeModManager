using HedgeModManager.Foundation;
using System.Diagnostics;

namespace HedgeModManager;

public class GameSimple(string platform, string id, string name, string root, string? executable, string nativeOS, string? launchCommandDirect, string? launchCommandLauncher, string? launchRoot) : IGame
{
    public string Platform { get; set; } = platform;
    public string ID { get; set; } = id;
    public string Name { get; set; } = name;
    public string Root { get; set; } = root;
    public string? Executable { get; set; } = executable;
    public string NativeOS { get; set; } = nativeOS;
    public string? PrefixRoot { get; set; } = null;
    public bool SupportsDirectLaunch => !string.IsNullOrEmpty(LaunchCommandDirect);
    public bool SupportsLauncher => !string.IsNullOrEmpty(LaunchCommandLauncher);
    public string? LaunchCommandDirect { get; set; } = launchCommandDirect;
    public string? LaunchCommandLauncher { get; set; } = launchCommandLauncher;
    public string? LaunchRoot { get; set; } = launchRoot;

    public Task Run(string? launchArgs, bool useLauncher)
    {
        string? command = LaunchCommandDirect;
        if (useLauncher || string.IsNullOrEmpty(command))
            command = LaunchCommandLauncher ?? command;

        // No valid commands
        if (string.IsNullOrEmpty(command))
            return Task.CompletedTask;

        (string executable, string? arguments) = ConvertCommandLine(command);

        Process.Start(new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments ?? launchArgs ?? string.Empty,
            WorkingDirectory = LaunchRoot ?? Root,
            UseShellExecute = true
        });

        return Task.CompletedTask;
    }

    public static (string, string?) ConvertCommandLine(string command)
    {
        string replacedLine = command.Replace("\\ ", "\x01");
        if (replacedLine.Contains(' '))
        {
            string executable = replacedLine[..replacedLine.IndexOf(' ')].Replace("\x01", " ");
            string arguments = replacedLine[(replacedLine.IndexOf(' ') + 1)..].Replace("\x01", " ");
            return (executable, arguments);
        }
        return (command.Replace("\\ ", " "), null);
    }
}
