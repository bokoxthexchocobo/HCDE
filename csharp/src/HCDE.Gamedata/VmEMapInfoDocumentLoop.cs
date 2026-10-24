namespace HCDE.Gamedata;

/// <summary><c>HCDE_ParseEMapInfo</c> section/property loop from <c>hcde_emapinfo.cpp</c> (847–921).</summary>
public static class VmEMapInfoDocumentLoop
{
    public static bool ShouldMirrorSky2ToSky1(string sky2) =>
        VmEMapInfoPropertyTail.IsNoFlatSky2(sky2);

    public static void ApplySky2Mirror(VmEMapInfoLevelDraft draft)
    {
        if (ShouldMirrorSky2ToSky1(draft.SkyPic2))
            draft.SkyPic2 = draft.SkyPic1;
    }

    public static bool IsKnownEternityMap(IReadOnlyDictionary<string, bool> registry, string? mapName) =>
        mapName is { Length: > 0 } && registry.ContainsKey(mapName);

    public static int ResolveExtraDataLump(
        IReadOnlyDictionary<string, int> lumpsByMap,
        string? mapName,
        string? edName,
        Func<string, int> lookupByPath)
    {
        if (mapName is { Length: > 0 } && lumpsByMap.TryGetValue(mapName, out var lump) && lump >= 0)
            return lump;
        if (string.IsNullOrEmpty(edName))
            return -1;
        return lookupByPath(edName);
    }
}
