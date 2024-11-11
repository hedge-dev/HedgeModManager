using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeModManager.UI
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;

            return BindingNotification.UnsetValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;

            return BindingNotification.UnsetValue;
        }
    }

    public class NullBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null;
        }
    }

    public class LogStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Logger.Log log)
            {
                string logType = log.Type switch
                {
                    Logger.LogType.Information => " INFO",
                    Logger.LogType.Warning => " WARN",
                    Logger.LogType.Error => "ERROR",
                    Logger.LogType.Debug => "DEBUG",
                    _ => "UNKNOWN"
                };
                return $"[{log.DateTime:HH:mm:ss} - {logType}] {log.Message}";
            }
            return BindingNotification.UnsetValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingNotification.UnsetValue;
        }
    }
}
