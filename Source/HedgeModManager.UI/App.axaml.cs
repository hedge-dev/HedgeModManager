using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using HedgeModManager.UI.Languages;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HedgeModManager.UI;

public partial class App : Application
{
    public Window? MainWindow { get; set; }

    public static T? GetStyleResource<T>(string key, StyledElement element)
    {
        element.TryFindResource(key, element.ActualThemeVariant, out object? resource);
        return resource is T result ? result : default;
    }

    public static T? GetResource<T>(string key)
    {
        object? resource = null;
        if (Current is App app)
            app.TryFindResource(key, app.ActualThemeVariant, out resource);
        return resource is T result ? result : default;
    }

    public static T? GetWindowResource<T>(string key)
    {
        object? resource = null;
        if (Current is App app && app.MainWindow is Window mainWindow)
            mainWindow.TryFindResource(key, mainWindow.ActualThemeVariant, out resource);
        return resource is T result ? result : default;
    }

    public static LanguageEntry GetClosestLanguage(ICollection<LanguageEntry> languages, string languageCode)
    {
        var entry = languages.FirstOrDefault(t => t.Code == languageCode);
        if (entry != null)
            return entry;

        string languageMain = languageCode.Split('-')[0];
        entry = languages.FirstOrDefault(t => t.Code.Split('-')[0] == languageMain);
        entry ??= languages.First();
        return entry;
    }

    public static List<LanguageEntry> GetLanguages()
    {
        return GetResource<List<LanguageEntry>>("Languages") ?? [];
    }

    public static void ChangeLanguage(LanguageEntry? language)
    {
        if (language == null || Current is not App app ||
            app.MainWindow is not MainWindow mainWindow)
            return;
        var resourcePath = new Uri($"{language.Code}.axaml", UriKind.Relative);

        if (mainWindow.Resources.MergedDictionaries.Count > 0)
            mainWindow.Resources.MergedDictionaries.RemoveAt(0);
        if (language.Code != "en-AU")
        {
            var resInclude = new MergeResourceInclude(new Uri("avares://HedgeModManager.UI/Languages/"))
            {
                Source = resourcePath
            };
            mainWindow.Resources.MergedDictionaries.Add(resInclude);
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            var languages = GetLanguages();
            var viewModel = new MainWindowViewModel(new UILogger(), languages);
            
            MainWindow = desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };

            Logger.Information($"Loading config...");
            viewModel.Config.Load();
            RequestedThemeVariant = Themes.Themes.GetTheme(viewModel.Config.Theme);

            ChangeLanguage(viewModel.SelectedLanguage =
            languages.FirstOrDefault(x => x.Code == viewModel.Config.Language));
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static string GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version is null)
            return "Unknown";
        return $"{version.Major}.{version.Minor}-{version.Revision}";
    }
}