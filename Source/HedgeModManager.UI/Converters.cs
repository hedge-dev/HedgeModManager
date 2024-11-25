using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI;

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
        if (value is UILogger.Log log)
        {
            string logType = log.Type switch
            {
                LogType.Information => " INFO",
                LogType.Warning => " WARN",
                LogType.Error => "ERROR",
                LogType.Debug => "DEBUG",
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
public class StringLocalizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
            return Localize(str);
        return BindingNotification.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingNotification.UnsetValue;
    }
}
