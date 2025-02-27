using Avalonia;
using HedgeModManager.UI.CLI;
using Microsoft.Win32;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Runtime.Versioning;

#if !DEBUG
using System.Diagnostics;
using System.Text;
#endif

namespace HedgeModManager.UI;

[Guid("DC33288B-818F-43AF-927C-75AA9A2FB42D")]
public sealed class Program
{
    public static readonly string ApplicationCompany = "hedge-dev";
    public static readonly string ApplicationName = "HedgeModManager";
    public static readonly string ApplicationFriendlyName = "Hedge Mod Manager";
    public static readonly string ApplicationID = "HedgeModManager.UI";

    public static readonly string GitHubRepoOwner = "hedge-dev";
    public static readonly string GitHubRepoName = "HedgeModManager";
    public static string UserAgent = $"Mozilla/5.0 (compatible; {ApplicationName}/{ApplicationVersion})";

    // Will become the GUID if exists
    public static string PipeName = $"{ApplicationCompany}\\{ApplicationName}";
    public static string InstallLocation = Path.GetDirectoryName(AppContext.BaseDirectory)!;
    public static string? FlatpakID = null;
    public static string ApplicationVersion = GetFormattedAppVersion(); 
#if DEBUG
    public const bool IsDebugBuild = true;
#else
    public const bool IsDebugBuild = false;
#endif

    public static Mutex? CurrentMutex = null;
    public static List<ICliCommand> StartupCommands = [];

    public static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
#if !DEBUG
        // Save and display unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            try
            {
                StringBuilder sb = new();
                sb.AppendLine($"{ApplicationFriendlyName} Crash Log");
                sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();

                sb.AppendLine("-- Logger Dump Start --");
                try
                {
                    sb.AppendLine(UILogger.Export());
                } catch (Exception loggerException)
                {
                    sb.AppendLine("Failed to export UI Logger: " + loggerException);
                }
                sb.AppendLine("-- Logger Dump End --");
                sb.AppendLine();

                var thrownException = e.ExceptionObject as Exception;

                sb.AppendLine("Unhandled Exception:");
                sb.AppendLine(thrownException?.ToString() ?? "NULL EXCEPTION");
                sb.AppendLine();

                string text = sb.ToString();
                Console.WriteLine(text);

                string crashLogFileName = $"HMMCrash-{DateTime.Now:yyyyMMddHHmmss}.log";
                string crashLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), crashLogFileName);

                File.WriteAllText(crashLogPath, text);
                Console.WriteLine($"Crash log saved to {crashLogPath}");
                Process.Start(new ProcessStartInfo(crashLogPath) { UseShellExecute = true });
            } catch
            {
                Console.WriteLine("Exception thrown while creating crash log.");
            }
        };
#endif

        var guidAttribute = typeof(Program).GetCustomAttribute<GuidAttribute>();
        if (guidAttribute == null)
            Console.WriteLine("Failed to get GUID");
        else
        {
            string mutexID = $"Global\\{guidAttribute?.Value}";
            PipeName = guidAttribute?.Value ?? PipeName;
            CurrentMutex = new Mutex(true, mutexID, out bool createdNew);

            if (!createdNew)
            {
                // Focus the existing instance if no arguments are passed
                if (args.Length == 0)
                    args = [ "--focus" ];

                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(3000);
                using var writer = new StreamWriter(client);
                writer.Write(JsonSerializer.Serialize(args));
                writer.Flush();
                Console.WriteLine("Message sent");
                CurrentMutex?.Dispose();
                return;
            }
        }

        Paths.CreateDirectories();
        Network.UserAgent = UserAgent;
        Network.Initialize();

        if (Environment.GetEnvironmentVariable("FLATPAK_ID") is string flatpakID)
            FlatpakID = flatpakID;

        // GB workaround
        // Checks if the first argument is "hedgemm", not "hedgemm:". If it is,
        //  it will add "-gb" to the arguments as it is assumed that a GB URI was passed.
        var editedArgs = new List<string>(args);
        if (editedArgs.Count > 0 && 
            editedArgs[0].StartsWith("hedgemm"))
            editedArgs.Insert(0, "--schema");

        var arguments = CommandLine.ParseArguments(args);
        var (continueStartup, commands) = CommandLine.ExecuteArguments(arguments);
        StartupCommands.AddRange(commands);
        if (continueStartup)
        {
            // Start Avalonia
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        CurrentMutex?.ReleaseMutex();
        CurrentMutex?.Dispose();
    }

    public static void InstallURIHandler()
    {
        if (OperatingSystem.IsWindows())
        {
            installToRegistery("hedgemm", "--schema \"%1\"");
            foreach (string schema in GameBanana.GameIDMappings.Keys)
                installToRegistery(schema, "--schema \"%1\"");
        }

        [SupportedOSPlatform("windows")]
        static void installToRegistery(string schema, string args)
        {
            if (Environment.ProcessPath is not string processPath)
                return;

            try
            {
                var reg = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{schema}");
                reg.SetValue("", $"URL:{ApplicationFriendlyName}");
                reg.SetValue("URL Protocol", "");
                reg = reg.CreateSubKey("shell\\open\\command");
                reg.SetValue("", $"\"{processPath}\" {args}");
                reg.Close();
            }
            catch { }
        }
    }

    public static string GetFormattedAppVersion()
    {
        string unstableType = "beta";
#if AUTOBUILD
        unstableType = "dev";
#endif

        var version = Assembly.GetExecutingAssembly().GetName().Version ?? Version.Parse("0");
        if (version.Revision == 0)
            return $"{version.Major}.{version.Minor}.{version.Build}";
        return $"{version.Major}.{version.Minor}.{version.Build} {unstableType} {version.Revision}";
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
