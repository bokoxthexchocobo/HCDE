namespace HCDE.Playsim;

/// <summary><c>AActor::UnlinkFromWorld</c>, <c>FixMapthingPos</c>, <c>LinkToWorld</c> from <c>p_maputl.cpp</c> (256–534).</summary>
public static class VmPMapUtlActorWorldLink
{
    public const ulong MfNosector = 1UL << 0;
    public const ulong MfNoblockmap = 1UL << 1;
    public const ulong Mf4Fixmapthingpos = 1UL << 2;

    public sealed class LinkContext
    {
        public object? SectorList { get; set; }
        public object? RenderList { get; set; }
    }

    public sealed class ActorLinkState
    {
        public ulong Flags { get; set; }
        public ulong Flags4 { get; set; }
        public object? Sector { get; set; }
        public object? Subsector { get; set; }
        public object? Section { get; set; }
        public object? TouchingSectorList { get; set; }
        public object? TouchingRenderSectors { get; set; }
        public object? BlockNodeHead { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Radius { get; set; }
        public double RenderRadius { get; set; } = -1;
        public SectorThingListHead? SectorHead { get; set; }
        public SectorThingListHead? PrevHead { get; set; }
        public ActorLinkState? NextInSector { get; set; }
        public bool ClientSide { get; set; }
        public int PortalGroup { get; set; }
        public double Top { get; set; }
    }

    public sealed class SectorThingListHead
    {
        public ActorLinkState? Head { get; set; }
    }

    public sealed class BlockNode
    {
        public BlockNode? NextActor { get; set; }
        public BlockNode? PrevActorPtr { get; set; }
        public BlockNode? NextBlock { get; set; }
        public BlockNode? PrevBlockPtr { get; set; }
    }

    public sealed class UnlinkContext
    {
        public ActorLinkState Actor { get; set; } = new();
        public LinkContext? SaveCtx { get; init; }
        public Action<object?>? DeleteSecList { get; init; }
        public Action? ClearRenderSectorList { get; init; }
        public Action? ClearRenderLineList { get; init; }
        public Action<BlockNode>? ReleaseBlockNode { get; init; }
    }

    public static void UnlinkFromWorld(UnlinkContext ctx)
    {
        var actor = ctx.Actor;
        if (ctx.SaveCtx is not null)
            ctx.SaveCtx.SectorList = null;

        if ((actor.Flags & MfNosector) == 0)
        {
            if (actor.PrevHead is not null)
            {
                var next = actor.NextInSector;
                actor.PrevHead.Head = next;
                if (next is not null)
                    next.PrevHead = actor.PrevHead;
                actor.SectorHead = null;
                actor.PrevHead = null;
            }

            if (ctx.SaveCtx is not null)
            {
                ctx.SaveCtx.SectorList = actor.TouchingSectorList;
                ctx.SaveCtx.RenderList = actor.TouchingRenderSectors;
            }
            else
            {
                ctx.DeleteSecList?.Invoke(actor.TouchingSectorList);
                ctx.DeleteSecList?.Invoke(actor.TouchingRenderSectors);
            }

            actor.TouchingSectorList = null;
            actor.TouchingRenderSectors = null;
        }

        if ((actor.Flags & MfNoblockmap) == 0)
        {
            var block = actor.BlockNodeHead as BlockNode;
            while (block is not null)
            {
                if (block.NextActor is not null)
                    block.NextActor.PrevActorPtr = block.PrevActorPtr;
                if (block.PrevActorPtr is not null)
                    block.PrevActorPtr.NextActor = block.NextActor;
                var next = block.NextBlock;
                ctx.ReleaseBlockNode?.Invoke(block);
                block = next;
            }

            actor.BlockNodeHead = null;
        }

        ctx.ClearRenderSectorList?.Invoke();
        ctx.ClearRenderLineList?.Invoke();
    }

    public sealed class MapLineDef
    {
        public int Index { get; init; }
        public object? FrontSector { get; init; }
        public object? BackSector { get; init; }
        public bool SameSectorBothSides { get; init; }
        public bool CoplanarFloorsCeilings { get; init; }
        public double BboxLeft { get; init; }
        public double BboxRight { get; init; }
        public double BboxBottom { get; init; }
        public double BboxTop { get; init; }
        public double DeltaX { get; init; }
        public double DeltaY { get; init; }
    }

    public sealed class FixMapthingPosContext
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
        public object? StartSector { get; set; }
        public IReadOnlyList<MapLineDef> Lines { get; set; } = Array.Empty<MapLineDef>();
        public Func<double, double, double>? InterceptDistanceToLine { get; init; }
        public Action<double, double>? SetXy { get; init; }
        public Action? ClearInterpolation { get; init; }
    }

    public static bool FixMapthingPos(FixMapthingPosContext ctx)
    {
        var success = false;
        foreach (var ldef in ctx.Lines)
        {
            if (ldef.SameSectorBothSides)
                continue;
            if (ldef.BackSector is not null && ldef.CoplanarFloorsCeilings)
                continue;
            if (ctx.X + ctx.Radius <= ldef.BboxLeft
                || ctx.X - ctx.Radius >= ldef.BboxRight
                || ctx.Y + ctx.Radius <= ldef.BboxBottom
                || ctx.Y - ctx.Radius >= ldef.BboxTop)
                continue;

            var distance = ctx.InterceptDistanceToLine?.Invoke(ldef.Index, Math.Sqrt(ldef.DeltaX * ldef.DeltaX + ldef.DeltaY * ldef.DeltaY)) ?? double.MaxValue;
            if (distance >= ctx.Radius)
                continue;

            var useBack = ldef.BackSector is not null && ReferenceEquals(ldef.BackSector, ctx.StartSector);
            var angleDeg = Math.Atan2(ldef.DeltaY, ldef.DeltaX) * 180 / Math.PI + (useBack ? 90 : -90);
            var push = ctx.Radius - distance;
            var rad = angleDeg * Math.PI / 180;
            ctx.X += Math.Cos(rad) * push;
            ctx.Y += Math.Sin(rad) * push;
            ctx.SetXy?.Invoke(ctx.X, ctx.Y);
            ctx.ClearInterpolation?.Invoke();
            success = true;
        }

        return success;
    }

    public sealed class LinkToWorldContext
    {
        public ActorLinkState Actor { get; set; } = new();
        public LinkContext? RestoreCtx { get; init; }
        public bool SpawningMapthing { get; set; }
        public object? SectorHint { get; set; }
        public Func<FixMapthingPosContext, bool>? FixMapthingPos { get; init; }
        public Func<(double X, double Y, double Z), object?>? PointInSector { get; init; }
        public Func<double, double, object?>? PointInSectorBuggy { get; init; }
        public Func<(double X, double Y, double Z), object?>? PointInRenderSubsector { get; init; }
        public Action<ActorLinkState, object?>? InsertSectorThingList { get; init; }
        public Func<ActorLinkState, object?, object?>? CreateSecNodeList { get; init; }
        public Action<object?>? DeleteRenderSecList { get; init; }
        public Func<ActorLinkState, IReadOnlyList<int>>? CollectPortalGroups { get; init; }
        public Action<ActorLinkState, int, int, int>? LinkBlockCell { get; init; }
        public Action<ActorLinkState>? UpdateRenderSectorList { get; init; }
    }

    public static void LinkToWorld(LinkToWorldContext ctx)
    {
        var actor = ctx.Actor;
        var spawning = ctx.SpawningMapthing;
        if (spawning && (actor.Flags4 & Mf4Fixmapthingpos) != 0 && ctx.SectorHint is null)
        {
            var fix = new FixMapthingPosContext { X = actor.X, Y = actor.Y, Radius = actor.Radius, StartSector = null };
            if (ctx.FixMapthingPos?.Invoke(fix) == true)
                spawning = false;
        }

        var sector = ctx.SectorHint;
        if (sector is null)
        {
            sector = spawning
                ? ctx.PointInSectorBuggy?.Invoke(actor.X, actor.Y)
                : ctx.PointInSector?.Invoke((actor.X, actor.Y, actor.Z));
        }

        actor.Sector = sector;
        actor.Subsector = ctx.PointInRenderSubsector?.Invoke((actor.X, actor.Y, actor.Z));
        actor.Section = actor.Subsector;

        if ((actor.Flags & MfNosector) == 0 && !actor.ClientSide)
        {
            ctx.InsertSectorThingList?.Invoke(actor, sector);
            actor.TouchingSectorList = ctx.CreateSecNodeList?.Invoke(actor, ctx.RestoreCtx?.SectorList);
            if (actor.RenderRadius >= 0)
                actor.TouchingRenderSectors = ctx.CreateSecNodeList?.Invoke(actor, ctx.RestoreCtx?.RenderList);
            else
            {
                actor.TouchingRenderSectors = null;
                if (ctx.RestoreCtx is not null)
                    ctx.DeleteRenderSecList?.Invoke(ctx.RestoreCtx.RenderList);
            }
        }

        if ((actor.Flags & MfNoblockmap) == 0 && !actor.ClientSide)
        {
            actor.BlockNodeHead = null;
            var groups = ctx.CollectPortalGroups?.Invoke(actor) ?? Array.Empty<int>();
            foreach (var group in groups)
                ctx.LinkBlockCell?.Invoke(actor, group, 0, 0);
        }

        if (!ctx.SpawningMapthing)
            ctx.UpdateRenderSectorList?.Invoke(actor);
    }

    public static void SetOrigin(LinkContext ctx, ActorLinkState actor, double x, double y, double z, bool moving, Action<UnlinkContext>? unlink, Action<LinkToWorldContext>? link, Action? findFloorCeil, Action? clearInterpolation)
    {
        var unlinkCtx = new UnlinkContext { Actor = actor, SaveCtx = ctx };
        unlink?.Invoke(unlinkCtx);
        actor.X = x;
        actor.Y = y;
        actor.Z = z;
        var linkCtx = new LinkToWorldContext { Actor = actor, RestoreCtx = ctx };
        link?.Invoke(linkCtx);
        findFloorCeil?.Invoke();
        if (!moving)
            clearInterpolation?.Invoke();
    }
}
