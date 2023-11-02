namespace HedgeModManager.Diagnostics;

public class DiffBlock
{
    public DiffType Type { get; set; }
    public string? Description { get; set; }
    public KeyValuePair<object, object> Data { get; set; }

    public DiffBlock(DiffType type, string? description)
    {
        Type = type;
        Description = description;
    }

    public DiffBlock(DiffType type, string? description, object dataKey, object dataValue)
    {
        Type = type;
        Description = description;
        Data = new(dataKey, dataValue);
    }

    public DiffBlock(DiffType type, string? description, string dataKey)
        : this(type, description, dataKey, string.Empty) { }

    public DiffBlock(DiffType type, string? description, string dataKey, string dataValue)
        : this(type, description, (object)dataKey, (object)dataValue) { }
}