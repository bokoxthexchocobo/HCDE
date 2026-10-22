namespace HCDE.Playsim;

/// <summary><c>FSpotList</c> and <c>DSpotState</c> spot routing from <c>a_specialspot.cpp</c> (53–305).</summary>
public static class VmASpecialSpot
{
    public sealed class SpotActor
    {
        public double X { get; set; }
        public double Y { get; set; }
        public object? Class { get; set; }

        public double Distance2D(double x, double y)
        {
            var dx = X - x;
            var dy = Y - y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public sealed class SpotList
    {
        public object? Type { get; set; }
        public List<SpotActor> Spots { get; } = new();
        public uint Index { get; set; }
        public int SkipCount { get; set; }
        public int NumCalls { get; set; }

        public SpotList()
        {
        }

        public SpotList(object? type)
        {
            Type = type;
            Index = 0;
            SkipCount = 0;
            NumCalls = 0;
        }

        public bool Add(SpotActor newspot)
        {
            foreach (var spot in Spots)
            {
                if (ReferenceEquals(spot, newspot))
                    return false;
            }

            Spots.Add(newspot);
            return true;
        }

        public bool Remove(SpotActor spot)
        {
            for (var i = 0; i < Spots.Count; i++)
            {
                if (ReferenceEquals(Spots[i], spot))
                {
                    Spots.RemoveAt(i);
                    if (Index > i)
                        Index--;
                    return true;
                }
            }

            return false;
        }

        public SpotActor? GetNextInList(int skipcounter)
        {
            if (Spots.Count > 0 && ++SkipCount > skipcounter)
            {
                SkipCount = 0;
                var spot = Spots[(int)Index];
                if (++Index >= Spots.Count)
                    Index = 0;
                NumCalls++;
                return spot;
            }

            return null;
        }

        public SpotActor? GetSpotWithMinMaxDistance(double x, double y, double mindist, double maxdist, Func<int, int>? randomSpot)
        {
            if (Spots.Count == 0)
                return null;

            var i = (randomSpot?.Invoke(Spots.Count) ?? 0) % Spots.Count;
            var initial = i;

            while (true)
            {
                var distance = Spots[i].Distance2D(x, y);
                if (distance >= mindist && (maxdist == 0 || distance <= maxdist))
                    break;

                i = (i + 1) % Spots.Count;
                if (i == initial)
                    return null;
            }

            NumCalls++;
            return Spots[i];
        }

        public SpotActor? GetRandomSpot(bool onlyfirst, Func<int, int>? randomSpot)
        {
            if (Spots.Count > 0 && NumCalls == 0)
            {
                var i = (randomSpot?.Invoke(Spots.Count) ?? 0) % Spots.Count;
                NumCalls++;
                return Spots[i];
            }

            return null;
        }
    }

    public sealed class SpotState
    {
        public List<SpotList> SpotLists { get; } = new();

        public void OnDestroy() => SpotLists.Clear();

        public void Tick()
        {
        }

        public SpotList? FindSpotList(object? type)
        {
            if (type is null)
                return null;

            foreach (var list in SpotLists)
            {
                if (ReferenceEquals(list.Type, type))
                    return list;
            }

            var created = new SpotList(type);
            SpotLists.Add(created);
            return created;
        }

        public bool AddSpot(SpotActor spot)
        {
            var list = FindSpotList(spot.Class);
            return list is not null && list.Add(spot);
        }

        public bool RemoveSpot(SpotActor spot)
        {
            var list = FindSpotList(spot.Class);
            return list is not null && list.Remove(spot);
        }

        public SpotActor? GetNextInList(object? type, int skipcounter) =>
            FindSpotList(type)?.GetNextInList(skipcounter);

        public SpotActor? GetSpotWithMinMaxDistance(object? type, double x, double y, double mindist, double maxdist, Func<int, int>? randomSpot) =>
            FindSpotList(type)?.GetSpotWithMinMaxDistance(x, y, mindist, maxdist, randomSpot);

        public SpotActor? GetRandomSpot(object? type, bool onlyonce, Func<int, int>? randomSpot) =>
            FindSpotList(type)?.GetRandomSpot(onlyonce, randomSpot);
    }
}
