namespace HCDE.Playsim;

/// <summary><c>PIT_CheckLine</c> movement iterator from <c>p_map.cpp</c> (839–1116).</summary>
public static class VmPMapPitCheckLine
{
    public const int MfMissile = 1;
    public const int Mf6Stepmissile = 1;
    public const int Mf3Floorhugger = 1;
    public const int Mf2Blasted = 1;
    public const int MfDropoff = 1;
    public const int MfNogravity = 1;
    public const int MfNoclip = 1;
    public const int Mf8Crosslinecheck = 1;
    public const int Mf8Blockasplayer = 1;
    public const int MlBlockeverything = 1;
    public const int MlBlockprojectile = 2;
    public const int MlBlockPlayers = 4;
    public const int MlBlocking = 8;
    public const int MlRailing = 16;
    public const int FfcfNofloor = 1;
    public const int FfcfNoceiling = 2;
    public const int WallfPolyobj = 1;
    public const int BounceMbf = 1;

    public static bool SkipWhenBoxMissesLine(bool inRange, int boxSide) =>
        !inRange || boxSide != -1;

    public static bool IsOneSided(bool hasBacksector) => !hasBacksector;

    public static bool RecordPolyobjPortalHit(bool isPortal, bool polyWall) => isPortal && polyWall;

    public static bool PortalLayerIgnoresBlock(int portalFlags, int lineAbove, int lineBelow) =>
        ((portalFlags & FfcfNofloor) != 0 && lineAbove != 0)
        || ((portalFlags & FfcfNoceiling) != 0 && lineBelow != 0);

    public static bool IsStepMissile(int flags, int flags6, int flags3) =>
        (flags & MfMissile) != 0 && (flags6 & Mf6Stepmissile) == 0 && (flags3 & Mf3Floorhugger) == 0;

    public static bool CanStepCeilingPortal(
        bool isMissile,
        bool blocksMovement,
        double actorZ,
        double maxStep,
        double portalZ,
        double floorZ) =>
        !isMissile && !blocksMovement
        && actorZ < portalZ && actorZ + maxStep >= portalZ && floorZ < portalZ;

    public static bool BlastedActorDamagesSelf(int flags2) => (flags2 & Mf2Blasted) != 0;

    public static int BlastedDamageAmount(int mass) => mass >> 5;

    public static bool TreatAsProjectile(int flags, int bounceFlags) =>
        (flags & MfMissile) != 0 || (bounceFlags & BounceMbf) != 0;

    public static uint ProjectileBlockingMask(int flags8) =>
        (uint)(MlBlockeverything | MlBlockprojectile
               | ((flags8 & Mf8Blockasplayer) != 0 ? MlBlockPlayers | MlBlocking : 0));

    public static bool LineBlocksProjectile(uint lineFlags, uint mask) => (lineFlags & mask) != 0;

    public static bool IsRailingLine(int lineFlags) => (lineFlags & MlRailing) != 0;

public static bool PortalAboveSetsCeiling(int state, bool wasfit) => state == 1 && wasfit;

    public static double LowerCeilingToPortal(double portalZ, double currentCeiling) =>
        portalZ < currentCeiling ? portalZ : currentCeiling;

    public static bool PortalBelowSetsFloor(int state, bool wasfit) => state == 1 && wasfit;

    public static double RaiseFloorToPortal(double portalZ, double currentFloor) =>
        portalZ > currentFloor ? portalZ : currentFloor;

    public static bool SteepSlopeDropoffBlocked(
        int thingFlags,
        double frontC,
        double backC,
        double maxSteepness) =>
        (thingFlags & MfDropoff) == 0
        && (thingFlags & (MfNogravity | MfNoclip)) == 0
        && (frontC < maxSteepness) != (backC < maxSteepness);

    public static bool CrossLineCheckFails(int flags8, bool canCross) =>
        (flags8 & Mf8Crosslinecheck) != 0 && !canCross;

    public static bool RecalcFloorWhenPlanesMatch(bool samePlane, double openBottom, double atPosition) =>
        samePlane && openBottom > double.MinValue && atPosition < openBottom;

    public static bool RailingRaisesFloor(
        bool rail,
        bool compatRailing,
        double openBottom,
        double sectorFloorAtRef) =>
        rail && (!compatRailing || openBottom == sectorFloorAtRef);

    public static double RailingFloorBump(double openBottom) => openBottom + 32;

    public static bool AdjustCeilingWhenLower(double openTop, double ceilingZ) => openTop < ceilingZ;

    public static bool AdjustFloorWhenHigher(double openBottom, double floorZ) => openBottom > floorZ;

    public static bool MergeMidtexFlags(bool sameFloorZ, bool touchA, bool touchB) =>
        sameFloorZ && (touchA || touchB);

    public static bool UpdateDropoffWhenLower(double lowFloor, double dropoffZ) => lowFloor < dropoffZ;

public static bool PortalStepSetsGroup(bool portalStep, int portalGroup, int sectorGroup) =>
        portalStep && portalGroup == sectorGroup;

    public static bool MissileIgnoresPortalFloorStep(bool isMissile, bool sectorBlocksCeiling) =>
        !isMissile && !sectorBlocksCeiling;

public static bool ActorBelowPortalTop(double actorZ, double portalZ) => actorZ < portalZ;

    public static bool ActorReachesPortalWithStep(double actorZ, double step, double portalZ) =>
        actorZ + step >= portalZ;

    public static bool FloorBelowPortal(double floorZ, double portalZ) => floorZ < portalZ;

public static bool DamageOnBlastedImpact(int flags2) => (flags2 & Mf2Blasted) != 0;

    public static bool BlockWhenProjectileAndLineBlocks(
        bool projectile,
        uint lineFlags,
        uint mask,
        bool blockedByLine) =>
        (!projectile || (lineFlags & mask) != 0) && blockedByLine;

public static bool OpeningBottomWidensAtPosition(double atPos, double openBottom) =>
        atPos < openBottom;

    public static bool CeilingAdjustUsesOpenTop(double openTop, double ceilingZ) => openTop < ceilingZ;

    public static bool FloorAdjustUsesOpenBottom(double openBottom, double floorZ) => openBottom > floorZ;

    public static bool TouchMidtexWhenSameFloor(bool sameFloor, bool touch) => sameFloor && touch;

    public static bool DropoffUsesPortalLayer(bool noCeilingFlag, bool dropoffIsPortal) =>
        !noCeilingFlag || dropoffIsPortal;

}
