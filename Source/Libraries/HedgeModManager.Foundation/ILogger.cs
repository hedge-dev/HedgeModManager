namespace HedgeModManager.Foundation;

public interface ILogger
{
    public void WriteLine(LogType type, string message);
}
