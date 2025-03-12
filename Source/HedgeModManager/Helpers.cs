namespace HedgeModManager;

public static class Helpers
{
    public static bool IsFlatpak => Environment.GetEnvironmentVariable("FLATPAK_ID") != null;
}
