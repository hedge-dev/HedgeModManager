using System.Reflection;

namespace HedgeModManager;

public static class Cache
{
    private static Assembly HMMAssembly = typeof(Cache).Assembly;
    private static string? ProgramPath = null;

    public static string GetProgramPath()
    {
        if (ProgramPath != null)
            return ProgramPath;

        var companyAttribute = HMMAssembly.GetCustomAttribute<AssemblyCompanyAttribute>();
        var productAttribute = HMMAssembly.GetCustomAttribute<AssemblyProductAttribute>();
        string companyName = companyAttribute?.Company ?? "NeverFinishAnything";
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
}
