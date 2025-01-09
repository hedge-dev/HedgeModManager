namespace HedgeModManager.Steam;
using Foundation;
using System.Diagnostics;
using System.Threading.Tasks;

public class SteamGame : IGame
{
    public string Platform => "Steam";
    public string ID { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Root { get; init; } = string.Empty;
    public string? Executable { get; init; }
    public string NativeOS { get; } = "Windows";
    public bool SupportsDirectLaunch => OperatingSystem.IsWindows();
    public bool SupportsLauncher => true;

    public Task Run(string? launchArgs, bool useLauncher)
    {
        if (!useLauncher && Executable != null)
        {
            Process.Start(new ProcessStartInfo(Executable)
            {
                WorkingDirectory = Path.GetDirectoryName(Executable),
                Arguments = launchArgs
            });
        } else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = string.IsNullOrEmpty(launchArgs) ?
                    $"steam://run/{ID}" : $"steam://run/{ID}//{launchArgs}",
                UseShellExecute = true
            });
        }
        return Task.CompletedTask;
    }
}