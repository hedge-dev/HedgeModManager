namespace HedgeModManager.Diagnostics;

public class ReportBlock
{
    public Severity Severity { get; set; }
    public string Message { get; set; } = string.Empty;

    public ReportBlock()
    {

    }

    public ReportBlock(Severity severity, string message)
    {
        Severity = severity;
        Message = message;
    }

    public static ReportBlock Information(string message) => new(Severity.Information, message);
    public static ReportBlock Warning(string message) => new(Severity.Warning, message);
    public static ReportBlock Error(string message) => new(Severity.Error, message);


    public override string ToString()
    {
        return $"{Severity} | {Message}";
    }
}