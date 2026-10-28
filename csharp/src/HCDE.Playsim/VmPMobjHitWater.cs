namespace HCDE.Playsim;

/// <summary><c>P_GetThingFloorType</c> and <c>P_HitWater</c> from <c>p_mobj.cpp</c> (7475–7656).</summary>
public static class VmPMobjHitWater
{
    public const int CfPredicting = 1;
    public const int MfMissile = 1;
    public const int FfExists = 1;
    public const int FfSolid = 1 << 1;
    public const int FfSwimmable = 1 << 2;
    public const int SecmfClipfakeplanes = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf3Dontsplash = 1;
    public const int ThwSmall = 1;
    public const int ThwNovel = 1 << 1;

    public static int ResolveThingFloorType(int floorTerrain, Func<int> sectorFloorTerrain)
    {
        if (UseActorFloorTerrain(floorTerrain))
            return floorTerrain;
        return sectorFloorTerrain();
    }

    public static bool UseActorFloorTerrain(int floorTerrain) => floorTerrain >= 0;

    public static bool SkipWhenPredicting(bool hasPlayer, int cheats)
    {
        if (!hasPlayer)
            return false;
        return (cheats & CfPredicting) != 0;
    }

    public static double CompareZForCheckAbove(double centerZ, double velZ, bool isMissile)
    {
        if (isMissile)
            return centerZ - velZ;
        return centerZ;
    }

    public static bool CheckAboveCenter(bool checkAbove, double posZ, double compareZ)
    {
        if (!checkAbove)
            return false;
        return posZ > compareZ;
    }

    public static bool Valid3dFloor(int flags) => (flags & FfExists) != 0;

    public static bool Splashable3dTop(int flags, double alpha)
    {
        if ((flags & (FfSolid | FfSwimmable)) != 0)
            return true;
        return alpha > 0;
    }

    public static bool ZNearPlane(double posZ, double planeZ) =>
        posZ > planeZ - 0.5 && posZ < planeZ + 0.5;

    public static bool BlockBelowBottomPlane(double bottomPlaneZ, double posZ, double actorFloorZ)
    {
        if (bottomPlaneZ >= posZ)
            return false;
        return bottomPlaneZ >= actorFloorZ;
    }

    public readonly record struct F3dFloorTerrainProbe(int Flags, double Alpha, double TopPlaneZ, double BottomPlaneZ, int TerrainNum);

    /// <summary>3D-floor walk from <c>P_HitWater</c> when <c>!force</c> (7539–7554).</summary>
    public static bool? SelectTerrainFrom3dFloors(double posZ, double actorFloorZ, IReadOnlyList<F3dFloorTerrainProbe> floors, out int terrainNum)
    {
        terrainNum = 0;
        for (var i = 0; i < floors.Count; i++)
        {
            var rover = floors[i];
            if (!Valid3dFloor(rover.Flags))
                continue;

            if (ZNearPlane(posZ, rover.TopPlaneZ))
            {
                if (Splashable3dTop(rover.Flags, rover.Alpha))
                {
                    terrainNum = rover.TerrainNum;
                    return true;
                }

                continue;
            }

            if (BlockBelowBottomPlane(rover.BottomPlaneZ, posZ, actorFloorZ))
                return false;
        }

        return null;
    }

    public static bool UseHeightSecTerrain(bool force, bool hasHeightSec, int moreFlags)
    {
        if (force)
            return false;
        if (!hasHeightSec)
            return false;
        return (moreFlags & SecmfClipfakeplanes) == 0;
    }

    public static int SelectFloorTerrain(bool force, bool hasHeightSec, int moreFlags, Func<int> sectorFloor, Func<int> heightSecFloor)
    {
        if (UseHeightSecTerrain(force, hasHeightSec, moreFlags))
            return heightSecFloor();
        return sectorFloor();
    }

    public static bool NoSplashDefinition(int splashNum) => splashNum == -1;

    public static bool DealDamageOnLand(bool isPlayer, bool damageOnLand, int damageAmount, int timeMask, int levelTime)
    {
        if (!isPlayer || !damageOnLand || damageAmount <= 0)
            return false;
        return (levelTime & timeMask) != 0;
    }

    public static bool SkipUnderwaterFloor(bool underwater, double posZ, double floorZ)
    {
        if (!underwater)
            return false;
        return posZ <= floorZ;
    }

    public static bool SkipNovelLowVelocity(int flags, bool monsterOrPlayer, double velZ, bool force)
    {
        if ((flags & ThwNovel) != 0)
            return false;
        if (!monsterOrPlayer)
            return false;
        if (force)
            return false;
        return velZ >= -6;
    }

    public static bool SmallSplashMass(int flags, int mass)
    {
        if ((flags & ThwSmall) != 0)
            return true;
        return mass < 10;
    }

    public static bool AllowSplash(int flags3) => (flags3 & Mf3Dontsplash) == 0;

    public static bool UseSmallSplashDef(bool smallSplash, bool hasSmallClass)
    {
        if (!smallSplash)
            return false;
        return hasSmallClass;
    }

    public static bool ClientSideSmallSplash(bool isClientSide, bool defaultClientSide)
    {
        if (!isClientSide)
            return true;
        return defaultClientSide;
    }

    public static bool LiquidWithoutSplash(int splashNum, bool isLiquid) =>
        splashNum == -1 && isLiquid;

    public readonly record struct PortalFloorWalkContext(
        double DirZ,
        double PosZ,
        double PortalPlaneZ,
        bool BlocksMovement);

    public static bool WalkDownThroughFloorPortal(PortalFloorWalkContext ctx)
    {
        if (ctx.DirZ >= 0)
            return false;
        if (ctx.BlocksMovement)
            return false;
        return ctx.PosZ < ctx.PortalPlaneZ;
    }

    public static bool WalkUpThroughCeilingPortal(PortalFloorWalkContext ctx)
    {
        if (ctx.DirZ <= 0)
            return false;
        if (ctx.BlocksMovement)
            return false;
        return ctx.PosZ > ctx.PortalPlaneZ;
    }

    public readonly record struct HitWater3dProbeContext(
        int Flags,
        double Alpha,
        double PosZ,
        double TopPlaneZ,
        double BottomPlaneZ,
        double ActorFloorZ);

    public static bool TrySplashAt3dTop(HitWater3dProbeContext ctx)
    {
        if (!Valid3dFloor(ctx.Flags))
            return false;
        if (!Splashable3dTop(ctx.Flags, ctx.Alpha))
            return false;
        return ZNearPlane(ctx.PosZ, ctx.TopPlaneZ);
    }

    public static bool RejectBelowBottom(HitWater3dProbeContext ctx) =>
        BlockBelowBottomPlane(ctx.BottomPlaneZ, ctx.PosZ, ctx.ActorFloorZ);

    public readonly record struct SplashSpawnContext(
        bool AllowSplash,
        bool SmallSplash,
        bool HasSmallDef,
        bool IsClientSide,
        bool DefaultClientSide);

    public static bool SpawnSmallSplash(SplashSpawnContext ctx)
    {
        if (!ctx.AllowSplash)
            return false;
        if (!UseSmallSplashDef(ctx.SmallSplash, ctx.HasSmallDef))
            return false;
        return ClientSideSmallSplash(ctx.IsClientSide, ctx.DefaultClientSide);
    }

    public static bool SpawnRegularSplash(SplashSpawnContext ctx)
    {
        if (!ctx.AllowSplash)
            return false;
        return !ctx.SmallSplash;
    }

    public static bool ReturnLiquidOnly(int splashNum, bool isLiquid) =>
        LiquidWithoutSplash(splashNum, isLiquid);

    public static bool ReturnLiquidOnHeightSecPlane(bool onSectorFloorPlane, bool isLiquid)
    {
        if (onSectorFloorPlane)
            return false;
        return isLiquid;
    }
}
