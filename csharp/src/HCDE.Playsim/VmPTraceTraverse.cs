namespace HCDE.Playsim;

/// <summary><c>FTraceInfo::TraceTraverse</c> orchestration from <c>p_trace.cpp</c> (776–944).</summary>
public static class VmPTraceTraverse
{
    public const int TraceHitNone = 0;
    public const int TraceHitFloor = 1;
    public const int TraceHitCeiling = 2;
    public const int TracePortalRestrict = 1 << 4;
    public const int PorttLinked = 1;
    public const int FfExists = 1;

    public sealed class FFloorRover
    {
        public int Flags { get; set; }
        public bool IsLiquid { get; set; }
        public double TopZAtHit { get; set; }
        public double BottomZAtHit { get; set; }
        public object? Rover { get; set; }
    }

    public sealed class InterceptStep
    {
        public bool IsLine { get; set; }
        public double Frac { get; set; }
        public bool IsLinePortal { get; set; }
        public bool OnLineSidePrecise { get; set; }
        public bool EnterSectorNull { get; set; }
        public double EnterFloorZ { get; set; }
        public double EnterCeilingZ { get; set; }
        public int PortalType { get; set; }
        public uint ActorFlags { get; set; }
        public bool IsIgnoredThing { get; set; }
        public bool LineCheckPasses { get; set; } = true;
        public bool ThingCheckPasses { get; set; } = true;
        public bool BlocksFloorPortal { get; set; } = true;
        public bool BlocksCeilingPortal { get; set; } = true;
        public double HitZ { get; set; }
        public double PortalFloorZ { get; set; }
        public double PortalCeilingZ { get; set; }
        public double VecZ { get; set; }
        public double StartZ { get; set; }
        public double LimitZ { get; set; }
        public int SectorNum { get; set; }
        public IReadOnlyList<FFloorRover>? SectorFFloors { get; set; }
        public double SectorFloorZAtHit { get; set; }
        public Func<FFloorRover, bool>? Check3DFloorPlane { get; init; }
    }

    public sealed class TraceTraverseContext
    {
        public IReadOnlyList<InterceptStep> Steps { get; set; } = Array.Empty<InterceptStep>();
        public double MaxDist { get; set; }
        public int HitTypeNone { get; set; } = TraceHitNone;
        public int CurrentHitType { get; set; }
        public double LimitZ { get; set; }
        public uint ActorMask { get; set; } = 0xffffffff;
        public int TraceFlags { get; set; }
        public int LastSplashSector { get; set; } = -1;
        public object? Crossed3DWater { get; set; }
        public double Distance { get; set; }
        public bool PortalBlocksFloor { get; set; }
        public bool PortalBlocksCeiling { get; set; }
        public bool HeightSecPresent { get; set; }
        public double HeightSecFloorZAtStart { get; set; }
        public double HeightSecFloorZAtHit { get; set; }
        public double StartZ { get; set; }
        public object? CrossedWater { get; set; }
        public object? Ffloor { get; set; }
        public double SectorFloorZAtHit { get; set; }
        public double SectorCeilingZAtHit { get; set; }
        public double HitPosZ { get; set; }
        public Action? Setup3DFloors { get; init; }
        public Action<InterceptStep>? EnterLinePortal { get; init; }
        public Action<InterceptStep>? EnterSectorPortalFloor { get; init; }
        public Action<InterceptStep>? EnterSectorPortalCeiling { get; init; }
        public Func<InterceptStep, bool>? LineCheck { get; init; }
        public Func<InterceptStep, bool>? ThingCheck { get; init; }
        public Func<bool>? CheckSectorPlaneFloor { get; init; }
        public Func<bool>? CheckSectorPlaneCeiling { get; init; }
        public Func<bool>? CheckHeightSecPlane { get; init; }
        public Action<int>? AssignHitFloor { get; init; }
        public Action<int>? AssignHitCeiling { get; init; }
        public Action? AssignSector { get; init; }
        public Action? FinalizeMiss { get; init; }
        public Action? AssignSectorOnMiss { get; init; }
        public Action? SetSourcePosition { get; init; }
        public Action<object?>? AssignFfloor { get; init; }
        public Action<object?>? RecordCrossed3DWater { get; init; }
        public Action<object?>? RecordCrossedWater { get; init; }
    }

    public static bool LiquidRoverExists(FFloorRover rover) =>
        (rover.Flags & FfExists) != 0 && rover.IsLiquid;

    public static bool TryRecord3DWaterSplash(
        TraceTraverseContext ctx,
        InterceptStep step,
        Func<FFloorRover, bool>? checkPlane)
    {
        if (ctx.Crossed3DWater != null || ctx.LastSplashSector == step.SectorNum)
            return false;
        var rovers = step.SectorFFloors;
        if (rovers == null)
            return false;
        foreach (var rover in rovers)
        {
            if (!LiquidRoverExists(rover))
                continue;
            if (checkPlane?.Invoke(rover) != true)
                continue;
            if (rover.TopZAtHit <= step.SectorFloorZAtHit)
                continue;
            ctx.RecordCrossed3DWater?.Invoke(rover.Rover);
            ctx.Distance = 0;
            ctx.LastSplashSector = step.SectorNum;
            return true;
        }
        ctx.LastSplashSector = step.SectorNum;
        return false;
    }

    public static bool TryEnterFloorPortal(InterceptStep step, ref double limitZ)
    {
        if (step.VecZ >= 0 || step.BlocksFloorPortal)
            return false;
        if (step.HitZ >= step.PortalFloorZ || limitZ <= step.PortalFloorZ)
            return false;
        limitZ = step.PortalFloorZ;
        return true;
    }

    public static bool TryEnterCeilingPortal(InterceptStep step, ref double limitZ)
    {
        if (step.VecZ <= 0 || step.BlocksCeilingPortal)
            return false;
        if (step.HitZ <= step.PortalCeilingZ || limitZ >= step.PortalCeilingZ)
            return false;
        limitZ = step.PortalCeilingZ;
        return true;
    }

    public static bool LinePortalHeightOk(InterceptStep step) =>
        !step.EnterSectorNull
        && step.HitZ >= step.EnterFloorZ
        && step.HitZ <= step.EnterCeilingZ;

    public static bool LinePortalAllowed(int portalType, int traceFlags) =>
        portalType == PorttLinked || (traceFlags & TracePortalRestrict) == 0;

    public static bool ShouldTryLinePortal(InterceptStep step) =>
        step.IsLinePortal && step.OnLineSidePrecise && LinePortalHeightOk(step);

    public static bool ActorMaskAllowsThing(uint actorMask, uint actorFlags) =>
        (actorFlags & actorMask) != 0 || actorMask == 0xffffffff;

    public static bool RunInterceptStep(
        TraceTraverseContext ctx,
        InterceptStep step,
        ref double limitZ,
        ref bool loopBreak)
    {
        TryRecord3DWaterSplash(ctx, step, rover => step.Check3DFloorPlane?.Invoke(rover) == true);
        if (TryEnterFloorPortal(step, ref limitZ))
        {
            ctx.EnterSectorPortalFloor?.Invoke(step);
            ctx.LimitZ = limitZ;
            return true;
        }
        if (TryEnterCeilingPortal(step, ref limitZ))
        {
            ctx.EnterSectorPortalCeiling?.Invoke(step);
            ctx.LimitZ = limitZ;
            return true;
        }
        if (step.IsLine)
        {
            if (ShouldTryLinePortal(step) && LinePortalAllowed(step.PortalType, ctx.TraceFlags))
            {
                ctx.EnterLinePortal?.Invoke(step);
                return true;
            }
            if (ctx.LineCheck?.Invoke(step) == false)
            {
                loopBreak = true;
                return false;
            }
        }
        else if (ActorMaskAllowsThing(ctx.ActorMask, step.ActorFlags) && !step.IsIgnoredThing)
        {
            if (ctx.ThingCheck?.Invoke(step) == false)
                loopBreak = true;
        }
        return false;
    }

    public static void RunInterceptTraverseLoop(TraceTraverseContext ctx)
    {
        var limitZ = ctx.LimitZ;
        foreach (var step in ctx.Steps)
        {
            var loopBreak = false;
            if (RunInterceptStep(ctx, step, ref limitZ, ref loopBreak))
                continue;
            if (loopBreak)
                break;
        }
        ctx.LimitZ = limitZ;
    }

    public static void RunFinishSectorPlanes(TraceTraverseContext ctx)
    {
        if (ctx.CurrentHitType != ctx.HitTypeNone)
            return;
        if (ctx.PortalBlocksFloor && ctx.CheckSectorPlaneFloor?.Invoke() == true)
            ctx.AssignHitFloor?.Invoke(TraceHitFloor);
        else if (ctx.PortalBlocksCeiling && ctx.CheckSectorPlaneCeiling?.Invoke() == true)
            ctx.AssignHitCeiling?.Invoke(TraceHitCeiling);
    }

    public static void RunHeightSecWaterCross(TraceTraverseContext ctx)
    {
        if (ctx.CrossedWater != null || !ctx.HeightSecPresent)
            return;
        if (ctx.HeightSecFloorZAtStart >= ctx.StartZ)
            return;
        if (ctx.HeightSecFloorZAtHit < ctx.HitPosZ)
            return;
        if (ctx.CheckHeightSecPlane?.Invoke() != true)
            return;
        ctx.RecordCrossedWater?.Invoke(ctx);
        ctx.Distance = 0;
    }

    public static void RunFinalizeMissWhenNoHit(TraceTraverseContext ctx)
    {
        if (ctx.CurrentHitType != ctx.HitTypeNone)
            return;
        ctx.FinalizeMiss?.Invoke();
        ctx.AssignSectorOnMiss?.Invoke();
        ctx.SetSourcePosition?.Invoke();
        ctx.Distance = ctx.MaxDist;
    }

    public static bool FfloorTopMatchesHit(FFloorRover rover, double secBottom, double hitZ) =>
        Math.Abs(rover.TopZAtHit - secBottom) >= VmPTracePortalEntry.EqualEpsilon
        && Math.Abs(rover.TopZAtHit - hitZ) <= VmPTracePortalEntry.EqualEpsilon;

    public static bool FfloorBottomMatchesHit(FFloorRover rover, double secTop, double hitZ) =>
        Math.Abs(rover.BottomZAtHit - secTop) >= VmPTracePortalEntry.EqualEpsilon
        && Math.Abs(rover.BottomZAtHit - hitZ) <= VmPTracePortalEntry.EqualEpsilon;

    public static object? FindFfloorOnFloorHit(
        IReadOnlyList<FFloorRover>? rovers,
        double secBottom,
        double hitZ)
    {
        if (rovers == null)
            return null;
        foreach (var rover in rovers)
        {
            if ((rover.Flags & FfExists) == 0)
                continue;
            if (FfloorTopMatchesHit(rover, secBottom, hitZ))
                return rover.Rover;
        }
        return null;
    }

    public static object? FindFfloorOnCeilingHit(
        IReadOnlyList<FFloorRover>? rovers,
        double secTop,
        double hitZ)
    {
        if (rovers == null)
            return null;
        foreach (var rover in rovers)
        {
            if ((rover.Flags & FfExists) == 0)
                continue;
            if (FfloorBottomMatchesHit(rover, secTop, hitZ))
                return rover.Rover;
        }
        return null;
    }

    public static void RunAssignFfloorOnPlaneHit(TraceTraverseContext ctx, IReadOnlyList<FFloorRover>? rovers)
    {
        if (ctx.CurrentHitType == TraceHitFloor)
        {
            var ff = FindFfloorOnFloorHit(rovers, ctx.SectorFloorZAtHit, ctx.HitPosZ);
            if (ff != null)
                ctx.AssignFfloor?.Invoke(ff);
        }
        else if (ctx.CurrentHitType == TraceHitCeiling)
        {
            var ff = FindFfloorOnCeilingHit(rovers, ctx.SectorCeilingZAtHit, ctx.HitPosZ);
            if (ff != null)
                ctx.AssignFfloor?.Invoke(ff);
        }
    }

    public static bool TraceTraverse(TraceTraverseContext ctx)
    {
        ctx.Setup3DFloors?.Invoke();
        RunInterceptTraverseLoop(ctx);
        RunFinishSectorPlanes(ctx);
        ctx.AssignSector?.Invoke();
        RunHeightSecWaterCross(ctx);
        RunFinalizeMissWhenNoHit(ctx);
        var lastRovers = ctx.Steps.Count > 0 ? ctx.Steps[^1].SectorFFloors : null;
        RunAssignFfloorOnPlaneHit(ctx, lastRovers);
        return ctx.CurrentHitType != ctx.HitTypeNone;
    }
}
