namespace HCDE.Playsim;

/// <summary><c>P_GetFriction</c>, <c>P_GetMoveFactor</c>, and portal line helpers from <c>p_map.cpp</c> (650–798, 813–823).</summary>
public static class VmPMapGetFriction
{
    public const double OrigFriction = 0xE800 / 65536.0;
    public const double OrigFrictionFactor = 2048 / 65536.0;
    public const double FlyingFriction = 0xEB00 / 65536.0;
    public const double MoreFrictionVelocity = 15000 / 65536.0;

    public const int Mf2Fly = 1;
    public const int MfNogravity = 1;
    public const int MfNoclip = 1;
    public const int SecfFriction = 64;
    public const int FfExists = 1;
    public const int FfSwimmable = 2;
    public const int FfSolid = 4;

    public static (double Friction, double MoveFactor) Defaults() => (OrigFriction, OrigFrictionFactor);

    public static bool SelectFlying(bool fly, bool noGravity) => fly && noGravity;

    public static (double Friction, double MoveFactor) FlyingPair() => (FlyingFriction, OrigFrictionFactor);

    public static bool InDeepWater(int waterlevel) => waterlevel > 1;

    public static bool ShallowWaterAboveFloor(int waterlevel, double actorZ, double floorZ) =>
        waterlevel == 1 && actorZ > floorZ + 6;

    public static bool UsesWaterFriction(int waterlevel, double actorZ, double floorZ, bool noGravity) =>
        !noGravity && (InDeepWater(waterlevel) || ShallowWaterAboveFloor(waterlevel, actorZ, floorZ));

    public static double HalveMoveFactor(double movefactor) => movefactor * 0.5;

    public static bool ActorInsideSwimmableFloor(double actorZ, double actorTop, double topZ, double bottomZ) =>
        actorZ <= topZ && actorTop >= bottomZ;

    public static bool PreferLowerFriction(double candidate, double current) =>
        candidate < current || current == OrigFriction;

    public static bool VarFrictionGrounded(bool varFriction, int flags, bool noClip, bool noGravity) =>
        varFriction && (flags & (MfNoclip | MfNogravity)) == 0;

    public static bool SkipSectorWithoutFriction(int sectorFlags, double terrainFriction) =>
        (sectorFlags & SecfFriction) == 0 && terrainFriction == 0;

    public static bool ActorOnSectorFloor(double actorZ, double floorZ, bool hasHeightSec, double heightSecFloorZ) =>
        actorZ <= floorZ || (hasHeightSec && actorZ <= heightSecFloorZ);

    public static (double Friction, double MoveFactor) ApplyActorFrictionMultiplier(
        double friction,
        double movefactor,
        double actorFriction)
    {
        if (Math.Abs(actorFriction - 1) < 1e-9)
            return (friction, movefactor);
        var scaled = Math.Clamp(friction * actorFriction, 0, 1);
        return (scaled, FrictionToMoveFactor(scaled));
    }

    public static double FrictionToMoveFactorIce(double friction) =>
        (((0x10092 - friction * 65536) * 1024) / 4352 + 568) / 65536.0;

    public static double FrictionToMoveFactorMud(double friction) =>
        (((friction * 65536.0 - 0xDB34) * 0xA) / 0x80) / 65536.0;

    public static double FrictionToMoveFactor(double friction) =>
        friction >= OrigFriction ? FrictionToMoveFactorIce(friction) : FrictionToMoveFactorMud(friction);

    public static double BoostMoveFactorForVelocity(double movefactor, double velocity)
    {
        if (velocity > MoreFrictionVelocity * 4) return movefactor * 8;
        if (velocity > MoreFrictionVelocity * 2) return movefactor * 4;
        if (velocity > MoreFrictionVelocity) return movefactor * 2;
        return movefactor;
    }

    public static double ResolveMoveFactor(double friction, double movefactor, double velocity) =>
        friction < OrigFriction ? BoostMoveFactorForVelocity(movefactor, velocity) : movefactor;

    public static int LineIsAbovePortalBlocked(bool blocksMovement) => blocksMovement ? -1 : 0;

    public static int LineIsAboveCompare(double portalFloorZ, double actorTop) =>
        portalFloorZ >= actorTop ? 1 : 0;

    public static int LineIsBelowPortalBlocked(bool blocksMovement) => blocksMovement ? -1 : 0;

    public static int LineIsBelowCompare(double portalCeilingZ, double actorZ) =>
        portalCeilingZ <= actorZ ? 1 : 0;
}
