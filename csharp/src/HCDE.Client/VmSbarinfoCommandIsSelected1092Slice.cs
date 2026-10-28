namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawBar</c> member tail (layout only in band) plus <c>CommandIsSelected</c> from
/// <c>sbarinfo_commands.cpp</c> (**2851–3000**). Chunk **1092**; **1091** owns <c>AdditionalData</c> ctor (**2852–2854**).
/// </summary>
public static class VmSbarinfoCommandIsSelected1092Slice
{
    public static (int? Weapon0, int? Weapon1) CreateIsSelectedWeaponSlotDefaults() => (null, null);

    public static bool ShouldReplaceIsSelectedWeaponClass(bool classFound, bool isWeaponKind) =>
        !classFound || !isWeaponKind;

    public static (bool RequireSecondToken, bool UseStringConstFallback) ResolveIsSelectedFirstWeaponTokenShape(
        bool identifierPresent) =>
        identifierPresent
            ? (RequireSecondToken: false, UseStringConstFallback: false)
            : (RequireSecondToken: true, UseStringConstFallback: true);

    public static (bool ParseSecondWeapon, bool RequireStringOrIdentifier) ResolveIsSelectedSecondWeaponParse(
        bool commaAfterFirstWeapon) =>
        commaAfterFirstWeapon
            ? (ParseSecondWeapon: true, RequireStringOrIdentifier: true)
            : (ParseSecondWeapon: false, RequireStringOrIdentifier: false);

    public static bool ResolveIsSelectedFlowControlTruth(
        bool readyWeaponPresent,
        bool slot0MatchesReady,
        bool secondSlotConfigured,
        bool slot1MatchesReady) =>
        readyWeaponPresent
        && (slot0MatchesReady || (secondSlotConfigured && slot1MatchesReady));
}

/// <summary>
/// <c>CommandPlayerClass</c> parse/tick from the same native band (**2929–2978**).
/// </summary>
public static class VmSbarinfoCommandPlayerClass1092Slice
{
    public static bool TryMatchPlayerClassDisplayName(string token, string registryDisplayName) =>
        string.Equals(token, registryDisplayName, StringComparison.OrdinalIgnoreCase);

    public static (bool ContinueClassList, bool RequireNextIdentifier) ResolvePlayerClassParseListAdvance(
        bool commaAfterEntry) =>
        commaAfterEntry
            ? (ContinueClassList: true, RequireNextIdentifier: true)
            : (ContinueClassList: false, RequireNextIdentifier: false);

    public static bool ResolvePlayerClassFlowControlTruthFromScan(
        bool playerClassMissing,
        IReadOnlyList<int> listedClassIds,
        int spawnClassId)
    {
        if (playerClassMissing)
            return false;
        for (var i = 0; i < listedClassIds.Count; i++)
        {
            if (listedClassIds[i] == spawnClassId)
                return true;
        }
        return false;
    }
}

/// <summary>
/// <c>CommandPlayerType</c> parse head from the same native band (**2985–3000**).
/// </summary>
public static class VmSbarinfoCommandPlayerType1092Slice
{
    public static (bool MustReadIdentifier, bool AcceptedStringConst) ResolvePlayerTypeParseHeadToken(
        bool         stringConstPresent
            ? (MustReadIdentifier: false, AcceptedStringConst: true)
            : (MustReadIdentifier: true, AcceptedStringConst: false);

    public static int ResolvePlayerTypeClassListCountAfterLookup(
        int priorCount,
        bool classLookupSucceeded) => classLookupSucceeded ? priorCount + 1 : priorCount;

    public static (bool ContinueTypeList, bool RequireNextToken) ResolvePlayerTypeParseListAdvance(
        bool         commaAfterEntry
            ? (ContinueTypeList: true, RequireNextToken: true)
            : (ContinueTypeList: false, RequireNextToken: false);
}
