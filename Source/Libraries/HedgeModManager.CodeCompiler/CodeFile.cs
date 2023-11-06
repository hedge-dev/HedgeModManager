namespace HedgeModManager.CodeCompiler;
using System.Collections;
using PreProcessor;
using Diagnostics;
using Foundation;
using System.Text;

public class CodeFile : IIncludeResolver, IEnumerable<CSharpCode>
{
    public const string TagPrefix = "!!";
    public const string VersionTag = "VERSION";
    protected Version? mFileVersion;

    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    public List<CSharpCode> Codes { get; set; } = new List<CSharpCode>();
    public IEnumerable<CSharpCode> ExecutableCodes => Codes.Where(x => x.IsExecutable());

    public CodeFile() { }

    public CodeFile(string codeFilePath)
    {
        ParseFile(codeFilePath);
    }

    public Version FileVersion
    {
        get
        {
            if (mFileVersion == null)
            {
                Tags.TryGetValue(VersionTag, out string? v);
                mFileVersion = string.IsNullOrEmpty(v) ? new Version(0, 0) : Version.Parse(v);
            }

            return mFileVersion;
        }
        set
        {
            mFileVersion = value;
            Tags[VersionTag] = mFileVersion.ToString();
        }
    }

    public Diff CalculateDiff(CodeFile old)
    {
        var diff = new Diff();
        var addedCodes = new List<CSharpCode>();

        string GetCodeDiffName(CSharpCode code)
        {
            if (code.Type == CodeType.Library)
            {
                return $"[Library{(!string.IsNullOrEmpty(code.Category) ? $"/{code.Category}" : string.Empty)}] {code.Name}";
            }
            return !string.IsNullOrEmpty(code.Category)
                ? $"[{code.Category}] {code.Name}"
                : code.Name;
        }

        foreach (var code in Codes)
        {
            // Added
            if (old.Codes.All(x => x.Name != code.Name))
            {
                addedCodes.Add(code);
                continue;
            }
        }

        foreach (var code in old.Codes)
        {
            // Modified
            if (Codes.SingleOrDefault(x => x.Name == code.Name && x.Category == code.Category) is { } modified)
            {
                if (code.Body != modified.Body)
                {
                    diff.Modified(GetCodeDiffName(code));
                    continue;
                }
            }

            // Renamed
            if (Codes.SingleOrDefault(x => x.Body == code.Body) is { } renamed)
            {
                if (code.Name != renamed.Name || code.Category != renamed.Category)
                {
                    diff.Renamed($"{GetCodeDiffName(code)} -> {GetCodeDiffName(renamed)}", code, renamed);

                    // Remove this code from the added list so we don't display it twice.
                    if (addedCodes.SingleOrDefault(x => x.Name == renamed.Name && x.Category == renamed.Category) is { } duplicate)
                    {
                        addedCodes.Remove(duplicate);
                    }

                    continue;
                }
            }

            // Removed
            if (Codes.All(x => x.Name != code.Name))
            {
                diff.Removed(GetCodeDiffName(code));
                continue;
            }
        }

        foreach (var code in addedCodes)
        {
            diff.Added(GetCodeDiffName(code));
        }

        return diff;
    }

    public void ParseFile(string path)
    {
        if (File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            Parse(stream);
        }
    }

    public void Parse(Stream stream, Encoding? encoding = null)
    {
        var start = stream.Position;
        using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);

        var line = reader.ReadLine();

        while (line != null && line.StartsWith(TagPrefix))
        {
            var tagName = string.Empty;
            var tagValue = string.Empty;

            var separatorIndex = line.IndexOf(' ');
            if (separatorIndex < 0)
            {
                tagName = line.Substring(TagPrefix.Length);
            }
            else
            {
                tagName = line.Substring(TagPrefix.Length, separatorIndex - 2);
                tagValue = line.Substring(separatorIndex + 1).Trim();
            }

            if (!Tags.ContainsKey(tagName))
                Tags.Add(tagName, tagValue);
            else
                Tags[tagName] = tagValue;

            line = reader.ReadLine();
        }

        stream.Position = start;
        reader.DiscardBufferedData();
        Codes.AddRange(CSharpCode.Parse(reader));
    }

    public static unsafe CodeFile FromText(string text)
    {
        var file = new CodeFile();

        fixed (char* textPtr = text)
        {
            using var stream = new UnmanagedMemoryStream((byte*)textPtr, text.Length * sizeof(char));
            file.Parse(stream, BitConverter.IsLittleEndian ? Encoding.Unicode : Encoding.BigEndianUnicode);
            return file;
        }
    }

    public static CodeFile FromFile(string path)
    {
        var file = new CodeFile();
        file.ParseFile(path);
        return file;
    }

    public static CodeFile FromFiles(params string[] paths)
    {
        var file = new CodeFile();
        foreach (var path in paths)
        {
            file.ParseFile(path);
        }

        return file;
    }

    public string? Resolve(string name)
    {
        return Codes.FirstOrDefault(c => c.Name == name)?.Body;
    }

    public IEnumerator<CSharpCode> GetEnumerator()
    {
        return Codes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}