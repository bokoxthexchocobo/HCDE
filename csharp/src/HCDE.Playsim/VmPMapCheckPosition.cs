namespace HCDE.Playsim;

/// <summary><c>P_CheckPosition</c> and <c>P_DoMissileDamage</c> from <c>p_map.cpp</c> (1327–1366, 1817–2034).</summary>
public static class VmPMapCheckPosition
{
    public const int MfNoclip = 1;
    public const int MfPickup = 1;

    public static bool ShouldDealDamage(int damage, int flags6, int flags7) =>
        damage > 0 || (flags6 & 1) != 0 || (flags7 & 1) != 0;

    public static int HealFromNegativeDamage(int damage) => -damage;

    public static bool RipperUsesThreeSidedRoll(bool rip) => rip;

    public static bool ActorsOnlyOrNoclip(bool actorsonly, int flags) =>
        actorsonly || (flags & MfNoclip) != 0;

    public static bool GoodLineCheck(bool good, bool lineResult) => good & lineResult;

    public static bool PassablePortalCheck(bool passable, bool linked) => passable && !linked;

    public static bool HeightFit(double ceiling, double floor, double height) => ceiling - floor >= height;

    public static bool TouchMidtexResetsDropoff(bool touchmidtex) => touchmidtex;

    public static bool StepthingPreservesDropoff(bool hasStep) => hasStep;

    public static bool BlockingMobjClear(bool blockerNull) => blockerNull;

    public static bool UsePortalFloorCeiling(bool blocksCeiling, bool blocksFloor) =>
        !blocksCeiling || !blocksFloor;

    public static bool DropoffFromSectorFloor(double floorZ) => floorZ >= 0;

    public static bool IterateLinesUntilFail(bool good) => good;

    public static bool PortalDestinationProbe(bool passablePortal, bool portalResult) =>
        passablePortal && portalResult;

    public static bool RestoreRealHeight(double savedHeight) => savedHeight > 0;

    public static bool SpechitClearBeforeLines() => true;

    public static bool PortalhitClearBeforeLines() => true;

    public static bool FloorzFromDropoffMerge(double floorz, double dropoffz) => floorz >= dropoffz;

    public static bool MissileDamagePoisonBranch(bool poisonPositive, int duration) =>
        poisonPositive && duration != int.MinValue;

    public static bool BloodSplatterChance(int roll) => roll < 192;

    public static bool GiveBodyOnZeroDamage(int damage) => damage <= 0;

    public static bool PointInSector(bool found) => found;

    public static bool ClearBlockingLine(bool noclip2) => !noclip2;

    public static bool PortalBlocksBoth(bool ceiling, bool floor) => ceiling && floor;

    public static bool UseGetFloorCeilingZ(bool portal) => portal;

    public static bool SectorFloorAtPoint(double z) => z >= 0;

    public static bool ThingTopFor3d(double height, double z) => height > 0;

    public static bool FfloorBelowThing(bool below) => below;

    public static bool IterateThings(bool actorsonly) => !actorsonly;

    public static bool ThingblockerSet(bool nullBlocker) => nullBlocker;

    public static bool LineIteratorNext(bool hasNext) => hasNext;

    public static bool PortalPassableFlag(bool passable) => passable;

    public static bool LinkedPortalSkip(bool linked) => linked;

    public static bool CeilingBelowFloorFail(double c, double f, double h) => c - f < h;

    public static bool DropoffFromStepthing(bool step) => step;

    public static bool RestoreHeightAfter(bool restore) => restore;

    public static bool RipperSound(bool rip) => rip;

    public static bool TraceBleedOnDamage(int dealt, int damage) => dealt > 0 || damage > 0;

    public static bool PoisonWhenDurationValid(int dmg, int dur) => dmg > 0 && dur != int.MinValue;

    public static bool StrifeDamageRoll(bool strife) => strife;

    public static bool ForcePainEvenAtZero(bool force) => force;

    public static bool Midtex3dFloorScan(bool solid, bool exists) => solid && exists;

    public static bool SpechitPush(bool special) => special;

    public static bool PortalhitPush(bool portal) => portal;

    public static bool GoodStaysGood(bool good, bool result) => good && result;

    public static bool BlockingLineOnFail(bool wasfit) => wasfit;

    public static bool CheckPortalSetsBlock(bool portalOk) => portalOk;

    public static bool ActorOnlyIterator(bool only) => only;

    public static bool NoclipSkipsLines(bool noclip) => noclip;

    public static bool RealHeightRestore(double h) => h > 0;

    public static bool PcheckIteratorInit(bool ok) => ok;

    public static bool FloorPicFromSector(bool set) => set;

    public static bool TerrainFromSector(bool set) => set;
}
