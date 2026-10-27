namespace HCDE.Playsim;

/// <summary>Actor query NON-ACTION natives from <c>p_actionfunctions.cpp</c> (336–518).</summary>
public static class VmActionActorQueryNatives
{
    public const double DefaultCrouchFactor = 1.0;

    public static bool NativeCallWritesReturnValue(int numret) => numret > 0;

    public static double ResolveCrouchFactor(bool hasMobj, bool hasPlayer, double playerCrouchFactor) =>
        !hasMobj || !hasPlayer ? DefaultCrouchFactor : playerCrouchFactor;

    public static double MaterializeCrouchFactorQuery(
        bool numretPositive,
        bool hasMobj,
        bool hasPlayer,
        double factor) =>
        numretPositive ? ResolveCrouchFactor(hasMobj, hasPlayer, factor) : 0;

    public static int PlayerSlotForCvarLookup(bool hasPlayer, int playerNumber) =>
        hasPlayer ? playerNumber : -1;

    public static double CvarFloatOrZero(bool cvarFound, double value) =>
        cvarFound ? value : 0;

    public static double MaterializeCvarFloatQuery(
        bool numretPositive,
        bool hasPlayer,
        int playerNumber,
        bool cvarFound,
        double value)
    {
        if (!numretPositive)
            return 0;
        _ = PlayerSlotForCvarLookup(hasPlayer, playerNumber);
        return CvarFloatOrZero(cvarFound, value);
    }

    public static string CvarStringOrEmpty(bool cvarFound, string value) =>
        cvarFound ? value : string.Empty;

    public static string MaterializeCvarStringQuery(
        bool numretPositive,
        bool cvarFound,
        string value) =>
        numretPositive ? CvarStringOrEmpty(cvarFound, value) : string.Empty;

    public static int PlayerInputOrZero(bool hasMobj, bool hasPlayer, int inputValue) =>
        !hasMobj || !hasPlayer ? 0 : inputValue;

    public static int MaterializePlayerInputQuery(
        bool numretPositive,
        bool hasMobj,
        bool hasPlayer,
        int inputValue) =>
        numretPositive ? PlayerInputOrZero(hasMobj, hasPlayer, inputValue) : 0;

    public static bool ShouldQueryProximity(object? mobj) => mobj is not null;

    public static int ProximityCountWhenMobjMissing() => 0;

    public static int MaterializeCountProximityQuery(
        bool numretPositive,
        object? mobj,
        int proximityCount) =>
        !numretPositive ? 0 : ShouldQueryProximity(mobj) ? proximityCount : ProximityCountWhenMobjMissing();

    public static bool IsDecorateInternalInt(string funcName) =>
        funcName == "__decorate_internal_int__";

    public static bool IsDecorateInternalBool(string funcName) =>
        funcName == "__decorate_internal_bool__";

    public static bool IsDecorateInternalFloat(string funcName) =>
        funcName == "__decorate_internal_float__";

    public static bool IsDecorateInternalCast(string funcName) =>
        IsDecorateInternalInt(funcName) || IsDecorateInternalBool(funcName) || IsDecorateInternalFloat(funcName);

    public static int DecorateFloatReturnCode(bool numretPositive) =>
        numretPositive ? 1 : 0;

    public static double DecorateFloatValueWhenNumret(bool numretPositive, double returnme) =>
        numretPositive ? returnme : 0;
}
