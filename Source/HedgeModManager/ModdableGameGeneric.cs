namespace HedgeModManager;
using Foundation;
using Text;

public class ModdableGameGeneric : IModdableGameTDatabase<ModDatabaseGeneric>, IModdableGameTConfiguration<ModLoaderConfiguration>
{
    public IGame BaseGame { get; }

    public string Platform => BaseGame.Platform;
    public string ID => BaseGame.ID;
    public string Name { get; set; }
    public string Root { get; set; }
    public string? Executable { get; set; }
    public string ModLoaderName { get; init; } = "Unknown";
    public string ModLoaderFileName { get; init; } = string.Empty;
    public string NativeOS { get; set; } = "Windows";
    public ModDatabaseGeneric ModDatabase { get; } = new ModDatabaseGeneric();
    public ModLoaderConfiguration ModLoaderConfiguration { get; set; } = new ModLoaderConfiguration();

    public ModdableGameGeneric(IGame game)
    {
        BaseGame = game;
        Name = game.Name;
        Root = game.Root;
        Executable = game.Executable;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // TODO: Change this
            await ModLoaderConfiguration.Load(Path.Combine(Root, "cpkredir.ini"));
        }
        catch
        {
            ModLoaderConfiguration.DatabasePath = Path.Combine(Root, "mods", ModDatabaseGeneric.DefaultDatabaseName);
        }

        ModDatabase.LoadDatabase(ModLoaderConfiguration.DatabasePath);
    }

    public async Task<bool> InstallModLoaderAsync()
    {
        if (OperatingSystem.IsLinux() && NativeOS == "Windows")
        {
            string? prefix = LinuxCompatibility.GetPrefix(this);
            if (LinuxCompatibility.IsPrefixValid(prefix))
            {                
                await Task.Run(() => LinuxCompatibility.InstallRuntimeToPrefix(prefix));
                await LinuxCompatibility.AddDllOverride(prefix, ModLoaderFileName.Replace(".dll", ""));
            }
        }
        return false;
    }

    public bool IsModLoaderInstalled()
    {
        if (string.IsNullOrEmpty(ModLoaderFileName))
        {
            return false;
        }

        return File.Exists(Path.Combine(Root, ModLoaderFileName));
    }

    public Task Run(string? launchArgs, bool useLauncher) =>
        BaseGame.Run(launchArgs, useLauncher);
}