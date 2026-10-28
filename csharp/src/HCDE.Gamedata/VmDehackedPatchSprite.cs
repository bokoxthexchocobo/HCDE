namespace HCDE.Gamedata;

/// <summary><c>PatchSprite</c> offset/name remap from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchSprite
{
    public static bool IsSpriteIndexInRange(int sprNum, int orgSize) =>
        sprNum >= 0 && sprNum < orgSize;

    public static int ComputeNameOffset(int offset, int versionTableOffset, int dversionIndex, IReadOnlyList<int> toff)
    {
        if (offset <= 0) return -1;
        var baseOffset = toff[dversionIndex];
        return (offset - baseOffset - 22044) / 8;
    }

    public static string? TryRemapSpriteName(int nameOffset, int orgSize, IReadOnlyList<string> orgNames, int sprNum, string currentName)
    {
        if (nameOffset < 0 || nameOffset >= orgSize || sprNum < 0) return null;
        if (nameOffset >= orgNames.Count) return null;
        return orgNames[nameOffset].Length >= 4 ? orgNames[nameOffset][..4] : orgNames[nameOffset];
    }
}
