namespace HCDE.Gamedata;

/// <summary>
/// <c>ParseMapDefinition</c> tail, <c>GetDefaultLevelNum</c>, and <c>ParseMapHeader</c> head from <c>g_mapinfo.cpp</c> (**2277–2426**).
/// Chunk **1035**; follows **1034** (**2127–2276**).
/// </summary>
public static class VmMapInfoParseMapHeader1035Slice
{
    public const int FmtNew = VmMapInfoParseMapInfo1029Slice.FmtNew;
    public const uint HexenHackFlags = 0x0000020d;
    public const uint HexenHackFlags2 = 0x00000035;

    public static bool ShouldExitMapDefinitionAfterCloseBrace(bool closeBraceParsed) => closeBraceParsed;

    public static int GetDefaultLevelNum(string mapName, out int id24Num)
    {
        id24Num = 0;
        if (mapName.Length >= 3
            && mapName.AsSpan(0, 3).Equals("MAP".AsSpan(), StringComparison.OrdinalIgnoreCase)
            && mapName.Length <= 5)
        {
            if (int.TryParse(mapName.AsSpan(3), out var mapNum) && mapNum is >= 1 and <= 99)
            {
                id24Num = mapNum;
                return mapNum;
            }
        }

        if (mapName.Length >= 4
            && mapName[0] == 'E'
            && mapName[2] == 'M'
            && mapName[1] is >= '0' and <= '9'
            && mapName[3] is >= '0' and <= '9'
            && mapName.Length <= 5)
        {
            var epinum = mapName[1] - '1';
            var tail = mapName.AsSpan(3);
            if (!int.TryParse(tail, out var mapnum))
                mapnum = mapName[3] - '0';
            id24Num = mapnum;
            if (mapName.Length == 4)
                return epinum * 10 + mapnum;
        }

        return 0;
    }

    public static string ResolveMapNameToken(
        bool tokenIsNumber,
        int number,
        string text,
        int formatType,
        ref bool hexenHack,
        ref int formatTypeAfter)
    {
        if (!tokenIsNumber)
            return text.ToUpperInvariant();

        if (formatType == FmtNew)
            return text.ToUpperInvariant();

        hexenHack = true;
        formatTypeAfter = 0;
        return $"MAP{number:00}";
    }

    public static bool ShouldReserveWadLevelSlot(int levelIndex) => levelIndex < 0;

    public static void ApplyHexenHackDefaults(VmMapInfoLevelFlags level, ref int wallHorizLight, ref int wallVertLight)
    {
        wallHorizLight = 0;
        wallVertLight = 0;
        level.Flags |= HexenHackFlags;
        level.Flags2 |= HexenHackFlags2;
    }

    public static void StoreMapName(ref string mapNameStorage, string mapNameToken) =>
        mapNameStorage = mapNameToken.ToUpperInvariant();

    public static bool TryApplyDollarLevelTitle(VmMapInfoHeaderDraft draft, string token)
    {
        if (token.Length == 0 || token[0] != '$')
            return false;
        draft.LookupLevelName = true;
        draft.LevelName = token[1..];
        return true;
    }

    public static bool TryApplyLookupKeywordTitle(
        VmMapInfoHeaderDraft draft,
        string firstToken,
        string secondToken)
    {
        if (!firstToken.Equals("lookup", StringComparison.OrdinalIgnoreCase))
            return false;
        draft.LookupLevelName = true;
        draft.LevelName = secondToken;
        return true;
    }

    public static bool TryApplyIwadDefaultStringTitle(
        VmMapInfoHeaderDraft draft,
        string mapName,
        string parsedTitle,
        bool defaultStringMatches)
    {
        if (!defaultStringMatches)
            return false;
        draft.LookupLevelName = true;
        draft.LevelName = mapName;
        return true;
    }

    public static bool IsHexenFamilyWad(string? resourceFileName) =>
        resourceFileName != null
        && (resourceFileName.Equals("HEXEN.WAD", StringComparison.OrdinalIgnoreCase)
            || resourceFileName.Equals("HEXDD.WAD", StringComparison.OrdinalIgnoreCase));

    public static string BuildHexenTitleLookupKey(string resourceFileName, string mapName) =>
        $"TXT_{resourceFileName[..Math.Min(5, resourceFileName.Length)]}_{mapName}";

    public static bool TryApplyHexenWadTitleKey(
        VmMapInfoHeaderDraft draft,
        bool hexenHack,
        string? resourceFileName,
        string mapName,
        Func<string, bool> stringTableExists)
    {
        if (!hexenHack || !IsHexenFamilyWad(resourceFileName))
            return false;
        var key = BuildHexenTitleLookupKey(resourceFileName!, mapName);
        if (!stringTableExists(key))
            return false;
        draft.LookupLevelName = true;
        draft.LevelName = key;
        return true;
    }

    public static void ApplyPlainLevelTitle(VmMapInfoHeaderDraft draft, string title) =>
        draft.LevelName = title;
}
