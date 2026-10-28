namespace HCDE.Gamedata;

/// <summary>
/// Optional float compat, skip/EOF guards, lookup-name parse, and lump name token from <c>g_mapinfo.cpp</c> (**601–750**).
/// Chunk **1055**; follows **1054** (**451–600**). Uncredited <see cref="VmMapInfoOptionalFloatCompat"/> / <see cref="VmMapInfoParseLookupName"/> — seam only.
/// </summary>
public static class VmMapInfoParseMid1055Slice
{
    public const int FmtOld = VmMapInfoParseParser1054Slice.FmtOld;
    public const int FmtNew = VmMapInfoParseParser1054Slice.FmtNew;

    public static bool TryResolveOptionalFloatCompat(
        bool standardCheckFloatSucceeded,
        int formatType,
        bool skySpeedNoCommaCompatActive,
        bool scannerHasFloat,
        double scannerFloat,
        bool scannerHasNumber,
        int scannerNumber,
        ref bool compatWarningEmitted,
        string propertyName,
        out double value,
        out string? compatWarning)
    {
        value = 0;
        compatWarning = null;
        if (standardCheckFloatSucceeded)
            return true;

        if (formatType != FmtNew || !skySpeedNoCommaCompatActive)
            return false;

        if (scannerHasFloat)
            value = scannerFloat;
        else if (scannerHasNumber)
            value = scannerNumber;
        else
            return false;

        if (TakeFirstCompatWarning(ref compatWarningEmitted))
        {
            compatWarning =
                $"HCDE: tolerated MAPINFO '{propertyName}' value without comma for known mod compatibility.";
        }

        return true;
    }

    public static bool TakeFirstCompatWarning(ref bool warnedOnce)
    {
        if (warnedOnce)
            return false;
        warnedOnce = true;
        return true;
    }

    public static int CountSkipToNextTokens(bool tokenIsEquals, ReadOnlySpan<bool> commaContinuations)
    {
        if (!tokenIsEquals)
            return 0;
        var count = 1;
        foreach (var comma in commaContinuations)
        {
            if (!comma)
                break;
            count++;
        }

        return count;
    }

    public static bool ShouldScriptErrorUnexpectedEof(int formatType, bool scannerAtEnd) =>
        formatType == FmtNew && scannerAtEnd;

    public static bool TokenIsLookupKeyword(string token) =>
        token.Equals("lookup", StringComparison.OrdinalIgnoreCase);

    public static bool TryParseDollarLookupPrefix(string token, out string tableName)
    {
        if (token.Length > 0 && token[0] == '$')
        {
            tableName = token[1..];
            return true;
        }

        tableName = token;
        return false;
    }

    public static void AppendNewFormatTextLine(ref string dest, string line) => dest += line + '\n';

    public static string TrimTrailingNewlineFromMultiline(string dest) =>
        dest.Length > 0 ? dest[..^1] : dest;

    public static bool ShouldStopMultilineOnTrailingCommaCompat(
        bool sawComma,
        bool trailingCommaCompatActive,
        bool nextTokenIsCloseBrace,
        ref bool compatMessageEmitted)
    {
        if (!sawComma || !trailingCommaCompatActive || !nextTokenIsCloseBrace)
            return false;
        if (!compatMessageEmitted)
            compatMessageEmitted = true;
        return true;
    }

    public static string FormatTrailingCommaCompatMessage() =>
        "HCDE: tolerated trailing comma in MAPINFO text list for known mod compatibility.";

    public static void AssignMustGetString(ref string field, string? token) =>
        field = token ?? string.Empty;
}
