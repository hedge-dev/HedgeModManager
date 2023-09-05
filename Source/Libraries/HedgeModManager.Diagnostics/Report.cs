namespace HedgeModManager.Diagnostics;
using ReportSet = List<ReportBlock>;

public class Report
{
    public Dictionary<string, ReportSet> Blocks { get; set; } = new();

    public bool HasInformation => Blocks.Any(x => x.Value.Any(b => b.Severity == Severity.Information));
    public bool HasWarnings => Blocks.Any(x => x.Value.Any(b => b.Severity == Severity.Warning));
    public bool HasErrors => Blocks.Any(x => x.Value.Any(b => b.Severity == Severity.Error));

    public ReportSet EnsureSet(string name)
    {
        if (Blocks.TryGetValue(name, out var set))
        {
            return set;
        }

        set = new ReportSet();
        Blocks.Add(name, set);
        return set;
    }

    public void Information(string name, string message) => EnsureSet(name).Add(ReportBlock.Information(message));
    public void Warning(string name, string message) => EnsureSet(name).Add(ReportBlock.Warning(message));
    public void Error(string name, string message) => EnsureSet(name).Add(ReportBlock.Error(message));
}