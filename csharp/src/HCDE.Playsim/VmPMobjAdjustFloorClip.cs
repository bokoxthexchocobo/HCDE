namespace HCDE.Playsim;

/// <summary><c>AActor::AdjustFloorClip</c> from <c>p_mobj.cpp</c> (6006–6071).</summary>
public static class VmPMobjAdjustFloorClip
{
    public const int Mf3Specialfloorclip = 1;
    public const int FfSolid = 1;
    public const int FfExists = 1 << 1;
    public const int Maxplayers = 64;
    public const double NoFloorClip = double.MaxValue;

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

    public readonly record struct FloorTouchSector(
        bool HasHeightSec,
        double FloorZ,
        double ActorZ,
        double TerrainFootClip,
        IReadOnlyList<ThreeDFloorTouchContext> Floors);

    public static double ComputeShallowestClip(IEnumerable<FloorTouchSector> touches)
    {
        var shallowest = NoFloorClip;
        foreach (var touch in touches)
        {
            var terrainCtx = new SectorTouchContext(
                touch.HasHeightSec,
                touch.FloorZ,
                touch.ActorZ,
                touch.TerrainFootClip);
            TryTerrainClip(terrainCtx, ref shallowest);
            if (touch.HasHeightSec)
                continue;
            foreach (var floor in touch.Floors)
                Try3dFloorClip(floor, ref shallowest);
        }

        return shallowest;
    }

    public readonly record struct AdjustFloorClipResult(double FloorClip, double ViewHeight, double DeltaViewHeight);

    public static AdjustFloorClipResult RunAdjustFloorClip(
        int flags3,
        double oldClip,
        IEnumerable<FloorTouchSector> touches,
        bool isPlayer,
        bool isViewActor,
        double viewHeight,
        Func<double> getDeltaViewHeight)
    {
        if (SkipWhenSpecialFloorclip(flags3))
            return new AdjustFloorClipResult(oldClip, viewHeight, 0);

        var shallowest = ComputeShallowestClip(touches);
        var newClip = ResolveFloorClip(shallowest, shallowest < NoFloorClip);
        if (!PlayerViewAdjust(isPlayer, isViewActor, oldClip, newClip))
            return new AdjustFloorClipResult(newClip, viewHeight, 0);

        var adjustedView = AdjustViewHeight(viewHeight, oldClip, newClip);
        return new AdjustFloorClipResult(newClip, adjustedView, getDeltaViewHeight());
    }

    public const int MfUnmorphed = 1;

    public static bool PlayerSubValid(bool oldNull, bool newNull, bool same, bool oldPawn, bool newPawn) =>
        !oldNull && !newNull && !same && oldPawn && newPawn;

    public sealed class PlayerPointerFields
    {
        public object? Mo { get; set; }
        public object? Poisoner { get; set; }
        public object? Attacker { get; set; }
        public object? Camera { get; set; }
        public object? ConversationNpc { get; set; }
        public object? ConversationPc { get; set; }
    }

    public static void SubstitutePlayerPointers(PlayerPointerFields player, object oldPlayer, object newPlayer)
    {
        if (ReferenceEquals(player.Mo, oldPlayer))
            player.Mo = newPlayer;
        if (ReferenceEquals(player.Poisoner, oldPlayer))
            player.Poisoner = newPlayer;
        if (ReferenceEquals(player.Attacker, oldPlayer))
            player.Attacker = newPlayer;
        if (ReferenceEquals(player.Camera, oldPlayer))
            player.Camera = newPlayer;
        if (ReferenceEquals(player.ConversationNpc, oldPlayer))
            player.ConversationNpc = newPlayer;
        if (ReferenceEquals(player.ConversationPc, oldPlayer))
            player.ConversationPc = newPlayer;
    }

    public static void SubstituteSectorSoundTargets(IList<object?> sectors, object oldPlayer, object newPlayer)
    {
        for (var i = 0; i < sectors.Count; i++)
        {
            if (ReferenceEquals(sectors[i], oldPlayer))
                sectors[i] = newPlayer;
        }
    }

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

    public static bool EvaluateMorphSubstitution(
        MorphSubContext ctx,
        bool fromIsInventoryOwned,
        bool toIsInventoryOwned,
        out bool runPreMorph)
    {
        runPreMorph = false;
        if (MorphSubstitutionBlocked(ctx))
            return false;
        if (PlayerMorphRequiresValidMo(ctx))
            return false;
        if (MorphInventoryOwnedBlocked(ctx.ToIsPlayerPawn, ctx.ToHasPlayer, fromIsInventoryOwned, toIsInventoryOwned))
            return false;
        if (ctx.FromAltNull)
            runPreMorph = true;
        else
            runPreMorph = false;
        return true;
    }
}
