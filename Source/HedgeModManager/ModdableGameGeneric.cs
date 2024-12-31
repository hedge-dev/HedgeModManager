namespace HedgeModManager;
using Foundation;
using HedgeModManager.IO;
using HedgeModManager.Properties;
using System.IO.Compression;

public class ModdableGameGeneric : IModdableGameTDatabase<ModDatabaseGeneric>, IModdableGameTConfiguration<ModLoaderConfiguration>
{
    public IGame BaseGame { get; }

    public string Platform => BaseGame.Platform;
    public string ID => BaseGame.ID;
    public string Name { get; set; }
    public string Root { get; set; }
    public string? Executable { get; set; }
    public string DefaultDatabaseDirectory { get; set; } = "mods";
    public string ModLoaderName { get; init; } = "None";
    public string NativeOS { get; set; } = "Windows";
    public ModDatabaseGeneric ModDatabase { get; } = new ModDatabaseGeneric();
    public ModLoaderConfiguration ModLoaderConfiguration { get; set; } = new ModLoaderConfiguration();
    public ModLoaderGeneric? ModLoader { get; set; }

    public ModdableGameGeneric(IGame game)
    {
        BaseGame = game;
        Name = game.Name;
        Root = game.Root;
        Executable = game.Executable;
    }

    public async Task DownloadCodes(string? url)
    {
        url ??= Resources.CommunityCodesURL;
        if (url.EndsWith('/'))
            url += $"{Name}.hmm";

        string contents = await Network.Client.GetStringAsync(url);
        string modsRoot = PathEx.GetDirectoryName(ModLoaderConfiguration.DatabasePath).ToString();

        Directory.CreateDirectory(modsRoot);
        File.WriteAllText(Path.Combine(modsRoot, ModDatabaseGeneric.MainCodesFileName), contents);
    }

    public async Task InitializeAsync()
    {
        try
        {
            // TODO: Change this
            await ModLoaderConfiguration.Load(Path.Combine(Root, "cpkredir.ini"));
            string directory = PathEx.GetDirectoryName(ModLoaderConfiguration.DatabasePath).ToString();
            if (!Directory.Exists(directory))
            {
                ModLoaderConfiguration.DatabasePath = string.Empty;
            }
        }
        catch
        {
            ModLoaderConfiguration.DatabasePath = string.Empty;
        }

        if (string.IsNullOrEmpty(ModLoaderConfiguration.DatabasePath))
        {
            ModLoaderConfiguration.DatabasePath = Path.Combine(Root, DefaultDatabaseDirectory, ModDatabaseGeneric.DefaultDatabaseName);
        }

        ModDatabase.LoadDatabase(ModLoaderConfiguration.DatabasePath);
    }

    public async Task<bool> InstallModLoaderAsync()
    {
        if (ModLoader != null)
        {
            if (ModLoader.IsInstalled())
            {
                return await ModLoader.UninstallAsync();
            }
            else
            {
                return await ModLoader.InstallAsync();
            }
        }
        return true;
    }

    public bool IsModLoaderInstalled()
    {
        if (ModLoader != null)
        {
            return ModLoader.IsInstalled();
        }
        return true;
    }

    public Task Run(string? launchArgs, bool useLauncher) =>
        BaseGame.Run(launchArgs, useLauncher);
}