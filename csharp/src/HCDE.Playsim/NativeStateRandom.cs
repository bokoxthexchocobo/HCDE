namespace HCDE.Playsim;

internal static class NativeStateRandom
{
    internal static ulong Seed(uint seed, uint nameCrc = 0xedd97674u)
    {
        // The default name CRC is the native "StateTics" stream.
        var combined = ((ulong)seed << 32) | nameCrc;
        var z = unchecked(combined + 0x9e3779b97f4a7c15UL);
        z = unchecked((z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL);
        z = unchecked((z ^ (z >> 27)) * 0x94d049bb133111ebUL);
        var initial = z ^ (z >> 31);
        ulong state = 0;
        Next(ref state);
        state = unchecked(state + initial);
        Next(ref state);
        return state;
    }

    internal static uint Next(ref ulong state)
    {
        var old = state;
        state = unchecked(old * 6364136223846793005UL + 0xda3e39cb94b95bdbUL);
        var shifted = (uint)(((old >> 18) ^ old) >> 27);
        var rotation = (int)(old >> 59);
        return (shifted >> rotation) | (shifted << ((-rotation) & 31));
    }

    internal static uint NextBounded(ref ulong state, uint bound)
    {
        if (bound == 0) throw new ArgumentOutOfRangeException(nameof(bound));
        var threshold = unchecked(0u - bound) % bound;
        uint value;
        do { value = Next(ref state); } while (value < threshold);
        return value % bound;
    }
}
