namespace HCDE.Playsim;

/// <summary><c>FSpotList</c> and <c>DSpotState</c> from <c>a_specialspot.cpp</c>.</summary>
public static class VmASpecialSpot
{
    public static SpotList NewSpotList(object type) =>
        new() { Type = type, Index = 0, SkipCount = 0, NumCalls = 0 };

    public sealed class SpotList
    {
        public object? Type { get; set; }
        public List<Actor> Spots { get; } = new();
        public uint Index { get; set; }
        public int SkipCount { get; set; }
        public int NumCalls { get; set; }

        public bool Add(Actor newspot)
        {
            if (Spots.Contains(newspot))
                return false;
            Spots.Add(newspot);
            return true;
        }

        public bool Remove(Actor spot)
        {
            var i = Spots.IndexOf(spot);
            if (i < 0)
                return false;
            Spots.RemoveAt(i);
            if (Index > i)
                Index--;
            return true;
        }

        public Actor? GetNextInList(int skipcounter)
        {
            if (Spots.Count == 0 || ++SkipCount <= skipcounter)
                return null;
            SkipCount = 0;
            var spot = Spots[(int)Index];
            if (++Index >= Spots.Count)
                Index = 0;
            NumCalls++;
            return spot;
        }

        public Actor? GetSpotWithMinMaxDistance(double x, double y, double mindist, double maxdist, Func<int, int> randomSpot)
        {
            if (Spots.Count == 0)
                return null;
            var i = randomSpot(Spots.Count);
            var initial = i;
            while (true)
            {
                var distance = Distance2D(Spots[i], x, y);
                if (distance >= mindist && (maxdist == 0 || distance <= maxdist))
                    break;
                i = (i + 1) % Spots.Count;
                if (i == initial)
                    return null;
            }
            NumCalls++;
            return Spots[i];
        }

        public Actor? GetRandomSpot(bool onlyfirst, Func<int, int> randomSpot)
        {
            if (Spots.Count == 0 || NumCalls != 0)
                return null;
            var index = randomSpot(Spots.Count);
            NumCalls++;
            return Spots[index];
        }
    }

    public sealed record SpotListArchive(
        object? Type,
        IReadOnlyList<Actor> Spots,
        uint Index,
        int SkipCount,
        int NumCalls);

    public static SpotListArchive SerializeSpotList(SpotList list) =>
        new(list.Type, list.Spots.ToList(), list.Index, list.SkipCount, list.NumCalls);

    public static void DeserializeSpotList(SpotList list, SpotListArchive archive)
    {
        list.Type = archive.Type;
        list.Spots.Clear();
        list.Spots.AddRange(archive.Spots);
        list.Index = archive.Index;
        list.SkipCount = archive.SkipCount;
        list.NumCalls = archive.NumCalls;
    }

    public sealed class SpotState
    {
        public List<SpotList> SpotLists { get; } = new();

        public void OnDestroy() => SpotLists.Clear();

        public SpotList? FindSpotList(object? type)
        {
            if (type == null)
                return null;
            foreach (var list in SpotLists)
            {
                if (Equals(list.Type, type))
                    return list;
            }
            var created = NewSpotList(type);
            SpotLists.Add(created);
            return created;
        }

        public bool AddSpot(Actor spot, object actorType)
        {
            var list = FindSpotList(actorType);
            return list != null && list.Add(spot);
        }

        public bool RemoveSpot(Actor spot, object actorType)
        {
            var list = FindSpotList(actorType);
            return list != null && list.Remove(spot);
        }

        public Actor? GetNextInList(object? type, int skipcounter)
        {
            var list = FindSpotList(type);
            return list?.GetNextInList(skipcounter);
        }

        public Actor? GetSpotWithMinMaxDistance(object? type, double x, double y, double mindist, double maxdist, Func<int, int> randomSpot)
        {
            var list = FindSpotList(type);
            return list?.GetSpotWithMinMaxDistance(x, y, mindist, maxdist, randomSpot);
        }

        public Actor? GetRandomSpot(object? type, bool onlyonce, Func<int, int> randomSpot)
        {
            var list = FindSpotList(type);
            return list?.GetRandomSpot(onlyonce, randomSpot);
        }
    }

    public static double Distance2D(Actor spot, double x, double y)
    {
        var dx = spot.X.ToDouble() - x;
        var dy = spot.Y.ToDouble() - y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
