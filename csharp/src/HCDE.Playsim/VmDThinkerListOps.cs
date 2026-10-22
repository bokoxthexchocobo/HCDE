namespace HCDE.Playsim;

/// <summary><c>FThinkerList</c> list helpers and tick/profile passes (<c>dthinker.cpp</c> 452–795).</summary>
public static class VmDThinkerListOps
{
    public const int OfJustSpawned = 1 << 0;
    public const int OfEuthanizeMe = 1 << 1;
    public const int OfSentinel = 1 << 2;

    public const int MaxStatNum = ThinkerStat.MaxStatNum;

    public sealed class ThinkerNodeModel
    {
        public ThinkerNodeModel? PrevThinker { get; set; }
        public ThinkerNodeModel? NextThinker { get; set; }
        public int ObjectFlags { get; set; }
        public Action? CallPostBeginPlay { get; set; }
        public Action? CallTick { get; set; }
        public Func<string>? TypeName { get; set; }
    }

    public sealed class ThinkerListModel
    {
        public ThinkerNodeModel? Sentinel { get; set; }
        public Func<ThinkerNodeModel>? CreateSentinel { get; set; }
        public Action<ThinkerNodeModel, ThinkerNodeModel?>? WriteBarrier { get; set; }
    }

    public sealed class CollectionModel
    {
        public ThinkerListModel[] Thinkers { get; init; } = Array.Empty<ThinkerListModel>();
        public ThinkerListModel[] FreshThinkers { get; init; } = Array.Empty<ThinkerListModel>();
    }

    public sealed class ProfileEntry
    {
        public int NumCalls { get; set; }
        public Action? Clock { get; set; }
        public Action? Unclock { get; set; }
    }

    public sealed class TickContext
    {
        public int ThinkCount { get; set; }
        public Action<string>? FatalError { get; set; }
        public Dictionary<string, ProfileEntry>? Profiles { get; init; }
    }

    public static void AddTail(ThinkerListModel list, ThinkerNodeModel thinker)
    {
        if (list.Sentinel == null)
        {
            var sentinel = list.CreateSentinel?.Invoke()
                ?? throw new InvalidOperationException("CreateSentinel required when list is empty");
            sentinel.ObjectFlags |= OfSentinel;
            sentinel.NextThinker = sentinel;
            sentinel.PrevThinker = sentinel;
            list.Sentinel = sentinel;
            list.WriteBarrier?.Invoke(sentinel, null);
        }

        var tail = list.Sentinel.PrevThinker!;
        thinker.PrevThinker = tail;
        thinker.NextThinker = list.Sentinel;
        tail.NextThinker = thinker;
        list.Sentinel.PrevThinker = thinker;
        list.WriteBarrier?.Invoke(thinker, tail);
        list.WriteBarrier?.Invoke(thinker, list.Sentinel);
        list.WriteBarrier?.Invoke(tail, thinker);
        list.WriteBarrier?.Invoke(list.Sentinel, thinker);
    }

    public static ThinkerNodeModel? GetHead(ThinkerListModel list)
    {
        if (list.Sentinel == null || list.Sentinel.NextThinker == list.Sentinel)
            return null;
        return list.Sentinel.NextThinker;
    }

    public static ThinkerNodeModel? GetTail(ThinkerListModel list)
    {
        if (list.Sentinel == null || list.Sentinel.PrevThinker == list.Sentinel)
            return null;
        return list.Sentinel.PrevThinker;
    }

    public static bool IsEmpty(ThinkerListModel list) =>
        list.Sentinel == null || list.Sentinel.NextThinker == null;

    public static void Remove(ThinkerListModel list, ThinkerNodeModel node)
    {
        if (list.Sentinel == null || node.PrevThinker == null || node.NextThinker == null)
            return;
        node.PrevThinker.NextThinker = node.NextThinker;
        node.NextThinker.PrevThinker = node.PrevThinker;
        node.PrevThinker = null;
        node.NextThinker = null;
    }

    public static ThinkerNodeModel? FirstThinker(CollectionModel collection, int statnum)
    {
        if ((uint)statnum > MaxStatNum)
            statnum = MaxStatNum;

        var node = GetHead(collection.Thinkers[statnum]);
        if (node != null)
            return node;

        return GetHead(collection.FreshThinkers[statnum]);
    }

    public static void MarkRoots(CollectionModel collection, Action<ThinkerNodeModel?> mark)
    {
        for (var i = 0; i <= MaxStatNum; i++)
        {
            mark(collection.Thinkers[i].Sentinel);
            mark(collection.FreshThinkers[i].Sentinel);
        }

        mark(collection.Thinkers[MaxStatNum + 1].Sentinel);
    }

    public static int TickThinkers(ThinkerListModel list, ThinkerListModel? dest, TickContext ctx)
    {
        var node = GetHead(list);
        if (node == null)
            return 0;

        var count = 0;
        var sentinel = list.Sentinel!;
        while (node != sentinel)
        {
            count++;
            var next = node.NextThinker!;
            if ((node.ObjectFlags & OfJustSpawned) != 0)
            {
                if (dest != null)
                {
                    Remove(list, node);
                    AddTail(dest, node);
                }

                node.CallPostBeginPlay?.Invoke();
            }
            else if (dest != null)
            {
                ctx.FatalError?.Invoke("There is a thinker in the fresh list that has already ticked.\n");
                return count;
            }

            if ((node.ObjectFlags & OfEuthanizeMe) == 0)
            {
                ctx.ThinkCount++;
                node.CallTick?.Invoke();
                node.ObjectFlags &= ~OfJustSpawned;
            }

            node = next;
        }

        return count;
    }

    public static int ProfileThinkers(ThinkerListModel list, ThinkerListModel? dest, TickContext ctx)
    {
        var node = GetHead(list);
        if (node == null)
            return 0;

        var count = 0;
        var sentinel = list.Sentinel!;
        while (node != sentinel)
        {
            count++;
            var next = node.NextThinker!;
            if ((node.ObjectFlags & OfJustSpawned) != 0)
            {
                if (dest != null)
                {
                    Remove(list, node);
                    AddTail(dest, node);
                }

                node.CallPostBeginPlay?.Invoke();
            }
            else if (dest != null)
            {
                ctx.FatalError?.Invoke("There is a thinker in the fresh list that has already ticked.\n");
                return count;
            }

            if ((node.ObjectFlags & OfEuthanizeMe) == 0)
            {
                ctx.ThinkCount++;
                var className = node.TypeName?.Invoke() ?? "";
                if (!ctx.Profiles!.TryGetValue(className, out var prof))
                {
                    prof = new ProfileEntry();
                    ctx.Profiles[className] = prof;
                }

                prof.NumCalls++;
                prof.Clock?.Invoke();
                node.CallTick?.Invoke();
                prof.Unclock?.Invoke();
                node.ObjectFlags &= ~OfJustSpawned;
            }

            node = next;
        }

        return count;
    }
}
