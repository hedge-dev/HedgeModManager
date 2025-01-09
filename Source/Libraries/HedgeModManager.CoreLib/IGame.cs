namespace HedgeModManager.Foundation;

public interface IGame
{
    public string Platform { get; }
    public string ID { get; }
    public string Name { get; }
    public string Root { get; }
    public string? Executable { get; }
    public string NativeOS { get; }
    public bool SupportsDirectLaunch { get; }
    public bool SupportsLauncher { get; }

    public Task Run(string? launchArgs, bool useLauncher);
}