namespace HCDE.Playsim;

/// <summary>Exit, teleport, thrust specials from <c>p_lnspec.cpp</c> (227–239, 1067–1271).</summary>
public static class VmPLnspecExitTeleport
{
    public const int TelfDestFog = 1;
    public const int TelfFdCompat = 2;
    public const int TelfSourceFog = 4;
    public const int TelfKeepVelocity = 8;
    public const int TelfKeepOrientation = 16;
    public const int TelfRotateBoom = 32;
    public const int TelfRotateBoomInverse = 64;
    public const int TelfKeepHeight = 128;
    public const int GameStrife = 4;
    public const int Level2HexenHack = 1;
    public const double MaxMove = 30.0;

    public readonly record struct ExitInvoke(bool Good, int Position, bool Secret);

    public static ExitInvoke BuildExitNormal(bool good, int position) => new(good, position, false);

    public static ExitInvoke BuildExitSecret(bool good, int position) => new(good, position, true);

    public static bool ExitSucceeds(ExitInvoke invoke) => invoke.Good;

    public readonly record struct TeleportNewMapInvoke(
        bool AllowSide,
        bool HasInfo,
        bool GoodExit,
        int Position,
        bool KeepFacing);

    public static TeleportNewMapInvoke BuildTeleportNewMap(
        bool backSide,
        int gametype,
        object? info,
        bool goodExit,
        int position,
        int keepFacingArg) =>
        new(!backSide || gametype == GameStrife, info != null, goodExit, position, keepFacingArg != 0);

    public static int TeleportBaseFlags(bool noSourceFog) =>
        TelfDestFog | TelfFdCompat | (noSourceFog ? 0 : TelfSourceFog);

    public static int TeleportNoStopFlags(bool noSourceFog) =>
        TelfDestFog | TelfKeepVelocity | (noSourceFog ? 0 : TelfSourceFog);

    public static int TeleportNoFogOrientationFlags(int useAng, bool hasLine) => useAng switch
    {
        0 => TelfKeepOrientation,
        1 => 0,
        2 => hasLine ? TelfKeepOrientation | TelfRotateBoom : 0,
        3 => hasLine ? TelfKeepOrientation | TelfRotateBoomInverse : 0,
        _ => 0,
    };

    public static int ApplyKeepHeight(int flags, bool keepHeight) =>
        keepHeight ? flags | TelfKeepHeight : flags;

    public readonly record struct ZombieChangerInvoke(object? Activator, int Health, bool RanTeleport);

    public static bool ZombieChangerShouldPain(int health) => health >= 0;

    public static bool TeleportEndGame(bool backSide, bool exitGood) => !backSide && exitGood;

    public static bool TeleportOtherFog(bool fogArg) => fogArg;

    public static bool TeleportGroupMoveSource(bool moveSource) => moveSource;

    public static bool TeleportGroupFog(bool fog) => fog;

    public static bool TeleportSectorFog(bool fog) => fog;

    public static (double vx, double vy) ThrustClampVelocity(double vx, double vy, bool noLimit)
    {
        if (noLimit)
            return (vx, vy);
        return (
            Math.Clamp(vx, -MaxMove, MaxMove),
            Math.Clamp(vy, -MaxMove, MaxMove));
    }

    public static void ThrustThingHelper(double vx, double vy, double angleDeg, double force, bool noLimit, out double outVx, out double outVy)
    {
        var rad = angleDeg * Math.PI / 180.0;
        var nx = vx + Math.Cos(rad) * force;
        var ny = vy + Math.Sin(rad) * force;
        (outVx, outVy) = ThrustClampVelocity(nx, ny, noLimit);
    }

    public readonly record struct ThrustThingPlan(bool ByTid, int Tid, bool HexenBacksideReject, bool HasActivator);

    public static ThrustThingPlan BuildThrustThing(int tid, bool hexenHack, bool backSide, object? activator) =>
        tid != 0
            ? new(true, tid, false, true)
            : new(false, 0, hexenHack && backSide, activator != null);

    public static bool ThrustThingSucceeds(ThrustThingPlan plan) =>
        plan.ByTid || (plan.HasActivator && !plan.HexenBacksideReject);

    public static double ThrustThingZRaw(int arg1) => arg1 / 4.0;

    public static double ThrustThingZSigned(double thrust, bool invert) => invert ? -thrust : thrust;

    public readonly record struct ThrustZPlan(bool ByTid, int Tid, bool SetMode, double Thrust, object? Activator);

    public static ThrustZPlan BuildThrustZ(int tid, int zThrust, bool downUp, bool setMode, object? activator)
    {
        var raw = ThrustThingZRaw(zThrust);
        return new(tid != 0, tid, setMode, ThrustThingZSigned(raw, downUp), activator);
    }

    public static double ApplyThrustZ(double currentZ, ThrustZPlan plan) =>
        plan.SetMode ? currentZ + plan.Thrust : plan.Thrust;

    public static bool ThrustZSucceeds(ThrustZPlan plan) => plan.ByTid || plan.Activator != null;

    public static bool PolyStopSoundWhenFound(object? poly) => poly != null;

    public readonly record struct TeleportOtherInvoke(int OtherTid, int DestTid, bool Fog);

    public static TeleportOtherInvoke BuildTeleportOther(int a0, int a1, int a2) => new(a0, a1, a2 != 0);

    public readonly record struct TeleportGroupInvoke(
        int GroupTid,
        int SourceTid,
        int DestTid,
        bool MoveSource,
        bool Fog);

    public static TeleportGroupInvoke BuildTeleportGroup(int a0, int a1, int a2, int a3, int a4) =>
        new(a0, a1, a2, a3 != 0, a4 != 0);

    public readonly record struct TeleportSectorInvoke(int Tag, int SourceTid, int DestTid, bool Fog, int GroupTid);

    public static TeleportSectorInvoke BuildTeleportSector(int a0, int a1, int a2, int a3, int a4) =>
        new(a0, a1, a2, a3 != 0, a4);

    public static int CombineTeleportFlags(int baseFlags, int extra) => baseFlags | extra;

    public static bool TeleportNewMapChangeLevel(bool good, bool hasInfo) => good && hasInfo;

    public static int TeleportLineDestId(int arg1) => arg1;

    public static bool TeleportLineReversed(int arg2) => arg2 != 0;

    public static double ByteAngleToDegrees(int byteAngle) => byteAngle * (360.0 / 256.0);

    public static bool ThrustThingIteratorNonEmpty(bool any) => any;

    public static double ThrustThingForce(int arg1) => arg1;

    public static bool ThrustThingNoLimit(int arg2) => arg2 != 0;

    public static int ThrustThingTid(int arg3) => arg3;

    public static bool ThrustZDownUp(int arg2) => arg2 != 0;

    public static bool ThrustZSetMode(int arg3) => arg3 != 0;

    public static int ThrustZTid(int arg0) => arg0;

    public static bool ExitNormalCallsExitLevel(ExitInvoke invoke) => invoke.Good && !invoke.Secret;

    public static bool ExitSecretCallsSecret(ExitInvoke invoke) => invoke.Good && invoke.Secret;

    public static bool TeleportGroupReturns(bool ok) => ok;

    public static bool TeleportOtherReturns(bool ok) => ok;

    public static bool TeleportSectorReturns(bool ok) => ok;

    public static bool TeleportZombieRan(bool ran) => ran;

    public static int TeleportNewMapKeepFacingMask(int arg2) => arg2 != 0 ? 1 : 0;

    public static bool ChangeLevelNullMapName(bool endGame) => endGame;

    public static double ThrustHelperPreservesZ() => 0;

    public static bool PolyobjStopReturnsTrue() => true;

    public static bool ExitPositionValid(int pos) => pos >= 0;

    public static bool SecretExitPositionValid(int pos) => pos >= 0;

    public static int TeleportNoFogCase1() => 0;

    public static int TeleportNoFogCaseDefault(int useAng) => useAng == 1 ? 0 : 0;

    public static bool ThrustZIteratorNonEmpty(bool any) => any;

    public static bool ThrustZActivatorPath(bool hasIt) => hasIt;

    public static bool ThrustThingHexenReject(bool hexen, bool back) => hexen && back;
}
