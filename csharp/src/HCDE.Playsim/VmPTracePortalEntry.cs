namespace HCDE.Playsim;

/// <summary><c>GetPortalTransition</c>, trace setup hooks, and portal enter from <c>p_trace.cpp</c> (99–271).</summary>
public static class VmPTracePortalEntry
{
    public const int TraceReportPortals = 1;
    public const int TraceCrossingPortal = 2;
    public const int FfSwimmable = 1;

    public static bool IsLiquid(bool ffSwimmable, int terrainIndex, Func<int, (bool IsLiquid, int Splash)>? terrainLookup)
    {
        if (ffSwimmable)
            return true;
        var terrain = terrainLookup?.Invoke(terrainIndex) ?? (false, -1);
        return terrain.IsLiquid && terrain.Splash != -1;
    }

    public sealed class PortalTransitionContext
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public bool BlocksCeiling { get; set; }
        public bool BlocksFloor { get; set; }
        public double CeilingPlaneZ { get; set; }
        public double FloorPlaneZ { get; set; }
        public double DispX { get; set; }
        public double DispY { get; set; }
        public double DispZ { get; set; }
        public Func<double, double, double, object?>? PointInSector { get; set; }
        public object? CurrentSector { get; set; }
    }

    public static void GetPortalTransition(PortalTransitionContext ctx)
    {
        var moved = false;
        while (!ctx.BlocksCeiling)
        {
            if (ctx.PosZ > ctx.CeilingPlaneZ)
            {
                ctx.PosX += ctx.DispX;
                ctx.PosY += ctx.DispY;
                ctx.PosZ += ctx.DispZ;
                ctx.CurrentSector = ctx.PointInSector?.Invoke(ctx.PosX, ctx.PosY, ctx.PosZ);
                moved = true;
            }
            else
                break;
        }
        if (!moved)
        {
            while (!ctx.BlocksFloor)
            {
                if (ctx.PosZ <= ctx.FloorPlaneZ)
                {
                    ctx.PosX += ctx.DispX;
                    ctx.PosY += ctx.DispY;
                    ctx.PosZ += ctx.DispZ;
                    ctx.CurrentSector = ctx.PointInSector?.Invoke(ctx.PosX, ctx.PosY, ctx.PosZ);
                }
                else
                    break;
            }
        }
    }

    public sealed class TraceSetupContext
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double StartZ { get; set; }
        public object? Sector { get; set; }
        public bool ActorMaskNonZero { get; set; }
        public int TraceFlags { get; set; }
        public bool HasCallback { get; set; }
        public int PortalCallbackCount { get; set; }
        public PortalTransitionContext Transition { get; } = new();
        public Action? RunTraceTraverse { get; init; }
        public Action<double, double, double>? ReportPortalAtStart { get; init; }
        public Action<double, double, double>? ReportPortalAtEnd { get; init; }
        public Func<bool>? TraceTraverseResult { get; init; }
        public Func<bool>? EditTraceResult { get; init; }
    }

    public static bool Trace(TraceSetupContext ctx)
    {
        ctx.Transition.PosX = ctx.StartX;
        ctx.Transition.PosY = ctx.StartY;
        ctx.Transition.PosZ = ctx.StartZ;
        ctx.Transition.CurrentSector = ctx.Sector;
        GetPortalTransition(ctx.Transition);
        ctx.StartX = ctx.Transition.PosX;
        ctx.StartY = ctx.Transition.PosY;
        ctx.StartZ = ctx.Transition.PosZ;
        ctx.Sector = ctx.Transition.CurrentSector;

        if ((ctx.TraceFlags & TraceReportPortals) != 0 && ctx.HasCallback)
            ctx.ReportPortalAtStart?.Invoke(ctx.StartX, ctx.StartY, ctx.StartZ);

        var reslt = ctx.TraceTraverseResult?.Invoke() ?? false;
        if ((ctx.TraceFlags & TraceReportPortals) != 0 && ctx.HasCallback)
            ctx.ReportPortalAtEnd?.Invoke(ctx.StartX, ctx.StartY, ctx.StartZ);

        if (reslt)
            return ctx.TraceFlags != 0 ? ctx.EditTraceResult?.Invoke() ?? true : true;
        return false;
    }

    public sealed class EnterSectorPortalContext
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double StartZ { get; set; }
        public double MaxDist { get; set; }
        public double VecX { get; set; }
        public double VecY { get; set; }
        public double VecZ { get; set; }
        public double Frac { get; set; }
        public double DispX { get; set; }
        public double DispY { get; set; }
        public double DispZ { get; set; }
        public int TraceFlags { get; set; }
        public bool HasCallback { get; set; }
        public double EnterDist { get; set; }
        public double StartFrac { get; set; }
        public bool InShootThrough { get; set; } = true;
        public object? CurSector { get; set; }
        public Func<double, double, double, object?>? PointInSector { get; init; }
        public Action<double, double, double>? PortalRelocate { get; init; }
        public Action? Setup3DFloors { get; init; }
        public Action<double, double, double, double, double, double>? ReportPortal { get; init; }
    }

    public static void EnterSectorPortal(EnterSectorPortalContext ctx)
    {
        var enterdist = ctx.MaxDist * ctx.Frac;
        var exitX = ctx.StartX + enterdist * ctx.VecX;
        var exitY = ctx.StartY + enterdist * ctx.VecY;
        var exitZ = ctx.StartZ + enterdist * ctx.VecZ;
        var enterX = exitX + ctx.DispX;
        var enterY = exitY + ctx.DispY;
        var enterZ = exitZ + ctx.DispZ;

        ctx.StartX += ctx.DispX;
        ctx.StartY += ctx.DispY;
        ctx.StartZ += ctx.DispZ;
        ctx.CurSector = ctx.PointInSector?.Invoke(enterX, enterY, enterZ);
        ctx.InShootThrough = true;
        ctx.StartFrac = ctx.Frac;
        ctx.EnterDist = enterdist;
        ctx.PortalRelocate?.Invoke(ctx.DispX, ctx.DispY, ctx.DispZ);

        if ((ctx.TraceFlags & TraceReportPortals) != 0 && ctx.HasCallback)
            ctx.ReportPortal?.Invoke(exitX, exitY, exitZ, enterX, enterY, enterZ);

        ctx.Setup3DFloors?.Invoke();
    }

    public sealed class EnterLinePortalContext
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double StartZ { get; set; }
        public double LimitZ { get; set; }
        public double MaxDist { get; set; }
        public double VecX { get; set; }
        public double VecY { get; set; }
        public double Frac { get; set; }
        public int PortalType { get; set; }
        public int TraceFlags { get; set; }
        public bool HasCallback { get; set; }
        public bool Unlinked { get; set; }
        public VmPortalClipTranslate.TranslatePortalContext Translate { get; } = new();
        public Func<double, double, double, object?>? PointInSector { get; init; }
        public Action? PortalRelocate { get; init; }
        public Action<double, double, double, double, double, double>? ReportPortal { get; init; }
    }

    public const double EqualEpsilon = 1.0 / 65536;

    public static void EnterLinePortal(EnterLinePortalContext ctx)
    {
        var frac = ctx.Frac + EqualEpsilon;
        var enterdist = ctx.MaxDist * frac;
        var exitX = ctx.StartX + ctx.MaxDist * ctx.Frac * ctx.VecX;
        var exitY = ctx.StartY + ctx.MaxDist * ctx.Frac * ctx.VecY;
        var exitZ = ctx.StartZ + ctx.MaxDist * ctx.Frac * 0;

        var startX = ctx.StartX;
        var startY = ctx.StartY;
        var startZ = ctx.StartZ;
        var vecX = ctx.VecX;
        var vecY = ctx.VecY;
        var limitZ = ctx.LimitZ;
        VmPortalClipTranslate.TranslatePortalXy(ctx.Translate, ref startX, ref startY);
        VmPortalClipTranslate.TranslatePortalZ(ctx.Translate, ref startZ);
        VmPortalClipTranslate.TranslatePortalVxVy(ctx.Translate, ref vecX, ref vecY);
        VmPortalClipTranslate.TranslatePortalZ(ctx.Translate, ref limitZ);
        ctx.StartX = startX;
        ctx.StartY = startY;
        ctx.StartZ = startZ;
        ctx.VecX = vecX;
        ctx.VecY = vecY;
        ctx.LimitZ = limitZ;

        _ = ctx.PointInSector?.Invoke(ctx.StartX + enterdist * ctx.VecX, ctx.StartY + enterdist * ctx.VecY, ctx.StartZ);
        ctx.Unlinked |= ctx.PortalType != VmPortalUpdateFinalize.PorttLinked;
        ctx.PortalRelocate?.Invoke();

        if ((ctx.TraceFlags & TraceReportPortals) != 0 && ctx.HasCallback)
        {
            VmPortalClipTranslate.TranslatePortalXy(ctx.Translate, ref exitX, ref exitY);
            VmPortalClipTranslate.TranslatePortalZ(ctx.Translate, ref exitZ);
            ctx.ReportPortal?.Invoke(exitX, exitY, exitZ, exitX, exitY, exitZ);
        }
    }
}
