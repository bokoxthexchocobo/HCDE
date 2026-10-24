namespace HCDE.Playsim;

/// <summary>
/// <c>SpawnSubMissile</c> / <c>SpawnPlayerMissile</c> VM wrappers and
/// <c>P_SpawnPlayerMissile</c> from <c>p_mobj.cpp</c> (8112–8362).
/// </summary>
public static class VmPMobjSpawnPlayerMissile
{
    public const int AlfIgnoreNoAutoaim = 1;
    public const int AlfNoweaponcheck = 1 << 1;
    public const int WifNoautoaim = 1;
    public const int Mf3Floorhugger = 1;
    public const int Mf3Ceilinghugger = 1 << 1;
    public const int Mf4Spectral = 1;
    public const double OnFloorZ = VmPMobjSpawnMissileVariants.OnFloorZ;
    public const double OnCeilingZ = VmPMobjSpawnMissileVariants.OnCeilingZ;
    public static readonly double[] AutoAimAngleOffsetsDeg = [-5.625, 5.625, 0];

    public static int SanitizeAimFlags(int aimFlags) => aimFlags & ~AlfIgnoreNoAutoaim;

    public static bool RejectNullSpawn(bool sourceNull, bool typeNull) => sourceNull || typeNull;

    public static bool ShouldSuppressLocalCoopSpawn(bool suppress) => suppress;

    public static bool UseWeaponAimLock(
        bool hasPlayer,
        bool hasReadyWeapon,
        int weaponFlags,
        bool noAutoAim,
        int aimFlags) =>
        (aimFlags & AlfNoweaponcheck) == 0
        && hasPlayer
        && hasReadyWeapon
        && ((weaponFlags & WifNoautoaim) != 0 || noAutoAim);

    public static double LineTargetRange(double maxTargetRangeBlocks) =>
        maxTargetRangeBlocks > 0 ? maxTargetRangeBlocks * 64 : 16 * 64;

    public static bool ShouldBreakAutoAimLoop(
        bool hasPlayer,
        bool noFreeAim,
        bool freelookAllowed,
        double aimDist) =>
        hasPlayer && !noFreeAim && freelookAllowed && aimDist <= 0.5;

    public static bool CustomSpawnZ(double z) => z != OnFloorZ && z != OnCeilingZ;

    public static double ClampSpawnZToFloor(double z, double floorZ) => z < floorZ ? floorZ : z;

    public static bool HuggerUsesAngleVelocity(int flags3) =>
        (flags3 & (Mf3Floorhugger | Mf3Ceilinghugger)) != 0;

    public static bool ApplySpectralFriend(int flags4) => (flags4 & Mf4Spectral) != 0;

    public readonly record struct PlayerMissileAimResult(double Yaw, double Pitch, bool HasLineTarget);

    public static PlayerMissileAimResult ResolvePlayerMissileAim(
        bool weaponAimLock,
        double inputAngle,
        double sourcePitch,
        bool noFreeAim,
        bool freelookAllowed,
        Func<int, bool> tryAimAtOffset,
        bool hasLineTargetAfterLoop)
    {
        if (weaponAimLock)
            return new(inputAngle, sourcePitch, false);

        var index = 2;
        var yaw = inputAngle;
        var pitch = 0.0;
        var found = false;
        while (index >= 0)
        {
            if (tryAimAtOffset(index))
            {
                found = true;
                break;
            }

            index--;
        }

        if (!found && !hasLineTargetAfterLoop)
        {
            yaw = inputAngle;
            pitch = noFreeAim || !freelookAllowed ? 0 : sourcePitch;
        }

        return new(yaw, pitch, found || hasLineTargetAfterLoop);
    }

    public static double AdjustMissileZForAttackOffset(
        double z,
        double center,
        double floorClip,
        double attackOffset) =>
        z + center - floorClip + attackOffset;

    public static bool SpawnPlayerMissileBlocked(bool checkSpawnOk) => !checkSpawnOk;

    public static bool DefaultSpawnAngle(double angle) => angle == 1e37;
}
