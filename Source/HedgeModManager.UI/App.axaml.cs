using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HedgeModManager.UI.ViewModels;
using HedgeModManager.UI.Views;
using System;
using System.Reflection;

namespace HedgeModManager.UI
{
    public partial class App : Application
    {
        public static readonly string ApplicationCompany = "NeverFinishAnything";
        public static readonly string ApplicationName = "HedgeModManager";

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

                var viewModel = new MainWindowViewModel(new UILogger());
                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel,
                };
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
}