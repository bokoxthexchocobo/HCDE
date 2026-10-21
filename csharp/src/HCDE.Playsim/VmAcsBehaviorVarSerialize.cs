namespace HCDE.Playsim;

/// <summary>ACS behavior var serialization and <c>FBehavior</c> bootstrap from <c>p_acs.cpp</c> (2171–2327).</summary>
public static class VmAcsBehaviorVarSerialize
{
    public static void SerializeVarsNative(
        bool writing,
        Action serializeMapVarStore,
        int numArrays,
        Action<int> serializeArrayStore)
    {
        if (!BeginVariablesArray(writing))
            return;

        serializeMapVarStore();
        for (var i = 0; i < numArrays; i++)
            serializeArrayStore(i);
        EndVariablesArray();
    }

    public static bool BeginVariablesArray(bool writing) => writing || true;

/// <summary>Whole <c>FBehavior::SerializeVarSet</c> write/read span selection.</summary>
    public static (int count, int first, int last) SerializeVarSetWriteBounds(int[] vars, int max)
    {
        var first = 0;
        while (first < max && vars[first] == 0)
            first++;

        var last = max - 1;
        while (last >= first && vars[last] == 0)
            last--;

        if (last < first)
            return (0, 0, 0);

        return (last - first + 1, first, last);
    }

    public static void SerializeVarSetReadNative(int[] vars, int max, int count, int first)
    {
        Array.Clear(vars, 0, max);
        if (count == 0)
            return;

        if (first + count > max)
            count = max - first;
    }

    public static int ParseLocalArrayChunkNative(int chunkWord1MinusTwo, int[] sizes, int offset)
    {
        var count = (uint)(chunkWord1MinusTwo) / 4u;
        var newOffset = offset;
        for (var i = 0; i < count; i++)
        {
            newOffset += sizes[i];
        }

        return newOffset;
    }

    public static BehaviorCtorFields ResetBehaviorCtorNative()
    {
        return new BehaviorCtorFields
        {
            NumScripts = 0,
            NumFunctions = 0,
            NumArrays = 0,
            LumpNum = -1,
        };
    }

    public static bool InitRejectsTinyLumpNative(int len) => len < 32;

    public static bool InitReadsAcsMagicNative(ReadOnlySpan<byte> header) =>
        header.Length >= 3 && header[0] == (byte)'A' && header[1] == (byte)'C' && header[2] == (byte)'S';

    public struct BehaviorCtorFields
    {
        public int NumScripts;
        public int NumFunctions;
        public int NumArrays;
        public int LumpNum;
    }
}
