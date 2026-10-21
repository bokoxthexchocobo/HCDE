namespace HCDE.Playsim;

/// <summary><c>CheckIfTargetInLOS</c> guards from <c>p_actionfunctions.cpp</c> (2564–2650).</summary>
public static class VmActionTargetInLos
{
    public const int JlosfProjectile = 1 << 0;
    public const int JlosfNosight = 1 << 1;
    public const int JlosfClosenojump = 1 << 4;
    public const int JlosfDeadnojump = 1 << 5;
    public const int JlosfCheckmaster = 1 << 6;
    public const int JlosfTargetlos = 1 << 7;
    public const int JlosfAllynojump = 1 << 9;
    public const int JlosfCombatantonly = 1 << 10;
    public const int JlosfChecktracer = 1 << 12;
    public const int JlosfClosenofov = 1 << 2;
    public const int JlosfClosenosight = 1 << 3;
    public const int MfMissile = 1 << 2;
    public const int Mf2Seekermissile = 1 << 2;
    public const int Mf3IsMonster = 1 << 14;

    public static bool FailWhenNoTarget(object? target) => target is null;

    public static bool FailWhenDeadNoJump(int flags, int targetHealth) =>
        (flags & JlosfDeadnojump) != 0 && targetHealth <= 0;

    public static bool ShouldCheckSight(int flags) => (flags & JlosfNosight) == 0;

    public static bool FailCombatantOnly(int flags, bool targetIsPlayer, int targetFlags3) =>
        (flags & JlosfCombatantonly) != 0 && !targetIsPlayer && (targetFlags3 & Mf3IsMonster) == 0;

    public static bool FailAllyNoJump(int flags, bool isFriend) =>
        (flags & JlosfAllynojump) != 0 && isFriend;

    public static bool BeyondDistMax(double distance, double distMax) =>
        distMax != 0 && distance > distMax;

    public static bool WithinDistClose(double distance, double distClose) =>
        distClose != 0 && distance < distClose;

    public static bool CloseNoJump(int flags) => (flags & JlosfClosenojump) != 0;

    public static bool CloseClearsFov(int flags) => (flags & JlosfClosenofov) != 0;

    public static bool CloseClearsSight(int flags) => (flags & JlosfClosenosight) != 0;

    public static bool UseTargetLosViewport(int flags) => (flags & JlosfTargetlos) != 0;

    public static double ClampFovDegrees(double fovDegrees) => Math.Min(fovDegrees, 360.0);

    public static bool OutsideHalfFov(double absAngleDiffDegrees, double fovDegrees) =>
        fovDegrees > 0 && absAngleDiffDegrees > fovDegrees / 2.0;
}
