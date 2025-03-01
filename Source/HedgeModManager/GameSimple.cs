using HedgeModManager.Foundation;
using System.Diagnostics;

namespace HedgeModManager;

public class GameSimple(string platform, string id, string name, string root, string? executable, string nativeOS, string launchCommand, string launchCommandArgs) : IGame
{
    public string Platform { get; set; } = platform;
    public string ID { get; set; } = id;
    public string Name { get; set; } = name;
    public string Root { get; set; } = root;
    public string? Executable { get; set; } = executable;
    public string NativeOS { get; set; } = nativeOS;
    public string? PrefixRoot { get; set; } = null;
    public string LaunchCommand { get; set; } = launchCommand;
    public string LaunchCommandArgs { get; set; } = launchCommandArgs;
    public bool SupportsDirectLaunch { get; set; }
    public bool SupportsLauncher { get; set; }

    public Task Run(string? launchArgs, bool useLauncher)
    {
        launchArgs ??= LaunchCommandArgs;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = LaunchCommand,
            Arguments = launchArgs ?? string.Empty,
            WorkingDirectory = Root,
            UseShellExecute = true
        });

        return Task.CompletedTask;
    }
}
