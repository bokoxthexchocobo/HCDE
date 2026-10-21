namespace HCDE.Playsim;

/// <summary><c>FPolyObj::MovePolyobj</c>, <c>DoMovePolyobj</c>, <c>RotatePolyobj</c>, <c>EV_MovePoly</c> from <c>po_man.cpp</c> (405–452, 858–982).</summary>
public static class VmPoManMoveRotate
{
    public sealed class MovePolyobjContext
    {
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public bool Force { get; set; }
        public List<int> SideDefIndices { get; } = new();
        public List<(double X, double Y)> Vertices { get; } = new();
        public List<(double X, double Y)> PrevPts { get; } = new();
        public List<(double Top, double Bottom, double Left, double Right)> LineBboxes { get; } = new();
        public double StartSpotX { get; set; }
        public double StartSpotY { get; set; }
        public double CenterSpotX { get; set; }
        public double CenterSpotY { get; set; }

        public Action? Unlink { get; init; }
        public Action? Link { get; init; }
        public Action? ClearSubsectorLinks { get; init; }
        public Action? RecalcActorFloorCeil { get; init; }
        public Func<int, bool>? CheckMobjBlocking { get; init; }
    }

    public static bool MovePolyobj(MovePolyobjContext ctx)
    {
        ctx.Unlink?.Invoke();
        DoMovePolyobj(ctx);

        if (!ctx.Force)
        {
            var blocked = false;
            foreach (var side in ctx.SideDefIndices)
            {
                if (ctx.CheckMobjBlocking?.Invoke(side) == true)
                    blocked = true;
            }

            if (blocked)
            {
                ctx.DeltaX = -ctx.DeltaX;
                ctx.DeltaY = -ctx.DeltaY;
                DoMovePolyobj(ctx);
                ctx.Link?.Invoke();
                return false;
            }
        }

        ctx.StartSpotX += ctx.DeltaX;
        ctx.StartSpotY += ctx.DeltaY;
        ctx.CenterSpotX += ctx.DeltaX;
        ctx.CenterSpotY += ctx.DeltaY;
        ctx.Link?.Invoke();
        ctx.ClearSubsectorLinks?.Invoke();
        ctx.RecalcActorFloorCeil?.Invoke();
        return true;
    }

    public static void DoMovePolyobj(MovePolyobjContext ctx)
    {
        for (var i = 0; i < ctx.Vertices.Count; i++)
        {
            var v = ctx.Vertices[i];
            ctx.Vertices[i] = (v.X + ctx.DeltaX, v.Y + ctx.DeltaY);
            var p = ctx.PrevPts[i];
            ctx.PrevPts[i] = (p.X + ctx.DeltaX, p.Y + ctx.DeltaY);
        }

        for (var i = 0; i < ctx.LineBboxes.Count; i++)
        {
            var b = ctx.LineBboxes[i];
            ctx.LineBboxes[i] = (b.Top + ctx.DeltaY, b.Bottom + ctx.DeltaY, b.Left + ctx.DeltaX, b.Right + ctx.DeltaX);
        }
    }

    public sealed class RotatePolyobjContext
    {
        public double AngleDegrees { get; set; }
        public double DeltaDegrees { get; set; }
        public bool FromSave { get; set; }
        public double StartSpotX { get; set; }
        public double StartSpotY { get; set; }
        public List<(double X, double Y)> Vertices { get; } = new();
        public List<(double X, double Y)> PrevPts { get; } = new();
        public List<(double X, double Y)> OriginalPts { get; } = new();
        public List<int> SideDefIndices { get; } = new();

        public Action? Unlink { get; init; }
        public Action? Link { get; init; }
        public Action? UpdateBBox { get; init; }
        public Action? ClearSubsectorLinks { get; init; }
        public Action? RecalcActorFloorCeil { get; init; }
        public Func<int, bool>? CheckMobjBlocking { get; init; }
    }

    public static bool RotatePolyobj(RotatePolyobjContext ctx)
    {
        var an = ctx.AngleDegrees + ctx.DeltaDegrees;
        ctx.Unlink?.Invoke();

        for (var i = 0; i < ctx.Vertices.Count; i++)
        {
            ctx.PrevPts[i] = ctx.Vertices[i];
            var torot = ctx.OriginalPts[i];
            ctx.Vertices[i] = RotatePt(an, torot, (ctx.StartSpotX, ctx.StartSpotY));
        }

        var blocked = false;
        ctx.UpdateBBox?.Invoke();

        if (!ctx.FromSave)
        {
            foreach (var side in ctx.SideDefIndices)
            {
                if (ctx.CheckMobjBlocking?.Invoke(side) == true)
                    blocked = true;
            }

            if (blocked)
            {
                for (var i = 0; i < ctx.Vertices.Count; i++)
                    ctx.Vertices[i] = ctx.PrevPts[i];
                ctx.UpdateBBox?.Invoke();
                ctx.Link?.Invoke();
                return false;
            }
        }

        ctx.AngleDegrees += ctx.DeltaDegrees;
        ctx.Link?.Invoke();
        ctx.ClearSubsectorLinks?.Invoke();
        ctx.RecalcActorFloorCeil?.Invoke();
        return true;
    }

    static (double X, double Y) RotatePt(double degrees, (double X, double Y) point, (double Sx, double Sy) start)
    {
        var rad = degrees * Math.PI / 180.0;
        var s = Math.Sin(rad);
        var c = Math.Cos(rad);
        var x = point.X * c - point.Y * s + start.Sx;
        var y = point.X * s + point.Y * c + start.Sy;
        return (x, y);
    }

    public sealed class EvMovePolyContext
    {
        public double Speed { get; set; }
        public double AngleDegrees { get; set; }
        public double Distance { get; set; }
        public bool Override { get; set; }
        public bool Started { get; set; }

        public Func<int, bool>? PolyBusy { get; init; }
        public Action<int, double, double, double>? SpawnMoveThinker { get; init; }
    }

    public sealed class EvRotatePolyContext
    {
        public int Speed { get; set; }
        public int ByteAngle { get; set; }
        public int Direction { get; set; } = 1;
        public bool Override { get; set; }
        public bool Started { get; set; }

        public Func<int, RotatePolyState?>? ResolveMirror { get; init; }
        public Action<RotatePolyState, EvRotatePolyContext>? StartRotate { get; init; }
    }

    public sealed class RotatePolyState
    {
        public int PolyId { get; set; }
        public bool Blocked { get; set; }
        public bool HasSpecialData { get; set; }
        public int PortalLinkMode { get; set; }
    }

    public static bool EvRotatePoly(EvRotatePolyContext ctx, int polyNum)
    {
        var dir = ctx.Direction;
        RotatePolyState? last = null;
        for (var mirror = ctx.ResolveMirror?.Invoke(polyNum); mirror is not null; mirror = ctx.ResolveMirror?.Invoke(mirror.PolyId))
        {
            if ((mirror.HasSpecialData || mirror.Blocked) && !ctx.Override)
                break;
            if (mirror.PortalLinkMode == 2)
                break;

            ctx.StartRotate?.Invoke(mirror, ctx);
            last = mirror;
            dir = -dir;
        }

        ctx.Started = last is not null;
        return ctx.Started;
    }

    public static double RotatePolyDistance(int byteAngle) =>
        byteAngle switch
        {
            255 => -1,
            0 => 360,
            _ => byteAngle * (90.0 / 64),
        };

    public static double RotatePolySpeed(int speed, int direction) =>
        speed * direction * (90.0 / (64 << 3));

    public static bool EvMovePoly(EvMovePolyContext ctx, int polyNum)
    {
        if (ctx.PolyBusy?.Invoke(polyNum) == true && !ctx.Override)
            return false;

        var vx = Math.Cos(ctx.AngleDegrees * Math.PI / 180.0) * ctx.Speed;
        var vy = Math.Sin(ctx.AngleDegrees * Math.PI / 180.0) * ctx.Speed;
        ctx.SpawnMoveThinker?.Invoke(polyNum, vx * ctx.Distance, vy * ctx.Distance, ctx.Speed);
        ctx.Started = true;
        return true;
    }
}
