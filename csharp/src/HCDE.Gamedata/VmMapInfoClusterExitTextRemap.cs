namespace HCDE.Gamedata;

/// <summary>Hexen cluster exit-text remap from <c>g_mapinfo.cpp</c> (1008–1027).</summary>
public static class VmMapInfoClusterExitTextRemap
{
    public static bool TryRemapHexenExitLump(
        string resourceFileName,
        string exitText,
        Func<string, bool> stringExists,
        out string tableKey)
    {
        tableKey = "";
        if (!resourceFileName.Equals("HEXEN.WAD", StringComparison.OrdinalIgnoreCase)
            && !resourceFileName.Equals("HEXDD.WAD", StringComparison.OrdinalIgnoreCase))
            return false;

        tableKey = $"TXT_{resourceFileName[..5]}_{exitText}";
        if (!stringExists(tableKey))
            return false;
        return true;
    }
}
