using Avalonia;
using HedgeModManager.UI.CLI;
using Microsoft.Win32;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Runtime.Versioning;

namespace HedgeModManager.UI;

[Guid("DC33288B-818F-43AF-927C-75AA9A2FB42D")]
public sealed class Program
{
    public static readonly string ApplicationCompany = "NeverFinishAnything";
    public static readonly string ApplicationName = "HedgeModManager";
    public static readonly string ApplicationFriendlyName = "Hedge Mod Manager";
    public static readonly string ApplicationID = "HedgeModManager.UI";

    public static readonly string GitHubRepoOwner = "TheSuperSonic16";
    public static readonly string GitHubRepoName = "HedgeModManager";
    public static string UserAgent = $"Mozilla/5.0 (compatible; {ApplicationName}/{GetAppVersion()})";

    // Will become the GUID if exists
    public static string PipeName = $"{ApplicationCompany}\\{ApplicationName}";
    public static string InstallLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    public static string? FlatpakID = null;
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

        Network.UserAgent = UserAgent;
        Network.Initialize();

        if (Environment.GetEnvironmentVariable("FLATPAK_ID") is string flatpakID)
            FlatpakID = flatpakID;

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
            foreach (string schema in GameBanana.GameIDMapping.Keys)
                installToRegistery(schema, "-gb \"%1\"");
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

    public static string GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version is null)
            return "Unknown";
        return $"{version.Major}.{version.Minor}-{version.Revision}";
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
