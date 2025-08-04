using Avalonia.Data;
using Avalonia.Data.Converters;
using HedgeModManager.Foundation;
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

public class EmptyBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool val = true;
        if (value is string str)
            val = string.IsNullOrEmpty(str);
        if (value is int i)
            val = i == 0;

        return parameter as bool? == true ? !val : val;
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
    public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value?.ToString() is string str)
        {
            if (parameter?.ToString() is string param)
                return Localize(param, str);
            return Localize(str);
        }
        return BindingNotification.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingNotification.UnsetValue;
    }
}

public class StringLocalizeIfNotEmptyConverter : StringLocalizeConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString()))
            return BindingNotification.UnsetValue;
        return base.Convert(value, targetType, parameter, culture);
    }
}