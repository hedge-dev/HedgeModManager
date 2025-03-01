namespace HedgeModManager.Epic;
using Foundation;
using System.Diagnostics;
using System.Threading.Tasks;

public class EpicGame : IGame
{
    public string Platform => "Epic";
    public string ID { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Root { get; init; } = string.Empty;
    public string? Executable { get; init; }
    public string NativeOS { get; set; } = "Windows";
    public string? PrefixRoot { get; init; }
    public bool SupportsDirectLaunch => false; // TODO: Check Windows
    public bool SupportsLauncher => true;

    public Task Run(string? launchArgs, bool useLauncher)
    {
        if (useLauncher)
        {
            Process.Start(new ProcessStartInfo
            {
                //FileName = $"heroic://launch?appName={ID}&runner=legendary",
                FileName = $"heroic://launch/legendary/{ID}",
                UseShellExecute = true
            });
        }
        return Task.CompletedTask;
    }
}