namespace HedgeModManager.UI.CLI;

[AttributeUsage(AttributeTargets.Class)]
public class CliCommandAttribute(string name, string? alias, Type[]? inputs,
    string? description = null, string? usage = null, string? example = null, bool hidden = false) : Attribute
{
    public string Name = name;
    public string? Alias = alias;
    public Type[] Inputs = inputs ?? [];
    public string? Description = description;
    public string? Usage = usage;
    public string? Example = example;
    public bool Hidden = hidden;
}
