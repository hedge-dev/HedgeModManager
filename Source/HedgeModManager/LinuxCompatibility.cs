namespace HedgeModManager;

using Foundation;
using Properties;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;

public class LinuxCompatibility
{
    /// <summary>
    /// Installs the .NET runtime to the prefix
    /// </summary>
    /// <param name="path">Path to the prefix root directory</param>
    public static async Task<bool> InstallRuntimeToPrefix(string? path)
    {
        Logger.Debug($"Installing .NET runtime to {path}");
        if (path == null)
        {
            return false;
        }

        // Download runtime
        Logger.Information($"Downloading .NET runtime");
        var stream = await Network.Download(Resources.DotnetDownloadURL, "dotnetFrameworkRuntime.zip", null);
        if (stream == null)
        {
            Logger.Error($"Failed to download runtime");
            return false;
        }

        Logger.Information($"Installing .NET runtime");
        string cDrive = Path.Combine(path, "drive_c");
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        Logger.Debug("Opened zip");

        await Task.Run(() =>
        {
            foreach (var entry in archive.Entries)
            {
                string destinationPath = Path.Combine(cDrive, entry.FullName);

                if (entry.FullName.EndsWith('/'))
                {
                    var fileinfo = new FileInfo(destinationPath);
                    if (fileinfo.Exists)
                    {
                        // Remove slash from the end
                        destinationPath = destinationPath[..^1];
                        if (fileinfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            Logger.Debug($"Symlink detected, unlinking \"{destinationPath}\"");
                            File.Delete(destinationPath);
                        }
                        else
                        {
                            Logger.Warning($"File with the same name as the directory exists!");
                        }
                    }
                    continue;
                }

                // Try delete first
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                entry.ExtractToFile(destinationPath, true);
            }
            Logger.Debug($"Extracted .NET runtime");
        });
        await stream.DisposeAsync();

        await AddDotnetRegPatch(path);

        return true;
    }

    public static async Task<bool> AddDllOverride(string? path, string name)
    {
        Logger.Debug($"Adding DLL override {name} to {path}");
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            Logger.Debug($"Prefix is missing!");
            return false;
        }

        string reg = Path.Combine(path!, "user.reg");

        if (!Path.Exists(reg))
        {
            Logger.Debug($"Prefix is not initialised!");
            return false;
        }

        string dllOverrides = $"[Software\\\\Wine\\\\DllOverrides]\n\"{name}\"=\"native,builtin\"\n";
        await File.AppendAllTextAsync(reg, dllOverrides, Encoding.UTF8);
        Logger.Debug($"File written");
        return true;
    }

    public static async Task<bool> AddDotnetRegPatch(string? path)
    {
        Logger.Debug($"Adding .NET Framework registry patch to {path}");
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            Logger.Debug($"Prefix is missing!");
            return false;
        }

        string reg = Path.Combine(path!, "system.reg");

        if (!Path.Exists(reg))
        {
            Logger.Debug($"Prefix is not initialised!");
            return false;
        }

        string regPatch = Encoding.UTF8.GetString(Resources.dotnetReg);

        await File.AppendAllTextAsync(reg, regPatch, Encoding.UTF8);
        Logger.Debug($"File written");
        return true;
    }

    public static bool IsPrefixValid([NotNullWhen(true)] string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        return Directory.Exists(path);
    }

    public static bool IsPrefixPatched([NotNullWhen(true)] string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        string reg = Path.Combine(path, "system.reg");
        if (File.Exists(reg))
        {
            string text = File.ReadAllText(reg, Encoding.UTF8);
            return text.Contains("\"UseRyuJIT\"=dword:00000001", StringComparison.Ordinal);
        }

        return false;
    }

    public static string ToWinePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        // Root path
        if (path.StartsWith("/"))
        {
            return "Z:" + path.Replace("/", "\\");
        }

        // Unknown path
        return path.Replace("/", "\\");
    }

    public static string ToUnixPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        // Root path
        if (path.StartsWith("Z:\\"))
        {
            return path[2..].Replace("\\", "/");
        }

        // Unknown path
        return path.Replace("\\", "/");
    }

}
