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

        string GetCodeDiffName(CSharpCode code, bool showCodeCategory = false)
        {
            if (code.Type == CodeType.Library)
                return $"[Library{(!string.IsNullOrEmpty(code.Category) ? $"/{code.Category}" : string.Empty)}] {code.Name}";

            if (showCodeCategory)
            {
                return string.IsNullOrEmpty(code.Category)
                    ? code.Name
                    : $"[{code.Category}] {code.Name}";
            }

            return code.Name;
        }

        foreach (var code in Codes)
        {
            // Added
            if (old.Codes.All(x => x != code))
                addedCodes.Add(code);
        }

        foreach (var code in old.Codes)
        {
            void CreateMetadataDiff(CSharpCode compare)
            {
                bool isCategoryDiff = code.Category != compare.Category;

                // Remove this code from the added list so we don't display it twice.
                if (addedCodes.SingleOrDefault(x => x.Name == compare.Name && x.Category == compare.Category) is { } duplicate)
                    addedCodes.Remove(duplicate);

                // Renamed
                if (code.Name != compare.Name)
                {
                    diff.Renamed($"{GetCodeDiffName(code, isCategoryDiff)} -> {GetCodeDiffName(compare, isCategoryDiff)}", code, compare);

                    /* Combine Renamed and Moved blocks into one
                       if both the name and category has changed. */
                    if (isCategoryDiff)
                        return;
                }

                // Moved
                if (isCategoryDiff)
                    diff.Moved($"{compare.Name}: [{code.Category}] -> [{compare.Category}]", code, compare);
            }

            try
            {
                if (Codes.SingleOrDefault(x => x.Name == code.Name && x.Category == code.Category && x.Body.Trim() == code.Body.Trim()) is { } unchanged)
                {
                    if (addedCodes.Contains(unchanged))
                        addedCodes.Remove(unchanged);
                }
                else if (Codes.Count(x => x.Body.Trim() == code.Body.Trim()) == 1 &&
                    Codes.SingleOrDefault(x => x.Body.Trim() == code.Body.Trim()) is { } renamed)
                {
                    CreateMetadataDiff(renamed);
                }
                else if (Codes.SingleOrDefault(x => (x.Name == code.Name && x.Category == code.Category) || x.Name == code.Name) is { } modified)
                {
                    CreateMetadataDiff(modified);
                    diff.Modified(GetCodeDiffName(code, true));
                }
                else
                {
                    diff.Removed(GetCodeDiffName(code, true));
                }
            }
            catch (InvalidOperationException)
            {
                continue;
            }
        }

        foreach (var code in addedCodes)
            diff.Added(GetCodeDiffName(code, true));

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