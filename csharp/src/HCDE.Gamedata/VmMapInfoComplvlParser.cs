namespace HCDE.Gamedata;

public readonly record struct VmMapInfoComplvlFlags(uint Flags1, uint Flags2);

/// <summary><c>G_ParseMapInfo</c> COMPLVL lump presets from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoComplvlParser
{
    private const uint VanillaFlags1 = 2_853_780_231;
    private const uint VanillaFlags2 = 16_650;
    private const uint BoomFlags1 = 2_148_116_480;
    private const uint BoomFlags2 = 16_648;
    private const uint MbfFlags1 = 2_240_391_168;
    private const uint MbfFlags2 = 28_936;
    private const uint Mbf21Flags1 = 2_156_505_088;
    private const uint Mbf21Flags2 = 1_061_128;

    public static bool TryParseComplvl(ReadOnlySpan<char> lump, out VmMapInfoComplvlFlags flags)
    {
        flags = default;
        if (lump.Length == 7 && lump.Equals("vanilla".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            flags = new VmMapInfoComplvlFlags(VanillaFlags1, VanillaFlags2);
            return true;
        }

        if (lump.Length == 4 && lump.Equals("boom".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            flags = new VmMapInfoComplvlFlags(BoomFlags1, BoomFlags2);
            return true;
        }

        if (lump.Length == 3 && lump.Equals("mbf".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            flags = new VmMapInfoComplvlFlags(MbfFlags1, MbfFlags2);
            return true;
        }

        if (lump.Length == 5 && lump.Equals("mbf21".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            flags = new VmMapInfoComplvlFlags(Mbf21Flags1, Mbf21Flags2);
            return true;
        }

        return false;
    }
}
