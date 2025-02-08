using HedgeModManager.Foundation;

namespace HedgeModManager;

public class Logger
{
    public ILogger logger;

    public Logger(ILogger logger)
    {
        Singleton<Logger>.SetInstance(this);
        this.logger = logger;
    }

    public void WriteLine(LogType type, string message) => logger.WriteLine(type, message);

    public static Logger GetInstance()
    {
        return Singleton<Logger>.GetInstance();
    }

    public static void Information(string message)
    {
        GetInstance()?.WriteLine(LogType.Information, message);
    }

    public static void Warning(string message)
    {
        GetInstance()?.WriteLine(LogType.Warning, message);
    }

    public static void Error(string message)
    {
        GetInstance()?.WriteLine(LogType.Error, message);
    }

    public static void Debug(string message)
    {
        GetInstance()?.WriteLine(LogType.Debug, message);
    }

    public static void Error(Exception exception)
    {
        var instance = GetInstance();
        if (instance == null)
            return;

        string[] lines = exception.ToString().Replace("\r", "").Split('\n');
        foreach (var line in lines)
            instance.WriteLine(LogType.Error, line);
    }

}
