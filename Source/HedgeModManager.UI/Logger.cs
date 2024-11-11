using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Diagnostics;
using HedgeModManager.Foundation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HedgeModManager.UI.Logger;

namespace HedgeModManager.UI
{
    public partial class Logger : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Log> _logs = new();

        public Logger()
        {
            Singleton<Logger>.SetInstance(this);
        }

        public void Add(LogType type, string message)
        {
            Logs.Add(new Log(type, message));
        }

        public static Logger GetInstance()
        {
            return Singleton<Logger>.GetInstance();
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

        public static void Information(string message)
        {
            GetInstance()?.Add(LogType.Information, message);
        }

        public static void Warning(string message)
        {
            GetInstance()?.Add(LogType.Warning, message);
        }

        public static void Error(string message)
        {
            GetInstance()?.Add(LogType.Error, message);
        }

        public static void Debug(string message)
        {
            GetInstance()?.Add(LogType.Debug, message);
        }

        public enum LogType
        {
            Information,
            Warning,
            Error,
            Debug
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
                    _ => "UNKNOWN"
                };
                return $"[{DateTime:HH:mm:ss} - {logType}] {Message}";
            }

        }

    }
}
