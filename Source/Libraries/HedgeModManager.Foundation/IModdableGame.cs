namespace HedgeModManager.CoreLib;
using Foundation;

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