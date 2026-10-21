namespace HCDE.Playsim;

/// <summary><c>P_TestMobjLocation</c>, <c>P_CheckOnmobj</c>, <c>P_TestMobjZ</c>, <c>P_FakeZMovement</c>, <c>CheckForPushSpecial</c> from <c>p_map.cpp</c> (2036–2358).</summary>
public static class VmPMapTestMobj
{
    public const int MfPickup = 1;
    public const int MfNoclip = 1;
    public const int Mf2Thruactors = 1;
    public const int MfSolid = 1;
    public const int MfSpecial = 2;
    public const int Mf6Touchy = 1;
    public const int MfCorpse = 1;
    public const int MfIcecorpse = 1;
    public const int Mf4Actlikebridge = 1;
    public const int MfMissile = 1;
    public const int MfFloat = 1;
    public const int MfSkullfly = 1;
    public const int MfNogravity = 1;

    public static bool ClearPickupForTest(int flags) => (flags & MfPickup) != 0;

    public static bool ZOkWhenPositionOk(double z, double floorz, double top, double ceilingz) =>
        z >= floorz && top <= ceilingz;

    public static bool QuickReturnNoclip(int flags, int flags2) =>
        (flags & MfNoclip) != 0 || (flags2 & Mf2Thruactors) != 0;

    public static bool SkipHigherObstacle(bool quick, bool higher) => quick && higher;

    public static bool CorpseNeedsIce(int flags, int actorFlags) =>
        (flags & MfCorpse) != 0 && (actorFlags & MfIcecorpse) == 0;

    public static bool BridgeBlocksPickup(bool bridge, bool special) => !bridge && special;

    public static bool OverOrUnder(double actorZ, double actorTop, double thingTop, double thingZ) =>
        actorZ > thingTop || actorTop <= thingZ;

    public static bool FloatChaseTarget(bool floatFlag, bool skullfly, bool inflight, double dist, double delta) =>
        floatFlag && !skullfly && !inflight && dist < Math.Abs(delta) * 3;

    public static bool NogravityBob(bool player, bool nogravity, double z, double floorz, bool noclip2) =>
        player && nogravity && z > floorz && !noclip2;

    public static bool ClipToFloor(double z, double floorz) => z <= floorz;

    public static bool ClipToCeiling(double top, double ceilingz, double height) =>
        top > ceilingz;

    public static bool PushWindowCompat(bool compat, bool hasBack) => compat && hasBack;

    public static bool WindowNotBlocking(double fzt, double fzb, double bzt, double bzb, double top, double z) =>
        fzt >= top && bzt >= top && fzb <= z && bzb <= z;

    public static bool MidtexBlocks3d(bool hasMidtex, double bot, double top, double actorTop, double actorZ) =>
        hasMidtex && bot < actorTop && top > actorZ;

public static bool MissilesActivateImpactLevel(bool levelFlag, bool missile, bool noTarget) =>
        levelFlag || !missile || noTarget;

    public static bool RestorePickupFlags(int flags, int saved) => flags == saved;

    public static bool CollidedWhenBlocked(bool blocked, bool predicting) => blocked && !predicting;

    public static bool OnmobjGood(bool good) => !good;

    public static bool SaveRestoreZ(double oldz, double newz) => oldz != newz;

public static bool ThruSpeciesMatch(bool thru, bool same) => thru && same;

    public static bool IronCorpseSkip(bool corpse, bool ice) => corpse && !ice;

    public static bool SpectralReflectContinue(bool reflect, bool missile) => reflect && missile;

    public static bool RipLevelContinue(bool rip, bool levelOk) => rip && levelOk;

public static bool AddVelocityZ(double vel) => vel != 0;

    public static bool SetZToFloor(double z, double floor) => z <= floor;

    public static bool SetZToCeiling(double top, double ceil, double h) => top > ceil;

public static bool ImpactUsesActivator(bool impact, bool missile, bool level, bool noTarget) =>
        impact && (level || !missile || noTarget);

    public static bool FfloorSolidExists(bool solid, bool exists) => solid && exists;

    public static bool FfloorBlocksWindow(double ffBot, double ffTop, double top, double z) =>
        ffBot < top && ffTop > z;

public static bool ImpactTargetRedirect(bool missile, bool target) => missile && target;
}
