namespace HCDE.Gamedata;

/// <summary><c>RememberEMapInfoMap</c> tracking from <c>hcde_emapinfo.cpp</c>.</summary>
public static class VmEMapInfoRegistry
{
    private static readonly HashSet<string> KnownMaps = new(StringComparer.OrdinalIgnoreCase);

    public static void RememberMap(string mapName) => KnownMaps.Add(mapName);

    public static bool Contains(string mapName) => KnownMaps.Contains(mapName);

    public static void Clear() => KnownMaps.Clear();
}
