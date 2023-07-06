namespace HedgeModManager.Foundation;

public class UpdateInfo
{
    public string Version { get; set; } = string.Empty;
    public string Changelog { get; set; } = string.Empty;

    public UpdateInfo()
    {

    }

    public UpdateInfo(string version)
    {
        Version = version;
    }

    public UpdateInfo(string version, string changelog)
    {
        Version = version;
        Changelog = changelog;
    }
}