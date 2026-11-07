using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using System.Collections.ObjectModel;
using System.Text;
using static HedgeModManager.UI.UILogger;

namespace HedgeModManager.UI;

public partial class UILogger : ObservableObject, ILogger
{
    [ObservableProperty] private ObservableCollection<Log> _logs = [];
    [ObservableProperty] private ObservableCollection<Log> _filteredLogs = [];
    private List<LogType> LogFilters = [LogType.Information, LogType.Warning, LogType.Error, LogType.Debug];
    private List<Log> _backupLogs = [];

    private List<string> _secrets = [];

    public UILogger()
    {
        Singleton<UILogger>.SetInstance(this);
    }

    public void AddSecret(string secret)
    {
        string secretStr = secret.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(secretStr) || _secrets.Contains(secretStr))
            return;
        _secrets.Add(secretStr);
    }

    public void WriteLine(LogType type, string message)
    {
        var log = new Log(type, message);
        _backupLogs.Add(log);
        Dispatcher.UIThread.Invoke(() =>
        {
            Logs.Add(new Log(type, message));
            if (LogFilters.Contains(type))
                FilteredLogs.Add(new Log(type, message));
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
        var secrets = GetInstance()._secrets;
        return GetInstance()._backupLogs
            .Select(log =>
                secrets.Aggregate(log.Message, (msg, secret) =>
                    msg.Replace(secret, new string('*', secret.Length), StringComparison.InvariantCultureIgnoreCase)))
            .Aggregate(new StringBuilder(), (sb, log) => sb.AppendLine(log.ToString()))
            .ToString();
    }

    public void FilterLogs(List<LogType> flags)
    {
        if (flags.Count < LogFilters.Count && flags.All(LogFilters.Contains))
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var logsToRemove = FilteredLogs.Where(log => !flags.Contains(log.Type)).ToList();
                logsToRemove.ForEach(log => FilteredLogs.Remove(log));
            });
        }
        else
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                FilteredLogs.Clear();
                Logs.ForEach(log =>
                {
                    if (flags.Contains(log.Type))
                        FilteredLogs.Add(log);
                });
            });
        }
        LogFilters = flags;
    }

    public class Log
    {
        public LogType Type { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; } = string.Empty;

        public Log()
        {
            DateTime = DateTime.UtcNow;
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
