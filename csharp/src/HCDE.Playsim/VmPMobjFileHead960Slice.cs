namespace HCDE.Playsim;

/// <summary>
/// Water macros, <c>sv_gravity</c> cvar hook, and early <c>AActor</c> GC pointer registration from <c>p_mobj.cpp</c> (**1–150**).
/// Exclusive to chunk **960**; does not overlap chunk **728** tick or **210+** serialize bands.
/// </summary>
public static class VmPMobjFileHead960Slice
{
    public const double WaterSinkFactor = 0.125;
    public const double WaterSinkSmallFactor = 0.25;
    public const double WaterSinkSpeed = 0.5;
    public const double WaterJumpSpeed = 3.5;
    public const double ZUnderflow = -32768.0;
    public const float DefaultSvGravity = 800f;

    public static bool IsNonFiniteGravity(double value) =>
        double.IsNaN(value) || double.IsInfinity(value);

    public static float SanitizeSvGravity(float value) =>
        IsNonFiniteGravity(value) ? DefaultSvGravity : value;

    public static float ResolveSvGravityCvar(
        float proposed,
        IEnumerable<object> levels,
        Action<object, float> setLevelGravity)
    {
        var sanitized = SanitizeSvGravity(proposed);
        ApplySvGravityToAllLevels(levels, setLevelGravity, sanitized);
        return sanitized;
    }

    public static void ApplySvGravityToAllLevels(
        IEnumerable<object> levels,
        Action<object, float> setLevelGravity,
        float gravity)
    {
        foreach (var level in levels)
            setLevelGravity(level, gravity);
    }

    public static double AdvanceWaterSink(double currentZ, bool smallActor, double speedScale = 1.0)
    {
        var factor = smallActor ? WaterSinkSmallFactor : WaterSinkFactor;
        return currentZ - (factor * WaterSinkSpeed * speedScale);
    }

    public static double WaterJumpVelocity(double multiplier = 1.0) => WaterJumpSpeed * multiplier;

    public static bool ClampsBelowZUnderflow(double z) => z < ZUnderflow;

    public const bool DefaultClMissileDecals = true;
    public const bool DefaultAddRocketExplosion = true;
    public const int DefaultClPuffType = 0;
    public const int DefaultClBloodType = 0;

    public static bool RegistersAActorGcPointer(string fieldName) =>
        fieldName is "target"
            or "lastenemy"
            or "tracer"
            or "damagesource"
            or "goal"
            or "LastLookActor";

    public static int AActorGcPointerBandCountThrough150() => 6;

    public static IReadOnlyList<string> AActorGcPointerRegistrationOrderThrough150() =>
    [
        "target",
        "lastenemy",
        "tracer",
        "damagesource",
        "goal",
        "LastLookActor",
    ];
}
