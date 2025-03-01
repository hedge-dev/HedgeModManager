using System.Reflection;

namespace HedgeModManager.UI.CLI;

public class CommandLine
{
    public static Dictionary<CliCommandAttribute, Type> RegisteredCommands = [];

    static CommandLine()
    {
        RegisterCommands(Assembly.GetExecutingAssembly());
    }

    public static void RegisterCommands(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<CliCommandAttribute>() != null
            && typeof(ICliCommand).IsAssignableFrom(t));
        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<CliCommandAttribute>()!;
            RegisteredCommands[attribute] = type;
        }
    }

    public static void ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine($"HedgeModManager {Program.ApplicationVersion}\n");

        Console.WriteLine("Commands:");

        foreach (var command in RegisteredCommands)
        {
            if (command.Key.Description != null)
                Console.WriteLine("    --{0}: {1}", command.Key.Name, command.Key.Description.Replace("\n", "\n          "));
            else
                Console.WriteLine("    --{0}", command.Key.Name);
            if (command.Key.Usage != null)
                Console.WriteLine("        Usage: {0}", command.Key.Usage);
            if (command.Key.Example != null)
                Console.WriteLine("        Example: {0}", command.Key.Example);
        }
    }

    public static List<Command> ParseArguments(string[] args)
    {
        var commands = new List<Command>();
        
        for (int i = 0; i < args.Length; ++i)
        {
            // Check if it is a command
            if (args[i].StartsWith('-'))
            {
                var command = RegisteredCommands.FirstOrDefault(x => x.Key.Alias == args[i][1..]);
                if (args[i].StartsWith("--"))
                    command = RegisteredCommands.FirstOrDefault(x => x.Key.Name == args[i][2..]);
                if (command.Key == null)
                    continue;

                // Check input count
                if (i + command.Key.Inputs.Length > args.Length)
                {
                    Console.WriteLine("Too few inputs for --{0}", command.Key.Name);
                    continue;
                }

                // Read inputs
                var inputs = new List<object>();
                foreach (var input in command.Key.Inputs)
                {
                    i++;
                    // Break if new command is found
                    if (args[i].StartsWith('-'))
                    {
                        Console.WriteLine("Too few inputs for --{0}", command.Key.Name);
                        break;
                    }

                    var data = ReadTypeFromString(Type.GetTypeCode(input), args[i]);

                    if (data != null)
                        inputs.Add(data);
                    else
                        Console.WriteLine("Unknown type {0} for --{1}", input.Name, command.Key.Name);
                }
                commands.Add(new Command(command.Key, command.Value, inputs));
            }
            else
            {
                var lastCommand = commands.LastOrDefault();
                lastCommand?.Inputs.Add(args[i]);
            }
        }
        return commands;
    }

    public static object? ReadTypeFromString(TypeCode typeCode, string data)
    {
        return typeCode switch
        {
            TypeCode.String => data,
            TypeCode.Int32 => int.Parse(data),
            TypeCode.Boolean => bool.Parse(data),
            _ => null,
        };
    }

    public static (bool, List<ICliCommand>) ExecuteArguments(List<Command> commands)
    {
        bool continueStartup = true;
        var executedCommands = new List<ICliCommand>();
        foreach (var command in commands)
        {
            if (Activator.CreateInstance(command.Type) is ICliCommand cmd)
            {
                if (!cmd.Execute(commands, command))
                    continueStartup = false;
                executedCommands.Add(cmd);
            }
        }
        return (continueStartup, executedCommands);
    }

    public class Command(CliCommandAttribute cliCommandAttribute, Type type, List<object> inputs)
    {
        public CliCommandAttribute CommandAttribute = cliCommandAttribute;
        public Type Type = type;
        public List<object> Inputs = inputs;
    }
}
