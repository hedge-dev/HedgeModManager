using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace HedgeModManager.UI;

public partial class UILogger : ObservableObject, ILogger
{
    [ObservableProperty] private ObservableCollection<Log> _logs = new();

    public UILogger()
    {
        Singleton<UILogger>.SetInstance(this);
    }

    public void WriteLine(LogType type, string message)
    {
        Logs.Add(new Log(type, message));
    }

    public static UILogger GetInstance()
    {
        return Singleton<UILogger>.GetInstance();
    }

    public static void Clear()
    {
        GetInstance().Logs.Clear();
    }

    public static string Export()
    {
        return GetInstance().Logs
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
