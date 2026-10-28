namespace HCDE.Playsim;

/// <summary>
/// <c>DSpotState</c> spot-list lookup, registration, and getters from <c>a_specialspot.cpp</c> (**151–300**).
/// Chunk **1025**; list mechanics stay on <see cref="VmASpecialSpotSpotList1024Slice"/>.
/// </summary>
public static class VmASpecialSpotSpotState1025Slice
{
    public sealed class SpotState
    {
        public List<VmASpecialSpotSpotList1024Slice.SpotList> SpotLists { get; } = new();
    }

    public static void OnDestroyNative(SpotState state, Action? resetSpotLists, Action? superOnDestroy)
    {
        resetSpotLists?.Invoke();
        state.SpotLists.Clear();
        superOnDestroy?.Invoke();
    }

    public static VmASpecialSpotSpotList1024Slice.SpotList? FindSpotList(SpotState state, string? typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return null;

        foreach (var list in state.SpotLists)
        {
            if (list.TypeName == typeName)
                return list;
        }

        var created = new VmASpecialSpotSpotList1024Slice.SpotList { TypeName = typeName };
        state.SpotLists.Add(created);
        return created;
    }

    public static bool AddSpotNative(SpotState state, VmASpecialSpotSpotList1024Slice.SpotActor spot)
    {
        var list = FindSpotList(state, spot.ClassName);
        return list is not null && VmASpecialSpotSpotList1024Slice.TryAddSpot(list, spot);
    }

    public static bool RemoveSpotNative(SpotState state, VmASpecialSpotSpotList1024Slice.SpotActor spot)
    {
        var list = FindSpotList(state, spot.ClassName);
        return list is not null && VmASpecialSpotSpotList1024Slice.TryRemoveSpot(list, spot);
    }

    public static VmASpecialSpotSpotList1024Slice.SpotActor? GetNextInListNative(
        SpotState state,
        string? typeName,
        int skipCounter)
    {
        var list = FindSpotList(state, typeName);
        return list is null ? null : VmASpecialSpotSpotList1024Slice.GetNextInList(list, skipCounter);
    }

    public static VmASpecialSpotSpotList1024Slice.SpotActor? GetSpotWithMinMaxDistanceNative(
        SpotState state,
        string? typeName,
        double x,
        double y,
        double minDist,
        double maxDist,
        Func<int, int>? randomMod)
    {
        var list = FindSpotList(state, typeName);
        return list is null
            ? null
            : VmASpecialSpotSpotList1024Slice.GetSpotWithMinMaxDistance(list, x, y, minDist, maxDist, randomMod);
    }

    public static VmASpecialSpotSpotList1024Slice.SpotActor? GetRandomSpotNative(
        SpotState state,
        string? typeName,
        bool onlyOnce,
        Func<int, int>? randomMod)
    {
        var list = FindSpotList(state, typeName);
        if (list is null)
            return null;
        _ = onlyOnce;
        return VmASpecialSpotSpotList1024Slice.GetRandomSpot(list, randomMod);
    }

    public static bool ShouldWriteSpotListsArchiveKey(string key) =>
        string.Equals(key, "spots", StringComparison.Ordinal);

    public static int CountSpotListsForArchive(SpotState state) => state.SpotLists.Count;
}
