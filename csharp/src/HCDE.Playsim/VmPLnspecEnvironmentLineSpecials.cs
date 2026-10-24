namespace HCDE.Playsim;

/// <summary>Misc env/portal specials from <c>p_lnspec.cpp</c> (3155–3543).</summary>
public static class VmPLnspecEnvironmentLineSpecials
{
    public const int SecPartCeiling = 1;
    public const int SecPartFloor = 0;
    public const int SecPart3d = 2;

    public readonly record struct PortalInvoke(int ThisId, int DestId);

    public static PortalInvoke PortalTarget(int a0, int a1) => new(a0, a1);

    public static float ReflectionScale(int raw) => raw / 255f;

    public readonly record struct ReflectionInvoke(float Floor, float Ceiling, bool SkipFloorSlope, bool SkipCeilingSlope);

    public static ReflectionInvoke BuildReflection(int floorArg, int ceilingArg, bool floorSlope, bool ceilingSlope) =>
        new(
            floorSlope ? 0 : ReflectionScale(floorArg),
            ceilingSlope ? 0 : ReflectionScale(ceilingArg),
            floorSlope,
            ceilingSlope);

    public enum GlobalFogKind
    {
        Density,
        OutsideDensity,
        SkyFog,
    }

    public static GlobalFogKind? ResolveFogKind(int type) => type switch
    {
        0 => GlobalFogKind.Density,
        1 => GlobalFogKind.OutsideDensity,
        2 => GlobalFogKind.SkyFog,
        _ => null,
    };

    public static int ApplyGlobalFog(GlobalFogKind kind, int value) => kind switch
    {
        GlobalFogKind.SkyFog => value,
        _ => FogDensityFromArg(value),
    };

    public static int FogDensityFromArg(int value) => value >> 1;

    public static bool GlobalFogRejectUnknown(int type) => ResolveFogKind(type) is null;

    public readonly record struct GlowInvoke(int Tag, int Height, int R, int G, int B, bool Ceiling);

    public static GlowInvoke BuildGlow(int tag, int height, int r, int g, int b, bool ceiling) =>
        new(tag, height, r, g, b, ceiling);

    public static bool GlowInvalidates(int height) => height < 0;

    public static int LineHealthClamped(int health) => health < 0 ? 0 : health;

    public static int SectorHealthClamped(int health) => health < 0 ? 0 : health;

    public static bool SectorHealthCeiling(int part) => part == SecPartCeiling;

    public static bool SectorHealthFloor(int part) => part == SecPartFloor;

    public static bool SectorHealth3d(int part) => part == SecPart3d;

    public static bool LineHealthGroupUpdate(bool hasGroup) => hasGroup;

    public static bool SectorHealthGroupUpdate(bool hasGroup) => hasGroup;

    public static int NextSkillFromArg(int arg0, int skillCount) =>
        (uint)arg0 >= (uint)skillCount ? -1 : arg0;

    public static bool AutosaveAllowed(int gameAction, int gaSave, int gaQuick) =>
        gameAction != gaSave && gameAction != gaQuick;

    public static bool AutosaveClearsHint(int flags2, int hintMask) => (flags2 & hintMask) == 0;

    public static object? ResolveNoiseTarget(int tid, object? activator, object? iteratorNext) =>
        tid == 0 ? activator : iteratorNext;

    public static object? ResolveNoiseEmitter(int emitterTid, int targetTid, object? activator, object? target, object? iteratorNext) =>
        emitterTid == 0 ? activator : emitterTid == targetTid ? target : iteratorNext;

    public static bool CommunicatorRejectBackside(bool frontOnly, bool backSide) => frontOnly && backSide;

    public static bool CommunicatorRequiresPlayer(object? activator, object? player) =>
        activator != null && player != null;

    public static string CommunicatorSoundName(int voc) => $"svox/voc{voc}";

    public static string CommunicatorSubtitleKey(int id) => $"TXT_COMM{id}";

    public static bool CommunicatorShouldSetLog(bool noLog) => !noLog;

    public static bool SectorHealthAppliesToPart(int part) =>
        SectorHealthCeiling(part) || SectorHealthFloor(part) || SectorHealth3d(part);

    public static bool NoiseAlertRequiresTarget(object? target) => target != null;

    public static bool AutosaveWritesDemHint() => true;

    public static int SkyFogRaw(int value) => value;

    public static bool PortalInvokeValid(PortalInvoke invoke) => invoke.ThisId >= 0;

    public static float GlowHeight(float height) => height;

    public static bool ChangeSkillValid(int nextSkill) => nextSkill >= 0;
}
