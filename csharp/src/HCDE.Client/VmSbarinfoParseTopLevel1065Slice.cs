namespace HCDE.Client;

/// <summary>
/// Custom lump scan tail and <c>ParseSBarInfo</c> top-level commands from <c>sbarinfo.cpp</c> (**451–600**).
/// Chunk **1065**; follows **1064** load head (**432–450**). Slot create/parse delegates to <see cref="VmSbarinfoMainBlockLoad1064Slice"/>.
/// </summary>
public static class VmSbarinfoParseTopLevel1065Slice
{
    public const int BaseLumpUnset = -2;
    public const int GameTypeAny = 0;
    public const int GameTypeStrife = 8;

    public enum BaseGameNameKind
    {
        Unknown,
        Doom,
        Heretic,
        Hexen,
        Strife,
        None
    }

    public enum SpacingAlignment
    {
        Left,
        Center,
        Right
    }

    public static bool ShouldContinueCustomLumpScan(int lumpNum) => lumpNum != -1;

    public static VmSbarinfoMainBlockLoad1064Slice.DefaultScriptAction ResolveCustomSlotAction(bool slotIsNull) =>
        VmSbarinfoMainBlockLoad1064Slice.ResolveScriptSlotAction(slotIsNull);

    public static bool ShouldRecurseParsedInclude(bool tokenIsInclude, int resolvedLumpNum) =>
        tokenIsInclude && !ShouldFailMissingIncludeLump(resolvedLumpNum);

    public static bool ShouldFailMissingIncludeLump(int lumpNum) =>
        VmSbarinfoMainBlockLoad1064Slice.IsMissingLump(lumpNum);

    public static bool ShouldAcceptTopLevelToken(bool isIdentifier, bool isInclude) => isIdentifier || isInclude;

    public static BaseGameNameKind ClassifyBaseGameName(string name)
    {
        if (name.Equals("Doom", StringComparison.OrdinalIgnoreCase))
            return BaseGameNameKind.Doom;
        if (name.Equals("Heretic", StringComparison.OrdinalIgnoreCase))
            return BaseGameNameKind.Heretic;
        if (name.Equals("Hexen", StringComparison.OrdinalIgnoreCase))
            return BaseGameNameKind.Hexen;
        if (name.Equals("Strife", StringComparison.OrdinalIgnoreCase))
            return BaseGameNameKind.Strife;
        if (name.Equals("None", StringComparison.OrdinalIgnoreCase))
            return BaseGameNameKind.None;
        return BaseGameNameKind.Unknown;
    }

    public static string? StandardBaseRelativePath(BaseGameNameKind kind) => kind switch
    {
        BaseGameNameKind.Doom => "sbarinfo/doom.txt",
        BaseGameNameKind.Heretic => "sbarinfo/heretic.txt",
        BaseGameNameKind.Hexen => "sbarinfo/hexen.txt",
        _ => null
    };

    public static int? ResolveGameTypeOverride(BaseGameNameKind kind) => kind switch
    {
        BaseGameNameKind.Strife => GameTypeStrife,
        BaseGameNameKind.None => GameTypeAny,
        _ => null
    };

    public static bool ShouldLoadStandardBaseAfterLookup(int baselumpAfterLookup) =>
        baselumpAfterLookup != BaseLumpUnset;

    public static bool ShouldFailMissingStandardBase(int baselumpAfterLookup) =>
        baselumpAfterLookup == -1;

    public static bool ShouldFatalOnCoreBaseOverride(int fileContainer) => fileContainer > 0;

    public static bool TopLevelCommandIsBase(int commandIndex) =>
        commandIndex == (int)VmSbarinfoMainBlockLoad1064Slice.TopLevelKeywordId.Base;

    public static int MergeStatusBarHeight(int priorHeight, int parsedHeight, bool parsedIntPresent) =>
        parsedIntPresent ? parsedHeight : priorHeight;

    public static bool ResolveTopLevelBool(bool checkTrueHit, bool validatedFalseToken)
    {
        if (checkTrueHit)
            return true;
        if (validatedFalseToken)
            return false;
        throw new ArgumentException("Top-level bool requires true or validated false token.");
    }

    public static (bool Enabled, bool HasSpeed, int Speed) BuildInterpolateHealthState(
        bool checkTrueHit,
        bool validatedFalseToken,
        bool hasCommaSpeed,
        int speedValue)
    {
        var enabled = ResolveTopLevelBool(checkTrueHit, validatedFalseToken);
        return (enabled, hasCommaSpeed, hasCommaSpeed ? speedValue : 0);
    }

    public static (bool Enabled, bool HasSpeed, int Speed) BuildInterpolateArmorState(
        bool checkTrueHit,
        bool validatedFalseToken,
        bool hasCommaSpeed,
        int speedValue) =>
        BuildInterpolateHealthState(checkTrueHit, validatedFalseToken, hasCommaSpeed, speedValue);

    public static bool ResolveCompleteBorder(bool checkTrueHit, bool validatedFalseToken) =>
        ResolveTopLevelBool(checkTrueHit, validatedFalseToken);

    public static char ResolveMonospaceSpacingCharacter(bool trueBranch, char stringConstFirstChar) =>
        trueBranch ? stringConstFirstChar : '\0';

    public static SpacingAlignment? TryClassifySpacingAlignment(string token)
    {
        if (token.Equals("left", StringComparison.OrdinalIgnoreCase))
            return SpacingAlignment.Left;
        if (token.Equals("center", StringComparison.OrdinalIgnoreCase))
            return SpacingAlignment.Center;
        if (token.Equals("right", StringComparison.OrdinalIgnoreCase))
            return SpacingAlignment.Right;
        return null;
    }

    public static SpacingAlignment? TryParseOptionalSpacingAlignment(bool hasAlignmentComma, string alignmentToken) =>
        hasAlignmentComma ? TryClassifySpacingAlignment(alignmentToken) : null;
}
