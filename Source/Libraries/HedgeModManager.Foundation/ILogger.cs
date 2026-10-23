namespace HedgeModManager.Foundation;

public interface ILogger
{
    public void AddSecret(string secret);
    public void WriteLine(LogType type, string message);
}
