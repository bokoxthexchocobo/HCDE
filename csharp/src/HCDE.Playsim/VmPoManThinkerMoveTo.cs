namespace HCDE.Playsim;

/// <summary>Poly thinker ticks and <c>EV_MovePolyTo</c> from <c>po_man.cpp</c> (147–187, 203–214, 290–313, 376–397, 453–474, 482–519).</summary>
public static class VmPoManThinkerMoveTo
{
    public sealed class PolyActionContext
    {
        public bool Destroyed { get; set; }
        public Action? Stop { get; init; }
        public Action? Destroy { get; init; }
    }

    public static void PolyActionStop(PolyActionContext ctx) => ctx.Stop?.Invoke();

    public static void PolyActionOnDestroy(PolyActionContext ctx)
    {
        ctx.Stop?.Invoke();
        ctx.Destroyed = true;
        ctx.Destroy?.Invoke();
    }

    public sealed class RotatePolyTickContext
    {
        public bool HasPoly { get; set; } = true;
        public double Dist { get; set; }
        public double Speed { get; set; }
        public bool Destroyed { get; set; }
        public Func<double, bool>? RotatePolyobj { get; init; }
        public Action? StopSequence { get; init; }
        public Action? Destroy { get; init; }
    }

    public static void RotatePolyTick(RotatePolyTickContext ctx)
    {
        if (!ctx.HasPoly)
            return;

        if (ctx.Dist != -1 && ctx.Dist < Math.Abs(ctx.Speed))
            ctx.Speed = ctx.Speed < 0 ? -ctx.Dist : ctx.Dist;

        if (ctx.RotatePolyobj?.Invoke(ctx.Speed) != true)
            return;

        if (ctx.Dist == -1)
            return;

        ctx.Dist -= Math.Abs(ctx.Speed);
        if (ctx.Dist != 0)
            return;

        ctx.StopSequence?.Invoke();
        ctx.Destroyed = true;
        ctx.Destroy?.Invoke();
    }

    public sealed class MovePolyTickContext
    {
        public bool HasPoly { get; set; } = true;
        public double Dist { get; set; }
        public double Speed { get; set; }
        public double AngleDegrees { get; set; }
        public double SpeedVx { get; set; }
        public double SpeedVy { get; set; }
        public bool Destroyed { get; set; }
        public Func<double, double, bool>? MovePolyobj { get; init; }
        public Action? StopSequence { get; init; }
        public Action? Destroy { get; init; }
        public Action? UpdateLinks { get; init; }
    }

    public static void MovePolyTick(MovePolyTickContext ctx)
    {
        if (!ctx.HasPoly)
            return;

        if (ctx.MovePolyobj?.Invoke(ctx.SpeedVx, ctx.SpeedVy) != true)
            return;

        var absSpeed = Math.Abs(ctx.Speed);
        ctx.Dist -= absSpeed;
        if (ctx.Dist <= 0)
        {
            ctx.StopSequence?.Invoke();
            ctx.Destroyed = true;
            ctx.Destroy?.Invoke();
            return;
        }

        if (ctx.Dist < absSpeed)
        {
            ctx.Speed = ctx.Dist * (ctx.Speed < 0 ? -1 : 1);
            var rad = ctx.AngleDegrees * Math.PI / 180.0;
            ctx.SpeedVx = Math.Cos(rad) * ctx.Speed;
            ctx.SpeedVy = Math.Sin(rad) * ctx.Speed;
        }

        ctx.UpdateLinks?.Invoke();
    }

    public sealed class MovePolyToTickContext
    {
        public bool HasPoly { get; set; } = true;
        public double Dist { get; set; }
        public double Speed { get; set; }
        public double SpeedVx { get; set; }
        public double SpeedVy { get; set; }
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public bool Destroyed { get; set; }
        public Func<double, double, bool>? MovePolyobj { get; init; }
        public Action? StopSequence { get; init; }
        public Action? Destroy { get; init; }
        public Action? UpdateLinks { get; init; }
    }

    public static void MovePolyToTick(MovePolyToTickContext ctx)
    {
        if (!ctx.HasPoly)
            return;

        if (ctx.MovePolyobj?.Invoke(ctx.SpeedVx, ctx.SpeedVy) != true)
            return;

        var absSpeed = Math.Abs(ctx.Speed);
        ctx.Dist -= absSpeed;
        if (ctx.Dist <= 0)
        {
            ctx.StopSequence?.Invoke();
            ctx.Destroyed = true;
            ctx.Destroy?.Invoke();
            return;
        }

        if (ctx.Dist < absSpeed)
        {
            ctx.Speed = ctx.Dist * (ctx.Speed < 0 ? -1 : 1);
            ctx.SpeedVx = ctx.TargetX - ctx.StartX;
            ctx.SpeedVy = ctx.TargetY - ctx.StartY;
        }

        ctx.UpdateLinks?.Invoke();
    }

    public sealed class EvMovePolyToContext
    {
        public bool Override { get; set; }
        public bool Started { get; set; }
        public Func<int, MovePolyToPolyState?>? ResolveMirror { get; init; }
        public Action<MovePolyToPolyState, double, double, double, double, double, double>? SpawnThinker { get; init; }
    }

    public sealed class MovePolyToPolyState
    {
        public int PolyId { get; set; }
        public int MirrorPolyId { get; set; }
        public bool Blocked { get; set; }
        public bool HasSpecialData { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
    }

    public static bool EvMovePolyTo(EvMovePolyToContext ctx, int polyNum, double targetX, double targetY, double speed)
    {
        if (ctx.ResolveMirror is null)
            return false;

        var first = ctx.ResolveMirror(polyNum);
        if (first is null)
            return false;

        var distX = targetX - first.StartX;
        var distY = targetY - first.StartY;
        var distLen = Math.Sqrt(distX * distX + distY * distY);
        if (distLen > 0)
        {
            distX /= distLen;
            distY /= distLen;
        }

        MovePolyToPolyState? last = null;
        foreach (var mirror in VmPoManMirrorChain.Enumerate(polyNum, ctx.ResolveMirror, s => s.MirrorPolyId))
        {
            if ((mirror.HasSpecialData || mirror.Blocked) && !ctx.Override)
                break;

            var ux = distX;
            var uy = distY;
            var spawnTargetX = mirror.StartX + ux * distLen;
            var spawnTargetY = mirror.StartY + uy * distLen;
            ctx.SpawnThinker?.Invoke(mirror, distLen, speed, ux * speed, uy * speed, spawnTargetX, spawnTargetY);
            last = mirror;
            distX = -distX;
            distY = -distY;
        }

        ctx.Started = last is not null;
        return ctx.Started;
    }
}
