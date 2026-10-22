namespace HCDE.Playsim;

/// <summary>Decal spread helpers from <c>a_decals.cpp</c> (437–622).</summary>
public static class VmADecalSpread
{
    public const int NoSide = -1;

    public sealed class SpreadInfo
    {
        public double DecalWidth { get; set; }
        public double DecalLeft { get; set; }
        public double DecalRight { get; set; }
        public VmADecalBase.DecalState SpreadSource { get; set; } = null!;
        public object? SpreadTemplate { get; set; }
        public double SpreadZ { get; set; }
        public bool SpreadSourceIsImpact { get; set; }
        public List<object> SpreadStack { get; } = new();
        public SpreadWallOps WallOps { get; set; } = null!;
        public VmADecalBase.CloneSelfOps BaseCloneOps { get; set; } = null!;
        public VmADecalImpact.ImpactCloneSelfOps ImpactCloneOps { get; set; } = null!;
    }

    public sealed class SpreadWallOps
    {
        public Func<object, int> GetLeftSide { get; init; } = _ => NoSide;
        public Func<object, int> GetRightSide { get; init; } = _ => NoSide;
        public Func<int, object> ResolveSide { get; init; } = _ => null!;
        public Func<object, object?> GetNextWall { get; init; } = _ => null;
        public Func<object, bool> WallIsSide0 { get; init; } = _ => true;
        public Func<object, double> GetV1X { get; init; } = _ => 0;
        public Func<object, double> GetV1Y { get; init; } = _ => 0;
        public Func<object, double> GetDeltaX { get; init; } = _ => 0;
        public Func<object, double> GetDeltaY { get; init; } = _ => 0;
    }

    public static void GetWallStuff(bool wallIsSide0, double v1x, double v1y, double v2x, double v2y, double deltaX, double deltaY, out double ldx, out double ldy)
    {
        if (wallIsSide0)
        {
            ldx = deltaX;
            ldy = deltaY;
        }
        else
        {
            ldx = -deltaX;
            ldy = -deltaY;
        }
    }

    public static double Length(double dx, double dy) => Math.Sqrt(dx * dx + dy * dy);

    public static bool TryNextWall(bool wallIsSide0, bool hasSide1, out bool nextIsSide0)
    {
        nextIsSide0 = !wallIsSide0;
        return wallIsSide0 ? hasSide1 : true;
    }

    public static void SpreadLeft(double r, double v1x, double v1y, object feelwall, object? ffloor, SpreadInfo spread)
    {
        var ops = spread.WallOps;
        spread.SpreadStack.Add(feelwall);
        while (r < 0 && ops.GetLeftSide(feelwall) != NoSide)
        {
            var startr = r;
            var x = v1x;
            var y = v1y;
            feelwall = ops.ResolveSide(ops.GetLeftSide(feelwall));
            GetWallStuff(ops.WallIsSide0(feelwall), ops.GetV1X(feelwall), ops.GetV1Y(feelwall), 0, 0, ops.GetDeltaX(feelwall), ops.GetDeltaY(feelwall), out var ldx, out var ldy);
            var wallsize = Length(ldx, ldy);
            r += spread.DecalLeft;
            x += r * ldx / wallsize;
            y += r * ldy / wallsize;
            r = wallsize + startr;
            CloneForSpread(spread, x, y, feelwall, ffloor);
            spread.SpreadStack.Add(feelwall);

            var nextwall = ops.GetNextWall(feelwall);
            if (nextwall != null && ops.GetLeftSide(nextwall) != NoSide)
            {
                var i = spread.SpreadStack.Count;
                while (i-- > 0)
                {
                    if (spread.SpreadStack[i] == nextwall)
                        break;
                }
                if (i == -1)
                {
                    GetWallStuff(ops.WallIsSide0(nextwall), ops.GetV1X(nextwall), ops.GetV1Y(nextwall), 0, 0, ops.GetDeltaX(nextwall), ops.GetDeltaY(nextwall), out _, out _);
                    SpreadLeft(startr, ops.GetV1X(nextwall), ops.GetV1Y(nextwall), nextwall, ffloor, spread);
                }
            }
        }
    }

    public static void SpreadRight(double r, object feelwall, double wallsize, object? ffloor, SpreadInfo spread)
    {
        var ops = spread.WallOps;
        spread.SpreadStack.Add(feelwall);
        while (r > wallsize && ops.GetRightSide(feelwall) != NoSide)
        {
            feelwall = ops.ResolveSide(ops.GetRightSide(feelwall));
            var nextwall = ops.GetNextWall(feelwall);
            if (nextwall != null && ops.GetLeftSide(nextwall) != NoSide)
            {
                var i = spread.SpreadStack.Count;
                while (i-- > 0)
                {
                    if (spread.SpreadStack[i] == nextwall)
                        break;
                }
                if (i == -1)
                    SpreadRight(r, nextwall, wallsize, ffloor, spread);
            }

            r = spread.DecalWidth - r + wallsize - spread.DecalLeft;
            GetWallStuff(ops.WallIsSide0(feelwall), ops.GetV1X(feelwall), ops.GetV1Y(feelwall), 0, 0, ops.GetDeltaX(feelwall), ops.GetDeltaY(feelwall), out var ldx, out var ldy);
            var x = ops.GetV1X(feelwall);
            var y = ops.GetV1Y(feelwall);
            wallsize = Length(ldx, ldy);
            x -= r * ldx / wallsize;
            y -= r * ldy / wallsize;
            r = spread.DecalRight - r;
            CloneForSpread(spread, x, y, feelwall, ffloor);
            spread.SpreadStack.Add(feelwall);
        }
    }

    static void CloneForSpread(SpreadInfo spread, double x, double y, object feelwall, object? ffloor)
    {
        if (spread.SpreadSourceIsImpact)
            VmADecalImpact.ImpactCloneSelf(spread.SpreadSource, spread.SpreadTemplate, x, y, spread.SpreadZ, feelwall, ffloor, spread.ImpactCloneOps);
        else
            VmADecalBase.CloneSelf(spread.SpreadSource, spread.SpreadTemplate, x, y, spread.SpreadZ, feelwall, ffloor, spread.BaseCloneOps);
    }

    public static void Spread(
        VmADecalBase.DecalState source,
        object? tpl,
        object wall,
        double x,
        double y,
        double z,
        object? ffloor,
        bool sourceIsImpact,
        double scaleX,
        Func<int, double> texWidth,
        Func<int, double> texLeftOffset,
        Func<int, bool> texValid,
        SpreadWallOps wallOps,
        VmADecalBase.CloneSelfOps baseCloneOps,
        VmADecalImpact.ImpactCloneSelfOps impactCloneOps)
    {
        if (!texValid(source.PicNum))
            return;
        GetWallStuff(wallOps.WallIsSide0(wall), wallOps.GetV1X(wall), wallOps.GetV1Y(wall), 0, 0, wallOps.GetDeltaX(wall), wallOps.GetDeltaY(wall), out var ldx, out var ldy);
        var rorg = Length(x - wallOps.GetV1X(wall), y - wallOps.GetV1Y(wall));
        var dwidth = texWidth(source.PicNum) * scaleX;
        var spread = new SpreadInfo
        {
            DecalWidth = dwidth,
            DecalLeft = texLeftOffset(source.PicNum) * scaleX,
            SpreadSource = source,
            SpreadTemplate = tpl,
            SpreadZ = z,
            SpreadSourceIsImpact = sourceIsImpact,
            WallOps = wallOps,
            BaseCloneOps = baseCloneOps,
            ImpactCloneOps = impactCloneOps,
        };
        spread.DecalRight = spread.DecalWidth - spread.DecalLeft;
        SpreadLeft(rorg - spread.DecalLeft, wallOps.GetV1X(wall), wallOps.GetV1Y(wall), wall, ffloor, spread);
        spread.SpreadStack.Clear();
        SpreadRight(rorg + spread.DecalRight, wall, Length(wallOps.GetDeltaX(wall), wallOps.GetDeltaY(wall)), ffloor, spread);
    }
}
