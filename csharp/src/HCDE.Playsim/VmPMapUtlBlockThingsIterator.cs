namespace HCDE.Playsim;

/// <summary><c>FBlockThingsIterator</c> from <c>p_maputl.cpp</c> (864–1026).</summary>
public static class VmPMapUtlBlockThingsIterator
{
    public const int MapBlockUnits = 128;
    public const int FixedBucketCount = 64;
    public const int FixedHashCapacity = 64;

    public sealed class BlockNode
    {
        public object? Me { get; set; }
        public BlockNode? NextActor { get; set; }
        public BlockNode? NextBlock { get; set; }
        public object? PrevBlock { get; set; }
        public object? ActorBlockNodeHead { get; set; }
    }

    public sealed class HashEntry
    {
        public object? Actor { get; set; }
        public int Next { get; set; } = -1;
    }

    public sealed class IteratorState
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }
        public int CurX { get; set; }
        public int CurY { get; set; }
        public BlockNode? Block { get; set; }
        public int[] Buckets { get; } = Enumerable.Repeat(-1, FixedBucketCount).ToArray();
        public HashEntry[] FixedHash { get; } = Enumerable.Range(0, FixedHashCapacity).Select(_ => new HashEntry()).ToArray();
        public List<HashEntry> DynHash { get; } = new();
        public int NumFixedHash { get; set; }
        public double BmapOriginX { get; set; }
        public double BmapOriginY { get; set; }
    }

    public static void ClearHash(IteratorState state)
    {
        Array.Fill(state.Buckets, -1);
        state.NumFixedHash = 0;
        state.DynHash.Clear();
    }

    public static void InitFromBox(IteratorState state, double left, double bottom, double right, double top, Func<double, int> blockX, Func<double, int> blockY, bool clearHash)
    {
        state.MaxY = blockY(top);
        state.MinY = blockY(bottom);
        state.MaxX = blockX(right);
        state.MinX = blockX(left);
        if (clearHash)
            ClearHash(state);
        state.CurX = state.MinX - 1;
        state.CurY = state.MinY;
    }

    public static void StartBlock(IteratorState state, int x, int y, Func<int, int, bool> isValid, Func<int, int, BlockNode?> getHead)
    {
        state.CurX = x;
        state.CurY = y;
        state.Block = isValid(x, y) ? getHead(x, y) : null;
    }

    public static void SwitchBlock(IteratorState state, int x, int y, Func<int, int, bool> isValid, Func<int, int, BlockNode?> getHead)
    {
        state.MinX = state.MaxX = x;
        state.MinY = state.MaxY = y;
        StartBlock(state, x, y, isValid, getHead);
    }

    public static object? Next(
        IteratorState state,
        bool centerOnly,
        Func<int, int, bool> isValid,
        Func<int, int, BlockNode?> getHead,
        Func<object?, double> actorX,
        Func<object?, double> actorY)
    {
        while (true)
        {
            while (state.Block is not null)
            {
                var me = state.Block.Me;
                var myNode = state.Block;
                state.Block = state.Block.NextActor;

                if (myNode.NextBlock is null && ReferenceEquals(myNode.PrevBlock, myNode.ActorBlockNodeHead))
                    return me;

                if (centerOnly)
                {
                    var blockLeft = state.CurX * MapBlockUnits + state.BmapOriginX;
                    var blockRight = blockLeft + MapBlockUnits;
                    var blockBottom = state.CurY * MapBlockUnits + state.BmapOriginY;
                    var blockTop = blockBottom + MapBlockUnits;
                    var x = actorX(me!);
                    var y = actorY(me!);
                    if (x >= blockLeft && x < blockRight && y >= blockBottom && y < blockTop)
                        return me;
                    continue;
                }

                var hash = ((nint)me! >> 3) % FixedBucketCount;
                var chain = state.Buckets[hash];
                var alreadySeen = false;
                while (chain >= 0)
                {
                    var entry = chain < FixedHashCapacity ? state.FixedHash[chain] : state.DynHash[chain - FixedHashCapacity];
                    if (ReferenceEquals(entry.Actor, me))
                    {
                        alreadySeen = true;
                        break;
                    }

                    chain = entry.Next;
                }

                if (alreadySeen)
                    continue;

                HashEntry slot;
                if (state.NumFixedHash < FixedHashCapacity)
                {
                    slot = state.FixedHash[state.NumFixedHash];
                    slot.Next = state.Buckets[hash];
                    state.Buckets[hash] = state.NumFixedHash++;
                }
                else
                {
                    slot = new HashEntry { Next = state.Buckets[hash] };
                    var idx = state.DynHash.Count;
                    state.DynHash.Add(slot);
                    state.Buckets[hash] = idx + FixedHashCapacity;
                }

                slot.Actor = me;
                return me;
            }

            if (++state.CurX > state.MaxX)
            {
                state.CurX = state.MinX;
                if (++state.CurY > state.MaxY)
                    return null;
            }

            StartBlock(state, state.CurX, state.CurY, isValid, getHead);
        }
    }
}
