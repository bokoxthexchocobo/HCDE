namespace HCDE.Playsim;

/// <summary>Native ACS string id encoding (<c>STRPOOL_LIBRARYID_OR</c> / module table indices).</summary>
internal static class AcsStringIds
{
    public const int LibraryIdMask = unchecked((int)0xFFF00000);
    public const int LibraryIdShift = 20;
    public const int StringPoolLibraryId = int.MaxValue >> LibraryIdShift;
    public const int StringPoolMarker = StringPoolLibraryId << LibraryIdShift;

    public static bool IsGlobalPool(int stringId) =>
        ((uint)stringId >> LibraryIdShift) == StringPoolLibraryId;

    public static int GlobalIndex(int stringId) => stringId & ~LibraryIdMask;

    public static int FromGlobalIndex(int index) => index | StringPoolMarker;

    public static string Lookup(int stringId, string[] moduleTable, AcsGlobalStrings global)
    {
        if (IsGlobalPool(stringId))
            return global.GetByIndex(GlobalIndex(stringId));
        if (stringId >= 0 && stringId < moduleTable.Length)
            return moduleTable[stringId] ?? string.Empty;
        return string.Empty;
    }
}
