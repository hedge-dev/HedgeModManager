using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using System.Collections.ObjectModel;
using System.Text;

namespace HedgeModManager.UI;

public partial class UILogger : ObservableObject, ILogger
{
    [ObservableProperty] private ObservableCollection<Log> _logs = [];
    private List<Log> _backupLogs = [];

    public UILogger()
    {
        Singleton<UILogger>.SetInstance(this);
    }

    public void WriteLine(LogType type, string message)
    {
        var log = new Log(type, message);
        _backupLogs.Add(log);
        Dispatcher.UIThread.Invoke(() =>
        {
            Logs.Add(new Log(type, message));
        });
    }

    public static UILogger GetInstance()
    {
        return Singleton<UILogger>.GetInstance();
    }

    public static void Clear()
    {
        GetInstance().Logs.Clear();
        GetInstance()._backupLogs.Clear();
    }

    public static string Export()
    {
        return GetInstance()._backupLogs
            .Aggregate(new StringBuilder(), (sb, log) => sb.AppendLine(log.ToString()))
            .ToString();
    }

    public class Log
    {
        public LogType Type { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;

        public Log()
        {
            DateTime = DateTime.Now;
        }

        public Log(LogType type, string message) : this()
        {
            Type = type;
            Message = message;
        }

        public override string ToString()
        {
            string logType = Type switch
            {
                LogType.Information => " INFO",
                LogType.Warning => " WARN",
                LogType.Error => "ERROR",
                LogType.Debug => "DEBUG",
                _ => " UNKN"
            };
            return $"[{DateTime:HH:mm:ss} - {logType}] {Message}";
        }
    }

}
