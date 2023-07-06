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