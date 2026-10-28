namespace HCDE.Playsim;

/// <summary><c>P_CheckSight</c> from <c>p_sight.cpp</c> (840–951).</summary>
public static class VmPSightCheckSight
{
    public const int SfIgnoreVisibility = 1;
    public const int SfIgnoreWaterBoundary = 2;
    public const int Mf8MvisBlocked = 1;
    public const int Mf8Minvisible = 1 << 1;
    public const int RfInvisible = 1;

    public sealed class CheckSightContext
    {
        public object? Viewer { get; set; }
        public object? Target { get; set; }
        public int Flags { get; set; }
        public int ViewerFlags8 { get; set; }
        public int TargetFlags8 { get; set; }
        public int TargetRenderFlags { get; set; }
        public bool TargetRenderVisible { get; set; } = true;
        public double TargetAlpha { get; set; } = 1;
        public double ViewerZ { get; set; }
        public double ViewerTop { get; set; }
        public double TargetZ { get; set; }
        public double TargetHeight { get; set; }
        public bool BotThinking { get; set; }
        public int SightRoll { get; set; } = 100;
        public bool RejectTableAllows { get; set; } = true;
        public bool ViewerHeightSec { get; set; }
        public bool TargetHeightSec { get; set; }
        public double ViewerHeightSecFloorZ { get; set; }
        public double ViewerHeightSecCeilZ { get; set; }
        public double TargetHeightSecFloorZ { get; set; }
        public double TargetHeightSecCeilZ { get; set; }
        public double ViewerX { get; set; }
        public double ViewerY { get; set; }
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public int TargetPortalGroup { get; set; }
        public List<VmPSightPathTraverse.SightPortalTask> PortalTasks { get; } = new();

        public Func<CheckSightContext, bool>? RunPrimarySightPath { get; init; }
        public Func<CheckSightContext, VmPSightPathTraverse.SightPortalTask, bool>? RunPortalSightPath { get; init; }
        public Action? ClockSightCycles { get; init; }
        public Action? UnclockSightCycles { get; init; }
    }

    public static bool CheckSight(CheckSightContext ctx)
    {
        ctx.ClockSightCycles?.Invoke();

        if (ctx.Viewer is null || ctx.Target is null)
        {
            ctx.UnclockSightCycles?.Invoke();
            return false;
        }

        if ((ctx.TargetFlags8 & Mf8MvisBlocked) != 0 && (ctx.Flags & SfIgnoreVisibility) == 0)
        {
            ctx.UnclockSightCycles?.Invoke();
            return false;
        }

        if (!ctx.RejectTableAllows)
        {
            ctx.UnclockSightCycles?.Invoke();
            return false;
        }

        if ((ctx.Flags & SfIgnoreVisibility) == 0
            && ((ctx.TargetRenderFlags & RfInvisible) != 0
                || (ctx.TargetFlags8 & Mf8Minvisible) != 0
                || !ctx.TargetRenderVisible))
        {
            if (ctx.SightRoll > 50)
            {
                ctx.UnclockSightCycles?.Invoke();
                return false;
            }
        }

        if (ReferenceEquals(ctx.Viewer, ctx.Target))
        {
            ctx.UnclockSightCycles?.Invoke();
            return true;
        }

        if ((ctx.Flags & SfIgnoreWaterBoundary) == 0)
        {
            if (ctx.ViewerHeightSec
                && ((ctx.ViewerTop <= ctx.ViewerHeightSecFloorZ && ctx.TargetZ >= ctx.ViewerHeightSecFloorZ)
                    || (ctx.ViewerZ >= ctx.ViewerHeightSecCeilZ && ctx.TargetZ + ctx.TargetHeight <= ctx.ViewerHeightSecCeilZ)))
            {
                ctx.UnclockSightCycles?.Invoke();
                return false;
            }

            if (ctx.TargetHeightSec
                && ((ctx.TargetZ + ctx.TargetHeight <= ctx.TargetHeightSecFloorZ && ctx.ViewerZ >= ctx.TargetHeightSecFloorZ)
                    || (ctx.TargetZ >= ctx.TargetHeightSecCeilZ && ctx.ViewerTop <= ctx.TargetHeightSecCeilZ)))
            {
                ctx.UnclockSightCycles?.Invoke();
                return false;
            }
        }

        var res = ctx.RunPrimarySightPath?.Invoke(ctx) == true;
        if (!res)
        {
            var dist = Math.Sqrt((ctx.ViewerX - ctx.TargetX) * (ctx.ViewerX - ctx.TargetX)
                + (ctx.ViewerY - ctx.TargetY) * (ctx.ViewerY - ctx.TargetY));
            if (dist > 0)
            {
                for (var i = 0; i < ctx.PortalTasks.Count; i++)
                {
                    var task = ctx.PortalTasks[i];
                    task = task with { Frac = task.Frac + 1 / dist };
                    if (ctx.RunPortalSightPath?.Invoke(ctx, task) == true)
                    {
                        res = true;
                        break;
                    }
                }
            }
        }

        ctx.UnclockSightCycles?.Invoke();
        return res;
    }
}
