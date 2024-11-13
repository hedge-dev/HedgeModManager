namespace HedgeModManager;
using Foundation;

public class ModdableGameGeneric : IModdableGameTDatabase<ModDatabaseGeneric>, IModdableGameTConfiguration<ModLoaderConfiguration>
{
    public IGame BaseGame { get; }

    public string Platform => BaseGame.Platform;
    public string ID => BaseGame.ID;
    public string Name { get; set; }
    public string Root { get; set; }
    public string? Executable { get; set; }
    public string ModLoaderName { get; init; } = "Unknown";
    public string? ModLoaderFileName { get; init; }
    public string? ModLoaderDownloadURL { get; init; }
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
        if (string.IsNullOrEmpty(ModLoaderFileName))
            return false;

        // Install .NET runtime to prefix
        if (OperatingSystem.IsLinux() && NativeOS == "Windows")
        {
            Logger.Information("Setting up prefix...");
            string? prefix = LinuxCompatibility.GetPrefix(this);
            Logger.Debug($"Prefix: {prefix}");
            if (LinuxCompatibility.IsPrefixValid(prefix))
            {                
                await Task.Run(() => LinuxCompatibility.InstallRuntimeToPrefix(prefix));
                await LinuxCompatibility.AddDllOverride(prefix, ModLoaderFileName.Replace(".dll", ""));
            }
            else
            {
                Logger.Error("Prefix missing! Please run the game atleast once.");
                // Abort
                return false;
            }
        }

        // Download mod loader
        if (!string.IsNullOrEmpty(ModLoaderDownloadURL))
        {
            Logger.Information("Downloading mod loader...");
            string modLoaderPath = Path.Combine(Root, ModLoaderFileName);
            var client = new HttpClient();
            var response = await client.GetAsync(ModLoaderDownloadURL);
            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(modLoaderPath);
                await stream.CopyToAsync(fileStream);
                Logger.Information("Completed mod loader installation");
                return true;
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