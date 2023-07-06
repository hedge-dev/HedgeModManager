namespace HedgeModManager.Foundation;

public interface IGameLocator
{
    IReadOnlyList<IGame> Locate();
}