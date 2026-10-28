namespace HCDE.Client;

/// <summary>Filesystem seam for <c>FStringTable::LoadStrings</c>.</summary>
public interface IVmStringTableLumpSource
{
    IEnumerable<VmStringTableLump> EnumerateLumps(string lumpName);
}

public readonly record struct VmStringTableLump(int LumpIndex, int FileNum, string Text);
