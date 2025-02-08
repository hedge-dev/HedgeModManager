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
    public string AuthorShort { get; set; } = string.Empty;
    public List<ModAuthor> Authors { get; set; } = new();
    public List<ModDependency> Dependencies { get; set; } = new();
    public List<ICode> Codes { get; set; } = new();
    public List<string> IncludeDirectories { get; set; } = new();
    public string ConfigSchemaFile { get; set; } = string.Empty;
    public string Markdown { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; } = false;
    public string SaveFile { get; set; } = string.Empty;

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
            Description = descSection.Get("Description", string.Empty);
            Date = descSection.Get("Date", string.Empty);
            AuthorShort = descSection.Get("Author", string.Empty);
            Markdown = descSection.Get("Markdown", string.Empty);

            var authorNames = descSection.GetList<string>("Author");
            var authorUrls = descSection.GetList<string>("AuthorURL");
            for (var i = 0; i < authorNames.Count; i++)
            {
                Authors.Add(new ModAuthor()
                {
                    Name = authorNames[i],
                    Url = i < authorUrls.Count ? authorUrls[i] : string.Empty
                });
            }
            if (authorNames.Count == 0)
            {
                if (!string.IsNullOrEmpty(AuthorShort))
                {
                    var split = AuthorShort.Split([", ", " & ", " and "],
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string authorName in split)
                    {
                        Authors.Add(new ModAuthor()
                        {
                            Name = authorName,
                            Url = Authors.Count == 0 ? descSection.Get("AuthorURL", string.Empty) : string.Empty
                        });
                    }
                }
            }
        }

        if (file.TryGetValue("Main", out var mainSection))
        {
            ID = mainSection.Get("ID", string.Empty);
            IncludeDirectories = mainSection.GetList<string>("IncludeDir");
            ConfigSchemaFile = mainSection.Get("ConfigSchemaFile", string.Empty);
            SaveFile = mainSection.Get("SaveFile", string.Empty);
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

            var dependencies = mainSection.GetList<string>("Depends");
            foreach (var dependency in dependencies)
            {
                var splits = dependency.Split('|');
                if (splits.Length == 4)
                {
                    Dependencies.Add(new ModDependency()
                    {
                        ID = splits[0],
                        Title = splits[1],
                        Url = splits[2],
                        Version = splits[3]
                    });
                }
                else
                {
                    logError($"Split count != 4 ({splits.Length})");
                }
            }

        }

        if (string.IsNullOrEmpty(ID))
        {
            ID = Title.GetDeterministicHashCode().ToString("X");
        }

        void logError(string reason)
        {
            Logger.Error($"Mod Parse Error: Title=\"{Title}\" Reason=\"{reason}\"");
        }
    }

    IReadOnlyList<ICode> IMod.Codes => Codes;
}