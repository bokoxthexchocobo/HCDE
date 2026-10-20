namespace HCDE.Gamedata;

/// <summary><c>InitSpawnablesFromMapinfo</c>, class-map dump/sort, spawn CCMDs from <c>g_doomedmap.cpp</c>.</summary>
public static class VmDoomEdSpawnInit
{
    public static IReadOnlyList<(int Key, string ClassName)> SortClassMapForDump(IReadOnlyDictionary<int, string> map) =>
        map.OrderBy(p => p.Key).Select(p => (p.Key, p.Value)).ToList();

    public static void InitSpawnablesFromMapinfo(
        IReadOnlyDictionary<int, VmMapinfoSpawnItem> spawnablesPending,
        IReadOnlyDictionary<int, VmMapinfoSpawnItem> conversationPending,
        Dictionary<int, string> spawnables,
        Dictionary<int, string> conversationIds,
        Func<string, bool> validateClass,
        List<string> errors)
    {
        VmDoomEdSpawnMapParser.ResolveClassMap(spawnablesPending, spawnables, validateClass, errors);
        VmDoomEdSpawnMapParser.ResolveClassMap(conversationPending, conversationIds, validateClass, errors);
    }

    public static void ClearStrifeTypes(Dictionary<int, string> strifeTypes) => strifeTypes.Clear();
}
