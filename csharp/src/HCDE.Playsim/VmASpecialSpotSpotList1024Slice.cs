namespace HCDE.Playsim;

/// <summary>
/// <c>FSpotList</c> spot registry and selection from <c>a_specialspot.cpp</c> (**1–150**).
/// Chunk **1024**; primary <c>p_floor.cpp</c> **1–150** rejected (missing file + <c>a_floor.cpp</c> overlap chunk **917**).
/// </summary>
public static class VmASpecialSpotSpotList1024Slice
{
    public sealed class SpotActor
    {
        public required string ClassName { get; init; }
        public double X { get; set; }
        public double Y { get; set; }

        public double Distance2D(double x, double y)
        {
            var dx = X - x;
            var dy = Y - y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public sealed class SpotList
    {
        public required string TypeName { get; init; }
        public List<SpotActor> Spots { get; } = new();
        public uint Index { get; set; }
        public int SkipCount { get; set; }
        public int NumCalls { get; set; }
    }

    public static bool TryAddSpot(SpotList list, SpotActor spot)
    {
        foreach (var existing in list.Spots)
        {
            if (ReferenceEquals(existing, spot))
                return false;
        }

        list.Spots.Add(spot);
        return true;
    }

    public static bool TryRemoveSpot(SpotList list, SpotActor spot)
    {
        for (var i = 0; i < list.Spots.Count; i++)
        {
            if (!ReferenceEquals(list.Spots[i], spot))
                continue;
            list.Spots.RemoveAt(i);
            if (list.Index > (uint)i)
                list.Index--;
            return true;
        }

        return false;
    }

    public static SpotActor? GetNextInList(SpotList list, int skipCounter)
    {
        if (list.Spots.Count == 0)
            return null;
        if (++list.SkipCount <= skipCounter)
            return null;

        list.SkipCount = 0;
        var spot = list.Spots[(int)list.Index];
        if (++list.Index >= (uint)list.Spots.Count)
            list.Index = 0;
        list.NumCalls++;
        return spot;
    }

    public static SpotActor? GetSpotWithMinMaxDistance(
        SpotList list,
        double x,
        double y,
        double minDist,
        double maxDist,
        Func<int, int>? randomMod)
    {
        if (list.Spots.Count == 0)
            return null;

        var roll = randomMod?.Invoke(list.Spots.Count) ?? 0;
        var i = roll % list.Spots.Count;
        var initial = i;

        while (true)
        {
            var distance = list.Spots[i].Distance2D(x, y);
            if (distance >= minDist && (maxDist == 0 || distance <= maxDist))
                break;

            i = (i + 1) % list.Spots.Count;
            if (i == initial)
                return null;
        }

        list.NumCalls++;
        return list.Spots[i];
    }

    public static SpotActor? GetRandomSpot(SpotList list, Func<int, int>? randomMod)
    {
        if (list.Spots.Count == 0 || list.NumCalls != 0)
            return null;

        var roll = randomMod?.Invoke(list.Spots.Count) ?? 0;
        var i = roll % list.Spots.Count;
        list.NumCalls++;
        return list.Spots[i];
    }

    public static SpotListSerializeBundle CaptureSerializeFields(SpotList list) =>
        new(list.TypeName, list.Spots.Count, list.Index, list.SkipCount, list.NumCalls);

    public readonly record struct SpotListSerializeBundle(
        string TypeName,
        int SpotCount,
        uint Index,
        int SkipCount,
        int NumCalls);

    public static void ApplySerializeFields(SpotList list, SpotListSerializeBundle bundle)
    {
        list.Index = bundle.Index;
        list.SkipCount = bundle.SkipCount;
        list.NumCalls = bundle.NumCalls;
    }
}
