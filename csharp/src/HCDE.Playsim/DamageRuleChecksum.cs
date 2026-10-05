namespace HCDE.Playsim;

internal static class DamageRuleChecksum
{
    internal static uint Mix(uint hash, uint value) => (hash ^ value) * 16777619u;

    internal static uint Entry(uint hash, string name, double factor)
    {
        var canonical = name.ToUpperInvariant();
        hash = Mix(hash, (uint)canonical.Length);
        foreach (var character in canonical) hash = Mix(hash, character);
        // Signed zero has identical damage semantics.
        var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(factor == 0 ? 0 : factor));
        hash = Mix(hash, (uint)bits);
        return Mix(hash, (uint)(bits >> 32));
    }
}
