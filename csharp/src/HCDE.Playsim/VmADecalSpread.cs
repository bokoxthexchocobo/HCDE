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
        public object? SpreadSource { get; set; }
        public object? SpreadTemplate { get; set; }
        public double SpreadZ { get; set; }
        public List<object> SpreadStack { get; } = new();
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

    public static void Spread(
        double scaleX,
        int picNum,
        Func<int, double> texWidth,
        Func<int, double> texLeftOffset,
        Func<int, bool> texValid,
        object wall,
        double x,
        double y,
        double z,
        object? ffloor,
        bool wallIsSide0,
        double v1x,
        double v1y,
        double deltaX,
        double deltaY,
        Action<double, double, double, object, object?, SpreadInfo> spreadLeft,
        Action<double, object, double, object?, SpreadInfo> spreadRight)
    {
        if (!texValid(picNum))
            return;
        var dwidth = texWidth(picNum) * scaleX;
        var spread = new SpreadInfo
        {
            DecalWidth = dwidth,
            DecalLeft = texLeftOffset(picNum) * scaleX,
            SpreadZ = z,
        };
        spread.DecalRight = spread.DecalWidth - spread.DecalLeft;
        var rorg = Length(x - v1x, y - v1y);
        spreadLeft(rorg - spread.DecalLeft, v1x, v1y, wall, ffloor, spread);
        spread.SpreadStack.Clear();
        var wallLen = Length(deltaX, deltaY);
        spreadRight(rorg + spread.DecalRight, wall, wallLen, ffloor, spread);
    }
}
