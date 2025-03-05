using HedgeModManager.Foundation;

namespace HedgeModManager;

public class ModDependencyReport(IMod rootMod)
{
    public IMod RootMod { get; init; } = rootMod;
    public List<ModDependency> Dependencies { get; init; } = [];
    public List<ModDependency> MissingDependencies { get; init; } = [];

    public static ModDependencyReport GenerateReport(IModDatabase database, IMod mod)
    {
        var report = new ModDependencyReport(mod);

        if (mod.Dependencies.Count == 0 || !database.Mods.Contains(mod))
        {
            return report;
        }

        foreach (var dependency in mod.Dependencies)
        {
            CollectDependencies(report.Dependencies, database, dependency);
        }

        report.MissingDependencies.AddRange(report.Dependencies.Where(x => !database.Mods.Any(xx => xx.ID == x.ID)));

        return report;
    }

    public static void CollectDependencies(List<ModDependency> dependencies, IModDatabase database, ModDependency modDependency)
    {
        // Check if the dependency is already in the list
        if (dependencies.Any(x => x.ID == modDependency.ID))
        {
            return;
        }
        dependencies.Add(modDependency);

        var mod = database.Mods.FirstOrDefault(x => x.ID == modDependency.ID);

        if (mod != null)
        {
            foreach (var dep in mod.Dependencies)
            {
                // Dont search already searched dependencies
                if (dependencies.Any(x => x.ID == dep.ID))
                {
                    continue;
                }
                CollectDependencies(dependencies, database, dep);
            }
        }
    }
}
