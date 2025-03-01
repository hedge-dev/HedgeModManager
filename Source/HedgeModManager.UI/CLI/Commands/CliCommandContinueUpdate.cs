using HedgeModManager.UI.Controls.Modals;
using HedgeModManager.UI.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HedgeModManager.UI.CLI.Commands;

[CliCommand("continue-update", null, [typeof(int), typeof(int), typeof(string)], "Performs the manager update step", "")]
public class CliCommandContinueUpdate : ICliCommand
{
    public int ProcessID = 0;
    public int Stage = 0;
    public string TargetDir = string.Empty;

    public bool Execute(List<CommandLine.Command> commands, CommandLine.Command command)
    {
        ProcessID = (int)command.Inputs[0];
        Stage = (int)command.Inputs[1];
        TargetDir = (string)command.Inputs[2];

        try
        {
            var process = Process.GetProcessById(ProcessID);
            process?.WaitForExit(TimeSpan.FromMinutes(2));
            // Wait for any debuggers to close
            Thread.Sleep(1000);
        }
        catch { }

        switch (Stage)
        {
            case 0: // Copy files

                bool isSingleFile = File.Exists(Path.Combine(Program.InstallLocation, "update.exe"));
                var updateDir = Program.InstallLocation;

                string exePath = Path.Combine(TargetDir, "HedgeModManager.exe");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    exePath = Path.Combine(TargetDir, "HedgeModManager.UI");

                if (isSingleFile)
                {
                    string packagePath = Path.Combine(updateDir, "update.exe");
                    File.Copy(packagePath, exePath, true);
                }
                else
                {
                    if (Directory.Exists(TargetDir))
                        Directory.Delete(TargetDir, true);
                    foreach (var subDir in Directory.GetDirectories(updateDir, "*", SearchOption.AllDirectories))
                    {
                        var targetSubDir = TargetDir + subDir[updateDir.Length..];
                        Directory.CreateDirectory(targetSubDir);
                        foreach (var file in Directory.GetFiles(subDir))
                            File.Copy(file, Path.Combine(targetSubDir, Path.GetFileName(file)));
                    }
                    // Root files
                    foreach (var file in Directory.GetFiles(updateDir))
                        File.Copy(file, Path.Combine(TargetDir, Path.GetFileName(file)));
                }
                string args = "--continue-update";
                args += $" {Process.GetCurrentProcess().Id}";
                args += $" 1";
                args += $" \"{updateDir}\"";
                Process.Start(new ProcessStartInfo(exePath, args) { UseShellExecute = true }); Environment.Exit(0);

                Environment.Exit(0);
                break;
            case 1: // Clean up
                if (Directory.Exists(TargetDir))
                    Directory.Delete(TargetDir, true);
                break;
        }
        return true;
    }

    public Task ExecuteUI(MainWindowViewModel mainWindowViewModel) => Task.CompletedTask;
}
