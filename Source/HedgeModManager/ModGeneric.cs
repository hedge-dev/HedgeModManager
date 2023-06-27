namespace HedgeModManager;
using CodeCompiler;
using Foundation;
using Updates;
using Text;

public class ModGeneric : IMod
{
    public const string ConfigName = "mod.ini";

    public string ID { get; set; } = string.Empty;
    public string Root { get; private set; }
    public bool Enabled { get; set; }
    public IUpdateSource? Updater { get; set; } = null;
    public ModAttribute Attributes { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Version { get; set; } = "0.0";
    public List<ModAuthor> Authors { get; set; } = new();
    public List<ModDependency> Dependencies { get; set; } = new();
    public List<ICode> Codes { get; set; } = new();
    public List<string> IncludeDirectories { get; set; } = new();

    public ModGeneric()
    {
        Root = string.Empty;
    }

    public ModGeneric(string root)
    {
        Root = root;
    }

    public void Parse(Ini file)
    {
        if (file.TryGetValue("Desc", out var descSection))
        {
            Title = descSection.Get("Title", string.Empty);
            Version = descSection.Get("Version", string.Empty);
        }

        if (file.TryGetValue("Main", out var mainSection))
        {
            ID = mainSection.Get("ID", string.Empty);
            IncludeDirectories = mainSection.GetList<string>("IncludeDir");
            var server = mainSection.Get("UpdateServer", string.Empty);
            if (!string.IsNullOrEmpty(server))
            {
                Updater = new UpdateSourceGMI<ModGeneric>(this, new Uri(server));
            }

            var codeFiles = mainSection.Get("CodeFile", string.Empty);
            if (!string.IsNullOrEmpty(codeFiles))
            {
                foreach (var codeFile in codeFiles.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var codePath = Path.Combine(Root, codeFile.Trim());
                    if (File.Exists(codePath))
                    {
                        Codes.AddRange(CodeFile.FromFile(codePath).Codes);
                    }
                }
            }

            var dllFiles = mainSection.Get("DLLFile", string.Empty);
            if (!string.IsNullOrEmpty(dllFiles))
            {
                foreach (var dllFile in dllFiles.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var dllPath = Path.Combine(Root, dllFile.Trim());
                    if (File.Exists(dllPath))
                    {
                        Codes.Add(new BinaryCode(dllPath));
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(ID))
        {
            ID = Title.GetDeterministicHashCode().ToString("X");
        }
    }

    IReadOnlyList<ICode> IMod.Codes => Codes;
}