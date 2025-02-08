namespace HedgeModManager.Diagnostics;
using System.Runtime.CompilerServices;

public class Diff
{
    private readonly List<DiffBlock>[] mBlocks = new List<DiffBlock>[(int)DiffType.Count];

    public Diff()
    {
        for (int i = 0; i < mBlocks.Length; i++)
        {
            mBlocks[i] = new List<DiffBlock>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MakeBlock(DiffType type, string? description)
    {
        mBlocks[(int)type].Add(new DiffBlock(type, description));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MakeBlock(DiffType type, string? description, object dataKey, object dataValue)
    {
        mBlocks[(int)type].Add(new DiffBlock(type, description, dataKey, dataValue));
    }

    public void Add(DiffBlock block)
    {
        mBlocks[(int)block.Type].Add(block);
    }

    public void Added(string? description)
    {
        MakeBlock(DiffType.Added, description);
    }

    public void Modified(string? description, object? oldData = null, object? newData = null)
    {
        if (oldData == null || newData == null)
            MakeBlock(DiffType.Modified, description);
        else
            MakeBlock(DiffType.Modified, description, oldData, newData);
    }

    public void Removed(string? description)
    {
        MakeBlock(DiffType.Removed, description);
    }

    public void Renamed(string? description, object oldData, object newData)
    {
        MakeBlock(DiffType.Renamed, description, oldData, newData);
    }

    public void Moved(string? description, object oldData, object newData)
    {
        MakeBlock(DiffType.Moved, description, oldData, newData);
    }

    public List<DiffBlock> ToList()
    {
        var list = new List<DiffBlock>(mBlocks.Sum(x => x.Count));
        foreach (var block in mBlocks)
        {
            list.AddRange(block);
        }

        return list;
    }
}