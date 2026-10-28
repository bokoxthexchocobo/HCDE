namespace HCDE.Playsim;

/// <summary>
/// ACS sound/string callfuncs from <c>p_acs.cpp</c> (**6150–6299**); chunk **943** (avoids recognition islands at **6000–6077**).
/// </summary>
public static class VmPAcsCallFuncSoundString943Slice
{
    public const int SdfFixedZoff = 1 << 0;
    public const int SdfFixedDistance = 1 << 1;

    public static bool MinArgCountMet(int argCount, int required) => argCount >= required;

    public static bool SoundVolumeUsesActivator(int tid) => tid == 0;

    public static bool StringCompareSameIndex(int indexA, int indexB) => indexA == indexB;

    public static string NullLookupToEmpty(string? value) => value ?? string.Empty;

    public static int StringCompareWithOptionalLength(
        int funcIsCaseSensitive,
        string a,
        string b,
        int argCount,
        int lengthArg)
    {
        if (argCount > 2)
            return funcIsCaseSensitive != 0
                ? string.CompareOrdinal(a, 0, b, 0, lengthArg)
                : string.Compare(a, 0, b, 0, lengthArg, StringComparison.OrdinalIgnoreCase);
        return funcIsCaseSensitive != 0
            ? string.CompareOrdinal(a, b)
            : string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public static int TrimSubstringLength(int oldLen, int requestedLen) =>
        oldLen < requestedLen ? oldLen : requestedLen;

    public static int StrMidPosClamped(int pos, int oldLen) => pos >= oldLen ? -1 : pos;

    public static int StrMidLengthClamped(int pos, int newLen, int oldLen)
    {
        if (pos + newLen > oldLen || pos + newLen < pos)
            return oldLen - pos;
        return newLen;
    }

    public static bool GetWeaponReturnsNone(object? activator, object? readyWeapon) =>
        activator is null || readyWeapon is null;

    public static bool SpawnDecalUsesActivator(int tid) => tid == 0;

    public static double SpawnDecalZOffset(int argCount, int flags, int zArg) =>
        argCount > 4 && (flags & SdfFixedZoff) != 0 ? zArg / 65536.0 : zArg;

    public static double SpawnDecalDistance(int argCount, int flags, int distArg, int defaultDist) =>
        argCount > 5
            ? (flags & SdfFixedDistance) != 0 ? distArg / 65536.0 : distArg
            : defaultDist;

    public static bool SpawnDecalTemplateMissing(object? template) => template is null;

    public static bool CheckFontFound(object? font) => font is not null;

    public static bool StrLeftUsesLeftSide(int funcIndex) => funcIndex == 0;

    public static string TakeSubstring(string oldstr, int start, int length) =>
        oldstr.Substring(start, length);

    public static int AccumulateSpawnDecalCount(int current, int added) => current + added;

    public static bool ActivatorRequiredForSpawn(int tid, object? activator) =>
        tid == 0 && activator is null;

    public static int ResolveEmptyStringReturn(bool isEmpty)
    {
        if (!isEmpty)
            return -1;
        return 0;
    }
}
