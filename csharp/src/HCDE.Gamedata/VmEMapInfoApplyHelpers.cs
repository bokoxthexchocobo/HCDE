namespace HCDE.Gamedata;

/// <summary>EMAPINFO apply/record helpers from <c>hcde_emapinfo.cpp</c> (375–524).</summary>
public sealed class VmEMapInfoStats
{
    public int Unsupported { get; set; }
    public int Invalid { get; set; }
    public List<(string Map, string Property, int Line)> UnsupportedSamples { get; } = [];
}

public static class VmEMapInfoApplyHelpers
{
    public static void RecordUnsupported(VmEMapInfoStats stats, string mapName, string property, int line)
    {
        stats.Unsupported++;
        if (stats.UnsupportedSamples.Count < 8)
            stats.UnsupportedSamples.Add((mapName, property, line));
    }

    public static void RecordInvalid(VmEMapInfoStats stats, string mapName, string property, int line)
    {
        stats.Invalid++;
        if (stats.UnsupportedSamples.Count < 8)
            stats.UnsupportedSamples.Add((mapName, property + " (invalid value)", line));
    }

    public static bool ApplyBoolProperty(
        VmEMapInfoStats stats, string mapName, string key, string value, int line, ref bool result)
    {
        if (VmEMapInfoLineParser.TryParseBool(value, out result))
            return true;
        RecordInvalid(stats, mapName, key, line);
        return false;
    }

    public static bool ApplyIntProperty(
        VmEMapInfoStats stats, string mapName, string key, string value, int line, ref int result)
    {
        if (VmEMapInfoLineParser.TryParseInt(value, out result))
            return true;
        RecordInvalid(stats, mapName, key, line);
        return false;
    }

    public static bool ApplyDoubleProperty(
        VmEMapInfoStats stats, string mapName, string key, string value, int line, ref double result)
    {
        if (double.TryParse(value, out result))
            return true;
        RecordInvalid(stats, mapName, key, line);
        return false;
    }

    public static bool TryParseBossSpecialFlag(string flag, ref int flags, ref uint flags3)
    {
        var upper = flag.ToUpperInvariant();
        switch (upper)
        {
            case "MAP07_1" or "MAP07_2":
                flags |= 1 << 0; // LEVEL_MAP07SPECIAL placeholder bit
                return true;
            case "E1M8":
                flags3 |= 1u;
                return true;
            case "E2M8":
                flags3 |= 2u;
                return true;
            case "E3M8":
                flags3 |= 4u;
                return true;
            case "E4M6":
                flags3 |= 8u;
                return true;
            case "E4M8":
                flags3 |= 16u;
                return true;
            default:
                return false;
        }
    }

    public static bool TryParseDefaultEnvironmentPair(string value, out int first, out int second)
    {
        first = 0;
        second = 0;
        var parts = VmEMapInfoLineParser.SplitValueList(value);
        if (parts.Count == 0 || parts.Count > 2)
            return false;
        if (!VmEMapInfoLineParser.TryParseInt(parts[0], out first))
            return false;
        if (parts.Count == 1)
            return true;
        return VmEMapInfoLineParser.TryParseInt(parts[1], out second);
    }

    public static ushort PackEnvironmentId(int first, int second) =>
        (ushort)((first << 8) | second);

    public static bool ApplyBossSpecials(ref int flags, ref uint flags3, IEnumerable<string> rawFlags)
    {
        var any = false;
        foreach (var raw in rawFlags)
        {
            if (!TryParseBossSpecialFlag(raw, ref flags, ref flags3))
                return false;
            any = true;
        }

        return any;
    }

    public static bool ApplyDefaultEnvironment(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        Func<ushort, int?> findReverbId,
        out ushort packedId,
        out int? matchedReverbId)
    {
        packedId = 0;
        matchedReverbId = null;
        if (!TryParseDefaultEnvironmentPair(value, out var first, out var second))
        {
            RecordInvalid(stats, mapName, key, line);
            return false;
        }

        packedId = PackEnvironmentId(first, second);
        matchedReverbId = findReverbId(packedId);
        if (matchedReverbId is null)
            RecordUnsupported(stats, mapName, key, line);
        return true;
    }
}
