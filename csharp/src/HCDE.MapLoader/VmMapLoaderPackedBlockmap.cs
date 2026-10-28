namespace HCDE.MapLoader;

/// <summary><c>BlockHash</c>, <c>BlockCompare</c>, and <c>CreatePackedBlockmap</c> from <c>maploader.cpp</c> (2237–2321).</summary>
public static class VmMapLoaderPackedBlockmap
{
    public const int HashMultiplier = 12235;
    public const int HashMask = 0x7fffffff;
    public const int BucketCount = 4096;
    public const int EmptyChain = -1;
    public const int ListTerminator = -1;
    public const int ListLeadingZero = 0;
    public const int HeaderSlots = 4;

    public static int ComputeBlockHash(ReadOnlySpan<int> block)
    {
        var hash = 0;
        for (var i = 0; i < block.Length; i++)
            hash = hash * HashMultiplier + block[i];
        return hash & HashMask;
    }

    public static bool BlocksEqual(ReadOnlySpan<int> a, ReadOnlySpan<int> b)
    {
        if (a.Length != b.Length)
            return false;
        if (a.Length == 0)
            return true;
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    public static int BucketIndex(int hash) => hash % BucketCount;

    public static int ResolveDuplicateOffset(int existingCellOffset) => existingCellOffset;

    public static int BeginNewListOffset(int blockMapSize) => blockMapSize;

    public static int[] EncodeBlockList(ReadOnlySpan<int> block)
    {
        var words = new int[block.Length + 2];
        words[0] = ListLeadingZero;
        for (var i = 0; i < block.Length; i++)
            words[i + 1] = block[i];
        words[^1] = ListTerminator;
        return words;
    }

    public sealed class PackState
    {
        public int[] Buckets { get; } = new int[BucketCount];
        public int[] HashChain { get; }
        public List<int> BlockMap { get; } = new();
        public int Hashed;
        public int NotHashed;

        public PackState(int cellCount)
        {
            HashChain = new int[cellCount];
            for (var i = 0; i < BucketCount; i++)
                Buckets[i] = EmptyChain;
            for (var i = 0; i < cellCount; i++)
                HashChain[i] = EmptyChain;
        }
    }

    public static int[] PackBlocks(IReadOnlyList<int[]> blocks, int bmapWidth, int bmapHeight)
    {
        var cellCount = bmapWidth * bmapHeight;
        var state = new PackState(cellCount);
        state.BlockMap.Capacity = cellCount * 3 + HeaderSlots + 8;
        for (var h = 0; h < HeaderSlots; h++)
            state.BlockMap.Add(0);
        for (var i = 0; i < cellCount; i++)
            state.BlockMap.Add(0);

        for (var i = 0; i < cellCount; i++)
        {
            var block = blocks[i];
            var hash = BucketIndex(ComputeBlockHash(block));
            var chain = state.Buckets[hash];
            var match = EmptyChain;
            while (chain != EmptyChain)
            {
                if (BlocksEqual(block, blocks[chain]))
                {
                    match = chain;
                    break;
                }

                chain = state.HashChain[chain];
            }

            if (match != EmptyChain)
            {
                state.BlockMap[HeaderSlots + i] = state.BlockMap[HeaderSlots + match];
                state.Hashed++;
            }
            else
            {
                state.HashChain[i] = state.Buckets[hash];
                state.Buckets[hash] = i;
                state.BlockMap[HeaderSlots + i] = BeginNewListOffset(state.BlockMap.Count);
                foreach (var word in EncodeBlockList(block))
                    state.BlockMap.Add(word);
                state.NotHashed++;
            }
        }

        return state.BlockMap.ToArray();
    }
}
