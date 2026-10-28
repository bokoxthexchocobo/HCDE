namespace HCDE.Gamedata;

/// <summary>
/// `ParseMapInfo` top-level tail, `G_ClearMapinfo`, and `HCDE_ParseCMPGNINF` scanner helpers from
/// <c>g_mapinfo.cpp</c> (**2750–2899**). Chunk **1030**; follows **1029** (**2600–2749**).
/// </summary>
public static class VmMapInfoParseMapInfo1030Slice
{
    public const int TkIdentifier = 1;
    public const int TkStringConst = 2;

    public const string CmpgninfMapNameTokenError =
        "CMPGNINF mapname must be an identifier or quoted string.";

    public sealed class MapinfoClearSnapshot
    {
        public int DefaultSkill { get; set; } = -1;
        public string PrimaryLevelF1Pic { get; set; } = "";
        public bool PrimaryLevelInfoCleared { get; set; }
        public int ClearedCollectionCount { get; set; }
    }

    public enum LateTopLevelCommand
    {
        None,
        DamageType,
        SpawnNums,
        ConversationIds,
        Automap,
        UnknownTopLevel
    }

    public static MapinfoClearSnapshot CreateClearedMapinfoState()
    {
        return new MapinfoClearSnapshot
        {
            DefaultSkill = -1,
            PrimaryLevelF1Pic = "",
            PrimaryLevelInfoCleared = true,
            ClearedCollectionCount = 5
        };
    }

    public static bool TryParseLateTopLevelKeyword(
        string keyword,
        int formatType,
        out LateTopLevelCommand command,
        out bool automapOverlay,
        out string? oldFormatError,
        out string? unknownTopLevelError)
    {
        command = LateTopLevelCommand.None;
        automapOverlay = false;
        oldFormatError = null;
        unknownTopLevelError = null;

        if (keyword.Equals("damagetype", StringComparison.OrdinalIgnoreCase))
            return RequireNewFormat(formatType, "damagetype definitions not supported with old MAPINFO syntax", out oldFormatError, LateTopLevelCommand.DamageType, out command);

        if (keyword.Equals("spawnnums", StringComparison.OrdinalIgnoreCase))
            return RequireNewFormat(formatType, "spawnnums definitions not supported with old MAPINFO syntax", out oldFormatError, LateTopLevelCommand.SpawnNums, out command);

        if (keyword.Equals("conversationids", StringComparison.OrdinalIgnoreCase))
            return RequireNewFormat(formatType, "conversationids definitions not supported with old MAPINFO syntax", out oldFormatError, LateTopLevelCommand.ConversationIds, out command);

        if (keyword.Equals("automap", StringComparison.OrdinalIgnoreCase)
            || keyword.Equals("automap_overlay", StringComparison.OrdinalIgnoreCase))
        {
            automapOverlay = keyword.Equals("automap_overlay", StringComparison.OrdinalIgnoreCase);
            return RequireNewFormat(formatType, "automap colorset definitions not supported with old MAPINFO syntax", out oldFormatError, LateTopLevelCommand.Automap, out command);
        }

        unknownTopLevelError = FormatUnknownTopLevelKeyword(keyword);
        command = LateTopLevelCommand.UnknownTopLevel;
        return false;
    }

    public static string FormatUnknownTopLevelKeyword(string keyword) =>
        $"{keyword}: Unknown top level keyword";

    public static int AdvanceIndexedPropertyBracketDepth(int depth, char token)
    {
        if (token == '[')
            return depth + 1;
        if (token == ']')
            return depth - 1;
        return depth;
    }

    public static bool IsIndexedPropertyBracketOpen(char token) => token == '[';

    public static bool ShouldContinueSkippingCmpgninfValue(bool sawComma, bool nextIsValue) =>
        sawComma && !nextIsValue;

    public static bool IsValidCmpgninfMapNameToken(int tokenType) =>
        tokenType == TkIdentifier || tokenType == TkStringConst;

    public static bool TryValidateCmpgninfMapNameToken(int tokenType, out string? error)
    {
        if (IsValidCmpgninfMapNameToken(tokenType))
        {
            error = null;
            return true;
        }

        error = CmpgninfMapNameTokenError;
        return false;
    }

    private static bool RequireNewFormat(
        int formatType,
        string oldError,
        out string? oldFormatError,
        LateTopLevelCommand whenNew,
        out LateTopLevelCommand command)
    {
        oldFormatError = null;
        if (formatType == VmMapInfoParseMapInfo1029Slice.FmtOld)
        {
            oldFormatError = oldError;
            command = LateTopLevelCommand.None;
            return false;
        }

        command = whenNew;
        return true;
    }
}
