namespace HCDE.Gamedata;

public readonly record struct VmDehackedThingBitName(int Bit, int WhichFlags, string Name);

/// <summary><c>LoadDehSupp</c> <c>ThingBits</c> from <c>d_dehacked.cpp</c> (3680–3707).</summary>
public static class VmDehackedDehSuppThingBits
{
    public const int MaxNameLength = 19;

    public static bool TryParseEntry(int bit, int word, string name, out VmDehackedThingBitName entry, out string? error)
    {
        entry = default;
        error = null;
        if (bit < 0 || bit > 31)
        {
            error = $"Invalid bit value {bit}";
            return false;
        }

        if (word < 0 || word > 2)
        {
            error = $"Invalid flag word {word}";
            return false;
        }

        if (string.IsNullOrWhiteSpace(name))
            return false;
        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
            trimmed = trimmed[..MaxNameLength];
        entry = new VmDehackedThingBitName(bit, word, trimmed);
        return true;
    }

    public static bool TryParseThingBitsSection(
        IReadOnlyList<(int Bit, int Word, string Name)> rows,
        out List<VmDehackedThingBitName> bitNames,
        out string? error)
    {
        bitNames = [];
        error = null;
        foreach (var (bit, word, name) in rows)
        {
            if (!TryParseEntry(bit, word, name, out var entry, out error))
                return false;
            bitNames.Add(entry);
        }

        return true;
    }
}
