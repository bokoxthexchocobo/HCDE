namespace HCDE.Playsim;

/// <summary><c>A_Respawn</c> from <c>p_actionfunctions.cpp</c> (2132–2288).</summary>
public static class VmActionRespawn
{
    public const int RsfFog = 1;
    public const int RsfKeepTarget = 2;
    public const int RsfTelefrag = 4;
    public const int MfSolid = 1 << 1;
    public const int MfFriendly = 1 << 26;
    public const int RfInvisible = 1 << 0;

    public static bool UseTelefragMove(int flags) => (flags & RsfTelefrag) != 0;

    public static bool ShouldClearTarget(int flags) => (flags & RsfKeepTarget) == 0;

    public static bool ShouldClearSelfTarget(object? target, object self) =>
        ReferenceEquals(target, self);

    public static bool ShouldSpawnFog(int flags) => (flags & RsfFog) != 0;

    public static int MergeFriendlyFlags(int defFlags, int selfFlags) =>
        (defFlags & ~MfFriendly) | (selfFlags & MfFriendly);

    public static bool RespawnSucceeded(bool positionOk) => positionOk;

    public static double RestoreDefaultHeight(double defaultHeight) => defaultHeight;

    public static double RestoreDefaultRadius(double defaultRadius) => defaultRadius;

    public static int RestoreDefaultHealth(int defaultHealth) => defaultHealth;

    public static int ApplySolidFlag(int flags) => flags | MfSolid;

    public static int ClearInvisibleRenderFlag(int renderFlags) => renderFlags & ~RfInvisible;

    public static bool ShouldIncrementKillCount(bool countsAsKill) => countsAsKill;

    public static int KeepHuntPlayersBit(int defFlags3, int selfFlags3, int huntBit) =>
        (defFlags3 & ~huntBit) | (selfFlags3 & huntBit);

    public static object? ClearTargetReference(object? target) => null;

    public static (bool ok, int health, int flags, int renderFlags) ApplyRespawnSuccess(
        bool positionOk,
        int defaultHealth,
        int defFlags,
        int selfFlags,
        int renderFlags)
    {
        if (!RespawnSucceeded(positionOk))
            return (false, 0, defFlags, renderFlags);

        return (
            true,
            RestoreDefaultHealth(defaultHealth),
            MergeFriendlyFlags(defFlags, selfFlags),
            ClearInvisibleRenderFlag(renderFlags));
    }

    public static bool ShouldClearLastHeard(int flags) => ShouldClearTarget(flags);

    public static bool ShouldClearLastEnemy(int flags, object? lastEnemy, object self) =>
        ShouldClearTarget(flags) || ShouldClearSelfTarget(lastEnemy, self);

    public static int CopyDefFlags2(int defFlags2) => defFlags2;

    public static int CopyDefFlags3(int defFlags3, int selfFlags3, int huntMask) =>
        (defFlags3 & ~huntMask) | (selfFlags3 & huntMask);

    public static int CopyDefFlags4(int defFlags4, int selfFlags4, int noHateMask) =>
        (defFlags4 & ~noHateMask) | (selfFlags4 & noHateMask);

    public static bool RespawnFailed(bool positionOk) => !RespawnSucceeded(positionOk);

    public static int CopyDefFlags5(int defFlags5) => defFlags5;
    public static int CopyDefFlags6(int defFlags6) => defFlags6;
    public static int CopyDefFlags7(int defFlags7) => defFlags7;
}
