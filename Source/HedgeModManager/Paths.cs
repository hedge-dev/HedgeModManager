using System.Reflection;

namespace HedgeModManager;

public static class Paths
{
    private static Assembly HMMAssembly = typeof(Paths).Assembly;
    private static string? ProgramPath = null;

    public static string GetProgramPath()
    {
        if (ProgramPath != null)
            return ProgramPath;

        var companyAttribute = HMMAssembly.GetCustomAttribute<AssemblyCompanyAttribute>();
        var productAttribute = HMMAssembly.GetCustomAttribute<AssemblyProductAttribute>();
        string companyName = companyAttribute?.Company ?? "hedge-dev";
        string productName = productAttribute?.Product ?? "HedgeModManager";
        return ProgramPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            companyName, productName);
    }

    public static string GetCachePath()
    {
        return Path.Combine(GetProgramPath(), "Cache");
    }

    public static string GetConfigPath()
    {
        return Path.Combine(GetProgramPath(), "Config");
    }

    public static string GetTempPath()
    {
        return Path.Combine(GetProgramPath(), "Temp");
    }

    public static string GetActualUserConfigPath()
    {
        if (Environment.GetEnvironmentVariable("HOME") is string home)
            return Path.Combine(home, ".config");

        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}
