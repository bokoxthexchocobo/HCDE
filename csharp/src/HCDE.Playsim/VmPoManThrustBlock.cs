namespace HCDE.Playsim;

/// <summary><c>FPolyObj::ThrustMobj</c> and <c>CheckMobjBlocking</c> from <c>po_man.cpp</c> (749–795, 1025–1146).</summary>
public static class VmPoManThrustBlock
{
    public sealed class ThrustMobjContext
    {
        public bool Shootable { get; set; }
        public bool IsPlayer { get; set; }
        public double SideV1X { get; set; }
        public double SideV1Y { get; set; }
        public double SideV2X { get; set; }
        public double SideV2Y { get; set; }
        public bool IsRotatePoly { get; set; }
        public double PolySpeed { get; set; }
        public bool Crush { get; set; }
        public bool HurtOnTouch { get; set; }
        public int LevelPolygrind { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }

        public Func<double, double, bool>? CheckMove { get; init; }
        public Func<int>? ApplyCrushDamage { get; init; }
        public Action<int>? TraceBleed { get; init; }
        public Action? CallGrind { get; init; }
    }

    public static void ThrustMobj(ThrustMobjContext ctx)
    {
        if (!ctx.Shootable && !ctx.IsPlayer)
            return;

        var dx = ctx.SideV2X - ctx.SideV1X;
        var dy = ctx.SideV2Y - ctx.SideV1Y;
        var angleRad = Math.Atan2(dy, dx) - Math.PI / 2;
        var force = ctx.IsRotatePoly
            ? Math.Clamp(ctx.PolySpeed * (90.0 / 2048), 1, 4)
            : Math.Clamp(ctx.PolySpeed / 8, 1, 4);
        if (ctx.PolySpeed == 0 && !ctx.IsRotatePoly)
            force = 1;

        var thrustX = Math.Cos(angleRad) * force;
        var thrustY = Math.Sin(angleRad) * force;
        ctx.VelX += thrustX;
        ctx.VelY += thrustY;

        if (!ctx.Crush) return;

        var newX = thrustX;
        var newY = thrustY;
        if (ctx.HurtOnTouch || ctx.CheckMove?.Invoke(newX, newY) != true)
        {
            var dmg = ctx.ApplyCrushDamage?.Invoke() ?? 0;
            ctx.TraceBleed?.Invoke(dmg > 0 ? dmg : 10);
        }

        if ((ctx.LevelPolygrind & 1) != 0)
            ctx.CallGrind?.Invoke();
    }

    public readonly record struct BlockingMobj(
        bool Solid,
        bool Noclip,
        double X,
        double Y,
        double Radius,
        double Z,
        double Top,
        double FloorZ,
        int Id);

    public sealed class CheckMobjBlockingContext
    {
        public int BlockWidth { get; set; }
        public int BlockHeight { get; set; }
        public int BoxLeft { get; set; }
        public int BoxRight { get; set; }
        public int BoxTop { get; set; }
        public int BoxBottom { get; set; }
        public bool HasBacksector { get; set; }
        public int LineFlags { get; set; }
        public object? TargetSide { get; set; }
        public bool Blocked { get; set; }
        public List<int> SeenIds { get; } = new();

        public Func<int, int, IEnumerable<BlockingMobj>>? MobjsInBlock { get; init; }
        public Func<BlockingMobj, bool>? IsBlockedByLine { get; init; }
        public Func<BlockingMobj, bool>? MidtexBlocks { get; init; }
        public Func<BlockingMobj, double, double, double, double, bool>? InRange { get; init; }
        public Func<BlockingMobj, int>? BoxOnLineSide { get; init; }
        public Func<BlockingMobj, bool>? TryMove { get; init; }
        public Func<BlockingMobj, int>? PointOnLineSide { get; init; }
        public Func<BlockingMobj, bool>? BelowFloorOrAboveCeiling { get; init; }
        public Action<BlockingMobj>? Thrust { get; init; }
    }

    public const int MfSolid = 1;
    public const int MfNoclip = 1 << 1;
    public const int Ml3dmidtex = 1;

    public static bool CheckMobjBlocking(CheckMobjBlockingContext ctx)
    {
        var left = Clamp(ctx.BoxLeft, 0, ctx.BlockWidth - 1);
        var right = Clamp(ctx.BoxRight, 0, ctx.BlockWidth - 1);
        var bottom = Clamp(ctx.BoxBottom, 0, ctx.BlockHeight - 1);
        var top = Clamp(ctx.BoxTop, 0, ctx.BlockHeight - 1);

        for (var j = bottom * ctx.BlockWidth; j <= top * ctx.BlockWidth; j += ctx.BlockWidth)
        {
            for (var i = left; i <= right; i++)
            {
                var index = j + i;
                foreach (var mobj in ctx.MobjsInBlock?.Invoke(i, j / ctx.BlockWidth) ?? Array.Empty<BlockingMobj>())
                {
                    if (ctx.SeenIds.Contains(mobj.Id)) continue;
                    ctx.SeenIds.Add(mobj.Id);

                    if (!mobj.Solid || mobj.Noclip) continue;

                    var performThrust = true;
                    if (ctx.HasBacksector && ctx.IsBlockedByLine?.Invoke(mobj) == false)
                    {
                        var midtex = (ctx.LineFlags & Ml3dmidtex) != 0;
                        var blockedByMidtex = midtex && ctx.MidtexBlocks?.Invoke(mobj) == true;
                        if (!midtex || !blockedByMidtex)
                            performThrust = false;
                    }

                    if (ctx.InRange?.Invoke(mobj, mobj.X, mobj.Y, mobj.Radius, mobj.Radius) != true
                        || ctx.BoxOnLineSide?.Invoke(mobj) != -1)
                    {
                        continue;
                    }

                    if (ctx.TryMove?.Invoke(mobj) == true)
                        continue;

                    if (performThrust == false && ctx.BelowFloorOrAboveCeiling?.Invoke(mobj) == true)
                        performThrust = true;

                    if (performThrust)
                    {
                        ctx.Thrust?.Invoke(mobj);
                        ctx.Blocked = true;
                    }
                }
            }
        }

        return ctx.Blocked;
    }

    static int Clamp(int v, int lo, int hi) => v < lo ? lo : v >= hi ? hi : v;
}
