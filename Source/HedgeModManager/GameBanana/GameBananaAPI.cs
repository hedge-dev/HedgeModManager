namespace HedgeModManager.GameBanana;

public class GameBananaAPI
{
    public const string EndpointURL = "https://gamebanana.com/apiv11";

    public static Dictionary<string, string> GameIDMappings = new()
    {
        { "6059" , "SonicGenerations" },
        { "6160" , "SonicForces" },
        { "6093" , "SonicLostWorld" },
        { "8707" , "PuyoPuyoTetris2" },
        { "11375", "SonicColorsUltimate" },
        { "15780", "SonicOrigins" },
        { "15779", "SonicFrontiers" },
        { "19886", "ShadowGenerations" },
        { "6559" , "UnleashedRecompiled" },
        { "21975", "UnleashedRecompiled" },
    };

    public static string BuildPath(params string[] paths)
    {
        return $"{EndpointURL}/{string.Join("/", paths)}";
    }

    public static async Task<string[]?> FetchRemoteInstallQueue(string memberID, string secretKey, string appID)
    {
        return await Network.Get<string[]>(BuildPath("RemoteInstall", memberID, secretKey, appID), null, false);
    }
}
