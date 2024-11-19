namespace HedgeModManager;
using Foundation;
using HedgeModManager.IO;
using System.IO.Compression;

public class ModdableGameGeneric : IModdableGameTDatabase<ModDatabaseGeneric>, IModdableGameTConfiguration<ModLoaderConfiguration>
{
    public IGame BaseGame { get; }

    public string Platform => BaseGame.Platform;
    public string ID => BaseGame.ID;
    public string Name { get; set; }
    public string Root { get; set; }
    public string? Executable { get; set; }
    public string ModLoaderName { get; init; } = "None";
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
                await LinuxCompatibility.InstallRuntimeToPrefix(prefix);
                Logger.Information("Applying patches to prefix...");
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
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Failed to download mod loader. Code: {response.StatusCode}");
                return false;
            }

            if (ModLoaderDownloadURL.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var outputStream = new MemoryStream();
                using var inputStream = await response.Content.ReadAsStreamAsync();
                await inputStream.CopyToAsync(outputStream);
                Logger.Debug($"Downloaded {outputStream.Position} bytes");
                outputStream.Position = 0;
                using var archive = new ZipArchive(outputStream, ZipArchiveMode.Read);
                Logger.Debug("Opened zip");

                foreach (var entry in archive.Entries)
                {
                    string destinationPath = Path.Combine(Root, entry.FullName);

                    if (entry.FullName.EndsWith('/'))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    // Rename mod loader file
                    if (entry.FullName.EndsWith($"{ModLoaderName}.dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Logger.Debug($"Renamed {PathEx.GetFileName(entry.FullName)} to {ModLoaderFileName}");
                        destinationPath = Path.Combine(Root, ModLoaderFileName);
                    }

                    // Try delete first
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }

                    entry.ExtractToFile(destinationPath, true);
                }
                Logger.Debug("Extracted zip");
            }
            else
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(modLoaderPath);
                await stream.CopyToAsync(fileStream);
            }

            Logger.Information("Completed mod loader installation");
            return true;
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