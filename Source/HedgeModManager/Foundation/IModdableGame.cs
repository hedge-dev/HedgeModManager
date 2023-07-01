namespace HedgeModManager.Foundation;

public interface IModdableGame : IGame
{
    public IModLoaderConfiguration ModLoaderConfiguration { get; }
    public IModDatabase ModDatabase { get; }
    public string ModLoaderName { get; }
    
    public Task InitializeAsync();
    public Task<bool> InstallModLoaderAsync();
    public bool IsModLoaderInstalled();
}

public interface IModdableGameTDatabase<out TDatabase> : IModdableGame where TDatabase : IModDatabase
{
    public new TDatabase ModDatabase { get; }

    IModDatabase IModdableGame.ModDatabase => ModDatabase;
}

public interface IModdableGameTConfiguration<out TConfig> : IModdableGame where TConfig : IModLoaderConfiguration
{
    public new TConfig ModLoaderConfiguration { get; }

    IModLoaderConfiguration IModdableGame.ModLoaderConfiguration => ModLoaderConfiguration;
}