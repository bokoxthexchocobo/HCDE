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

    public const int MfUnmorphed = 1;

    public static bool PlayerSubValid(bool oldNull, bool newNull, bool same, bool oldPawn, bool newPawn) =>
        !oldNull && !newNull && !same && oldPawn && newPawn;

    public static bool RewritePlayerMo(bool moMatch) => moMatch;

    public static bool RewriteSectorSoundTarget(bool match) => match;

    public readonly record struct MorphSubContext(
        bool FromNull,
        bool ToNull,
        bool Same,
        bool ToHasPlayer,
        int FromFlags,
        bool FromAltNull,
        bool ToAltNull,
        bool FromIsPlayerPawn,
        bool ToIsPlayerPawn,
        bool FromPlayerNull,
        bool FromMoIsFrom);

    public static bool MorphSubstitutionBlocked(MorphSubContext ctx)
    {
        if (ctx.FromNull || ctx.ToNull || ctx.Same || ctx.ToHasPlayer) return true;
        if ((ctx.FromFlags & MfUnmorphed) != 0) return true;
        if (ctx.FromAltNull && !ctx.ToAltNull) return true;
        if (!ctx.FromAltNull && ctx.ToAltNull && ctx.FromAltNull != ctx.ToAltNull) return true;
        return false;
    }

    public static bool PlayerMorphRequiresValidMo(MorphSubContext ctx) =>
        ctx.FromIsPlayerPawn && (!ctx.ToIsPlayerPawn || ctx.FromPlayerNull || !ctx.FromMoIsFrom);

    public static bool MorphInventoryOwnedBlocked(
        bool toIsPlayer,
        bool fromHasPlayer,
        bool fromIsInventoryOwned,
        bool toIsInventoryOwned) =>
        toIsPlayer
        || fromHasPlayer
        || fromIsInventoryOwned
        || toIsInventoryOwned;

    public static bool MorphNonPlayerPathAllowed(MorphSubContext ctx, bool inventoryBlocked) =>
        !MorphSubstitutionBlocked(ctx) && !PlayerMorphRequiresValidMo(ctx) && !inventoryBlocked;

    public static bool ShouldRunPreMorph(bool fromHasAlternative) => !fromHasAlternative;
}
