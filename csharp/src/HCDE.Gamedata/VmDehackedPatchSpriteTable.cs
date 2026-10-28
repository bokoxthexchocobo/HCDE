namespace HCDE.Gamedata;

/// <summary><c>PatchSpriteNames</c> validation from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchSpriteTable
{
    public static bool IsValidSpriteName(string name) => name.Length == 4;

    public static int GrowSpriteTableSize(int currentSize, long requestedIndex) =>
        (int)Math.Max(currentSize, requestedIndex + 1);

    public static void EnsureTableSize(List<string> table, long index)
    {
        while (table.Count <= index)
            table.Add("");
    }
}
