namespace HedgeModManager.CoreLib;
using Foundation;

public interface IModDatabase
{
    IReadOnlyList<IMod> Mods { get; }
    IReadOnlyList<ICode> Codes { get; }

    Task Save();
}