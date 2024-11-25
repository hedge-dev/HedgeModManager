using Avalonia;
using HedgeModManager.UI.CLI;
using System;
using System.IO.Pipes;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.Json;
using System.Collections.Generic;

namespace HedgeModManager.UI;

[Guid("DC33288B-818F-43AF-927C-75AA9A2FB42D")]
public sealed class Program
{
    public static readonly string ApplicationCompany = "NeverFinishAnything";
    public static readonly string ApplicationName = "HedgeModManager";

    // Will become the GUID if exists
    public static string PipeName = $"{ApplicationCompany}\\{ApplicationName}";

    private static Mutex? CurrentMutex = null;
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

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
