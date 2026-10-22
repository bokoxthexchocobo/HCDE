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
    }

    public sealed class TraceTraverseContext
    {
        public IReadOnlyList<InterceptStep> Steps { get; set; } = Array.Empty<InterceptStep>();
        public double MaxDist { get; set; }
        public int HitTypeNone { get; set; }
        public int CurrentHitType { get; set; }
        public Action? Setup3DFloors { get; init; }
        public Action<InterceptStep>? EnterLinePortal { get; init; }
        public Action<InterceptStep>? EnterSectorPortalFloor { get; init; }
        public Action<InterceptStep>? EnterSectorPortalCeiling { get; init; }
        public Func<InterceptStep, bool>? LineCheck { get; init; }
        public Func<InterceptStep, bool>? ThingCheck { get; init; }
        public Func<bool>? FinishFloorCeiling { get; init; }
        public Action? FinalizeMiss { get; init; }
    }

    public static bool TraceTraverse(TraceTraverseContext ctx)
    {
        ctx.Setup3DFloors?.Invoke();
        foreach (var step in ctx.Steps)
        {
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
            ctx.FinalizeMiss?.Invoke();

        return ctx.CurrentHitType != ctx.HitTypeNone;
    }
}
