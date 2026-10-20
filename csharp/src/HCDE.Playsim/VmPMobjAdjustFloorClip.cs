namespace HCDE.Playsim;

/// <summary><c>AActor::AdjustFloorClip</c> from <c>p_mobj.cpp</c> (6006–6071).</summary>
public static class VmPMobjAdjustFloorClip
{
    public const int Mf3Specialfloorclip = 1;
    public const int FfSolid = 1;
    public const int FfExists = 1 << 1;

    public static bool SkipWhenSpecialFloorclip(int flags3) => (flags3 & Mf3Specialfloorclip) != 0;

    public static bool TouchingFloorAtZ(double floorZ, double actorZ) => floorZ == actorZ;

    public static bool Valid3dFloor(int flags) => (flags & FfExists) != 0 && (flags & FfSolid) != 0;

    public static double PickShallowestClip(double current, double candidate) =>
        candidate < current ? candidate : current;

    public static double ResolveFloorClip(double shallowest, bool found) =>
        found ? shallowest : 0;

    public static double AdjustViewHeight(double viewHeight, double oldClip, double newClip) =>
        viewHeight - (oldClip - newClip);

    public readonly record struct SectorTouchContext(
        bool HasHeightSec,
        double FloorZ,
        double ActorZ,
        double TerrainFootClip);

    public static bool TryTerrainClip(SectorTouchContext ctx, ref double shallowest)
    {
        if (ctx.HasHeightSec) return false;
        if (!TouchingFloorAtZ(ctx.FloorZ, ctx.ActorZ)) return false;
        shallowest = PickShallowestClip(shallowest, ctx.TerrainFootClip);
        return true;
    }

    public readonly record struct ThreeDFloorTouchContext(
        int Flags,
        double TopZ,
        double ActorZ,
        double TerrainFootClip);

    public static bool Try3dFloorClip(ThreeDFloorTouchContext ctx, ref double shallowest)
    {
        if (!Valid3dFloor(ctx.Flags)) return false;
        if (!TouchingFloorAtZ(ctx.TopZ, ctx.ActorZ)) return false;
        shallowest = PickShallowestClip(shallowest, ctx.TerrainFootClip);
        return true;
    }

    public static bool PlayerViewAdjust(bool isPlayer, bool isViewActor, double oldClip, double newClip) =>
        isPlayer && isViewActor && oldClip != newClip;
}
