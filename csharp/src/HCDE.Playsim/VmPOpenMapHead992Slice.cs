namespace HCDE.Playsim;

/// <summary>
/// `GetMapIndex`, `P_IsBuildMap`, and `P_OpenMapData` head from <c>p_openmap.cpp</c> (**1–150**).
/// </summary>
public static class VmPOpenMapHead992Slice
{
    public const int MapLumpNameCompareLength = 8;
    public const int MaxClassicMapNameLength = 8;
    public const int ExternalMapPrefixLength = 5;
    public const int RequiredMissingIndex = -2;
    public const int EndOfMapIndex = -1;

    public static readonly MapLumpDescriptor[] MapLumpTable =
    [
        new("", true),
        new("THINGS", true),
        new("LINEDEFS", true),
        new("SIDEDEFS", true),
        new("VERTEXES", true),
        new("SEGS", false),
        new("SSECTORS", false),
        new("NODES", false),
        new("SECTORS", true),
        new("REJECT", false),
        new("BLOCKMAP", false),
        new("BEHAVIOR", false),
        new("LIGHTMAP", false),
    ];

    public readonly struct MapLumpDescriptor(string name, bool required)
    {
        public string Name { get; } = name;
        public bool Required { get; } = required;
    }

    public enum MapLumpScanResult
    {
        Matched,
        RequiredMissing,
        EndOfMap,
    }

    public static bool IsBuildMapProbeAlwaysFalse() => false;

    public static bool MapLumpNameMatches(string lumpName, string expectedName)
    {
        if (expectedName.Length == 0)
            return lumpName.Length == 0;

        return lumpName.AsSpan().StartsWith(
            expectedName.AsSpan(0, Math.Min(expectedName.Length, MapLumpNameCompareLength)),
            StringComparison.OrdinalIgnoreCase);
    }

    public static MapLumpScanResult ScanMapLumpIndex(
        string? lumpName,
        int lastIndex,
        bool needRequired,
        out int index)
    {
        lumpName ??= string.Empty;
        for (var i = lastIndex + 1; i < MapLumpTable.Length; i++)
        {
            ref readonly var entry = ref MapLumpTable[i];
            if (MapLumpNameMatches(lumpName, entry.Name))
            {
                index = i;
                return MapLumpScanResult.Matched;
            }

            if (entry.Required)
            {
                index = needRequired ? RequiredMissingIndex : RequiredMissingIndex;
                return MapLumpScanResult.RequiredMissing;
            }
        }

        index = EndOfMapIndex;
        return MapLumpScanResult.EndOfMap;
    }

    public static bool IsExternalFileMapName(ReadOnlySpan<char> mapName) =>
        mapName.Length >= ExternalMapPrefixLength
        && mapName[..ExternalMapPrefixLength].Equals("file:", StringComparison.OrdinalIgnoreCase);

    public static ReadOnlySpan<char> StripExternalFileMapPrefix(ReadOnlySpan<char> mapName) =>
        mapName.Length >= ExternalMapPrefixLength ? mapName[ExternalMapPrefixLength..] : mapName;

    public static bool UsesClassicShortMapName(string mapName) =>
        mapName.Length <= MaxClassicMapNameLength;

    public static string FormatMapsWadPath(string mapName) => $"maps/{mapName}.wad";

    public static string FormatMapsMapPath(string mapName) => $"maps/{mapName}.map";

    public static bool ShouldPreferShortNameLump(int lumpName, int lumpWad, int lumpMap) =>
        lumpName > lumpWad && lumpName > lumpMap && lumpName != -1;

    public static bool IsLumpFromDifferentContainer(int lumpFile, int nextFile) =>
        lumpFile != nextFile;

    public static bool ShouldAbortStandaloneNonBuildMap(bool buildMapProbe) =>
        !buildMapProbe;
}
