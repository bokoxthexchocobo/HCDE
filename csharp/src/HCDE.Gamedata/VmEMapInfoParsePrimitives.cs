namespace HCDE.Gamedata;

/// <summary>EMAPINFO numeric/bool helpers from <c>hcde_emapinfo.cpp</c>.</summary>
public static class VmEMapInfoParsePrimitives
{
    public static bool TryParseIntStrict(string value, out int result)
    {
        result = 0;
        if (value.Length == 0) return false;
        var end = 0;
        while (end < value.Length && !char.IsWhiteSpace(value[end])) end++;
        if (!int.TryParse(value.AsSpan(0, end), out result)) return false;
        for (var i = end; i < value.Length; i++)
            if (!char.IsWhiteSpace(value[i])) return false;
        return true;
    }

    public static bool TryParseDoubleStrict(string value, out double result)
    {
        result = 0;
        if (value.Length == 0) return false;
        var end = 0;
        while (end < value.Length && !char.IsWhiteSpace(value[end])) end++;
        if (!double.TryParse(value.AsSpan(0, end), out result)) return false;
        for (var i = end; i < value.Length; i++)
            if (!char.IsWhiteSpace(value[i])) return false;
        return true;
    }

    public static void SetFlag(ref int flags, int bit, bool enabled)
    {
        if (enabled) flags |= bit;
        else flags &= ~bit;
    }

    public static void SetFlag(ref uint flags, uint bit, bool enabled)
    {
        if (enabled) flags |= bit;
        else flags &= ~bit;
    }

    public static int GetDefaultLevelNum(string mapName, out int id24Num)
    {
        id24Num = 0;
        if (mapName.Length >= 3 && mapName.StartsWith("MAP", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(mapName.AsSpan(3), out var mapNum) && mapNum is >= 1 and <= 99)
            {
                id24Num = mapNum;
                return mapNum;
            }
        }

        if (mapName.Length >= 4 && mapName[0] == 'E' && mapName[2] == 'M' &&
            char.IsDigit(mapName[1]) && char.IsDigit(mapName[3]))
        {
            var epinum = mapName[1] - '1';
            var mapnum = mapName[3] - '0';
            id24Num = mapnum;
            return epinum * 10 + mapnum;
        }

        return 0;
    }
}
