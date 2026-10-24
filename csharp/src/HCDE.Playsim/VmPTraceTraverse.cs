namespace HCDE.Playsim;

/// <summary><c>FTraceInfo::TraceTraverse</c> orchestration from <c>p_trace.cpp</c> (776–944).</summary>
public static class VmPTraceTraverse
{
    public sealed class InterceptStep
    {
        public bool IsLine { get; set; }
        public double Frac { get; set; }
        public bool IsLinePortal { get; set; }
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
    }

    public sealed class TraceTraverseContext
    {
        public IReadOnlyList<InterceptStep> Steps { get; set; } = Array.Empty<InterceptStep>();
        public double MaxDist { get; set; }
        public int HitTypeNone { get; set; }
        public int CurrentHitType { get; set; }
        public double LimitZ { get; set; }
        public Action? Setup3DFloors { get; init; }
        public Action<InterceptStep>? EnterLinePortal { get; init; }
        public Action<InterceptStep>? EnterSectorPortalFloor { get; init; }
        public Action<InterceptStep>? EnterSectorPortalCeiling { get; init; }
        public Func<InterceptStep, bool>? LineCheck { get; init; }
        public Func<InterceptStep, bool>? ThingCheck { get; init; }
        public Func<bool>? FinishFloorCeiling { get; init; }
        public Action? FinalizeMiss { get; init; }
        public Action? AssignSectorOnMiss { get; init; }
        public Action? AssignFfloorOnPlaneHit { get; init; }
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

    public static bool TraceTraverse(TraceTraverseContext ctx)
    {
        ctx.Setup3DFloors?.Invoke();
        var limitZ = ctx.LimitZ;
        foreach (var step in ctx.Steps)
        {
            if (TryEnterFloorPortal(step, ref limitZ))
            {
                ctx.EnterSectorPortalFloor?.Invoke(step);
                ctx.LimitZ = limitZ;
                continue;
            }
            if (TryEnterCeilingPortal(step, ref limitZ))
            {
                ctx.EnterSectorPortalCeiling?.Invoke(step);
                ctx.LimitZ = limitZ;
                continue;
            }
            if (step.IsLinePortal)
            {
                ctx.EnterLinePortal?.Invoke(step);
                continue;
            }
            if (step.IsLine)
            {
                if (ctx.LineCheck?.Invoke(step) == false)
                    break;
            }
            else if (!ctx.ThingCheck?.Invoke(step) ?? false)
                break;
        }

        if (ctx.CurrentHitType == ctx.HitTypeNone)
            ctx.FinishFloorCeiling?.Invoke();

        if (ctx.CurrentHitType == ctx.HitTypeNone)
        {
            ctx.FinalizeMiss?.Invoke();
            ctx.AssignSectorOnMiss?.Invoke();
        }

        ctx.AssignFfloorOnPlaneHit?.Invoke();
        return ctx.CurrentHitType != ctx.HitTypeNone;
    }
}
