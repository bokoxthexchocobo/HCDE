namespace HCDE.Playsim;

/// <summary>Bone world transform, sector transition, and water depth from <c>p_mobj.cpp</c> (4475–5340).</summary>
public static class VmPMobjBoneWorldTick
{
    public const int ModeldataGetBoneInfo = 1;
    public const int Mf7Handlenodelay = 1;
    public const int Mf2Dormant = 1;
    public const int SecspacExit = 1;
    public const int SecspacEnter = 1 << 1;
    public const int SecspacHitfloor = 1 << 2;
    public const int SecspacHitceiling = 1 << 3;
    public const int SecspacHitfakefloor = 1 << 4;
    public const int SecmfUnderwater = 1;
    public const int SecmfUnderwatermask = 1 << 1;
    public const int SecmfFakeflooronly = 1 << 2;
    public const int FfExists = 1;
    public const int FfSolid = 1 << 1;
    public const int FfSwimmable = 1 << 2;

    public static (double X, double Y, double Z) SwizzlePosition(double x, double y, double z) => (x, z, y);

    public static (double X, double Y, double Z) UnswizzlePosition(double x, double y, double z) => (x, z, y);

    public static (double X, double Y, double Z) NormalizeVector(double x, double y, double z)
    {
        var len = Math.Sqrt(x * x + y * y + z * z);
        if (len == 0) return (0, 0, 0);
        return (x / len, y / len, z / len);
    }

    public static bool EvaluateBonePositionGate(
        bool hasModelData,
        int flags,
        int modelIndex,
        int boneIndex,
        int modelSlots,
        int boneSlots,
        bool picnumValid) =>
        hasModelData
        && (flags & ModeldataGetBoneInfo) != 0
        && modelIndex >= 0
        && boneIndex >= 0
        && modelSlots > modelIndex
        && boneSlots > boneIndex
        && !picnumValid;

    public readonly record struct BoneWorldTransformContext(
        double PosX,
        double PosY,
        double PosZ,
        double FwdX,
        double FwdY,
        double FwdZ,
        double UpX,
        double UpY,
        double UpZ);

    public static (double Px, double Py, double Pz, double Fx, double Fy, double Fz, double Ux, double Uy, double Uz)
        ApplyBoneThenWorldSwizzle(BoneWorldTransformContext ctx)
    {
        var (px, py, pz) = SwizzlePosition(ctx.PosX, ctx.PosY, ctx.PosZ);
        var (fx, fy, fz) = SwizzlePosition(ctx.FwdX, ctx.FwdY, ctx.FwdZ);
        var (ux, uy, uz) = SwizzlePosition(ctx.UpX, ctx.UpY, ctx.UpZ);
        var (nfx, nfy, nfz) = NormalizeVector(fx, fy, fz);
        var (nux, nuy, nuz) = NormalizeVector(ux, uy, uz);
        var worldPos = UnswizzlePosition(px, py, pz);
        var worldFwd = UnswizzlePosition(nfx, nfy, nfz);
        var worldUp = UnswizzlePosition(nux, nuy, nuz);
        return (worldPos.X, worldPos.Y, worldPos.Z, worldFwd.X, worldFwd.Y, worldFwd.Z, worldUp.X, worldUp.Y, worldUp.Z);
    }

    public static bool GetObjectToWorldMatrixGate(bool hasModelData, int flags, bool picnumValid) =>
        hasModelData && (flags & ModeldataGetBoneInfo) != 0 && !picnumValid;

    public static int CopyMatrixFlat(double[] source, double[] dest)
    {
        if (source.Length < 16 || dest.Length < 16) return 0;
        for (var i = 0; i < 16; i++) dest[i] = source[i];
        return 16;
    }

    public static bool CheckNoDelayEligible(int flags7, int flags2) =>
        (flags7 & Mf7Handlenodelay) != 0 && (flags2 & Mf2Dormant) == 0;

    public static int ClearHandleNoDelayFlag(int flags7) => flags7 & ~Mf7Handlenodelay;

    public readonly record struct CheckNoDelayContext(
        int Flags7,
        int Flags2,
        bool StateNoDelay,
        bool ActionFreed,
        bool HasNewState);

    public static bool EvaluateCheckNoDelay(CheckNoDelayContext ctx)
    {
        if (!CheckNoDelayEligible(ctx.Flags7, ctx.Flags2)) return true;
        if (!ctx.StateNoDelay) return true;
        if (ctx.ActionFreed) return false;
        return !ctx.HasNewState || true;
    }

    public static bool SectorChanged(sector_t? oldSec, sector_t? newSec) => oldSec != newSec;

    public static int BuildEnterSectorActions(double z, double floorZ, double top, double ceilingZ, bool heightSec, double fakeFloorZ)
    {
        var act = SecspacEnter;
        if (z <= floorZ) act |= SecspacHitfloor;
        if (top >= ceilingZ) act |= SecspacHitceiling;
        if (heightSec && z == fakeFloorZ) act |= SecspacHitfakefloor;
        return act;
    }

public static bool Check3dFloorHitAtFloor(double z, double floorz) => z == floorz;

    public static bool Check3dCeilingHitAtTop(double top, double ceilingz) => top == ceilingz;

    public readonly record struct WaterDepthContext(
        bool SectorNull,
        int MoreFlags,
        double ActorHeight,
        double ActorZ,
        double ActorTop,
        bool Splash,
        bool HasHeightSec,
        double HeightSecFloorZ,
        int HeightSecMoreFlags,
        double HeightSecCeilingZ);

    public static (int Level, double Depth, double FloorHeight) EvaluateWaterDepth(WaterDepthContext ctx)
    {
        if (ctx.SectorNull) return (0, 0, double.NegativeInfinity);
        if ((ctx.MoreFlags & SecmfUnderwater) != 0)
            return (1, ctx.ActorHeight, double.NegativeInfinity);
        if (!ctx.HasHeightSec) return (0, 0, double.NegativeInfinity);
        var fh = ctx.HeightSecFloorZ;
        var depth = fh - ctx.ActorZ;
        if (ctx.Splash || (ctx.HeightSecMoreFlags & SecmfUnderwatermask) != 0)
        {
            if (depth <= 0
                && (ctx.HeightSecMoreFlags & SecmfFakeflooronly) == 0
                && ctx.ActorTop > ctx.HeightSecCeilingZ)
            {
                depth = ctx.ActorHeight;
            }
            return (depth > 0 ? 1 : 0, Math.Max(0, depth), fh);
        }
        return (0, 0, fh);
    }

    public static bool Skip3dFloor(int flags) => (flags & FfExists) == 0 || (flags & FfSolid) != 0;

    public static bool Treat3dFloorAsWater(bool splash, int flags, int alpha) =>
        (flags & FfSwimmable) != 0 || splash && alpha == 0;

    public readonly record struct sector_t(int Id);

    public static bool HeightSecUnderwater(int moreFlags, bool splash) =>
        splash || (moreFlags & SecmfUnderwatermask) != 0;

    public static bool RoverSubmergesActor(double ffTop, double ffBottom, double actorZ, double actorCenter) =>
        ffTop > actorZ && ffBottom <= actorCenter;

    public static double RoverWaterDepth(double ffTop, double actorZ) => ffTop - actorZ;

    public static double ClampNonNegativeDepth(double depth) => depth < 0 ? 0 : depth;

    public readonly record struct WaterLevelContext(
        double Depth,
        double Height,
        double ViewHeight,
        bool IsPlayer);

    public static int ClassifyWaterLevel(WaterLevelContext ctx)
    {
        var depth = ClampNonNegativeDepth(ctx.Depth);
        if (depth <= 0) return 0;
        if (depth <= ctx.Height / 2) return 1;
        if (ctx.IsPlayer && depth >= ctx.ViewHeight) return 3;
        if (depth >= ctx.Height) return 3;
        return 2;
    }

    public readonly record struct RoverWaterContext(
        int Flags,
        int Alpha,
        bool Splash,
        double FfTop,
        double FfBottom,
        double ActorZ,
        double ActorCenter);

    public static bool TryRoverWaterDepth(RoverWaterContext ctx, out double depth, out double floorHeight)
    {
        depth = 0;
        floorHeight = double.NegativeInfinity;
        if (Skip3dFloor(ctx.Flags)) return false;
        if (!Treat3dFloorAsWater(ctx.Splash, ctx.Flags, ctx.Alpha)) return false;
        if (!RoverSubmergesActor(ctx.FfTop, ctx.FfBottom, ctx.ActorZ, ctx.ActorCenter)) return false;
        floorHeight = ctx.FfTop;
        depth = RoverWaterDepth(ctx.FfTop, ctx.ActorZ);
        return true;
    }

    public static bool SplashOnBoomTransition(int boomWaterLevel, int waterLevel) =>
        boomWaterLevel == 0 && waterLevel != 0;
}
