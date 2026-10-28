namespace HCDE.Gamedata;

/// <summary><c>ParseNextMap</c> from <c>g_mapinfo.cpp</c> (1041–1064).</summary>
public static class VmMapInfoParseNextMap
{
    public static string FormatNumericNextMap(int number, bool hexenHack) =>
        hexenHack ? $"&wt@{number:D2}" : $"MAP{number:D2}";

    public static string FormatEndSequenceMap(uint sequenceIndex) =>
        $"enDSeQ{sequenceIndex:x4}";
}
