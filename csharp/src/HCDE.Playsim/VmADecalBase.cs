namespace HCDE.Playsim;

/// <summary><c>DBaseDecal</c> construction, wall stick, and Z helpers from <c>a_decals.cpp</c>.</summary>
public static class VmADecalBase
{
    public const int RfRelMid = 1 << 0;
    public const int RfRelUpper = 1 << 1;
    public const int RfRelLower = 1 << 2;
    public const int RfRelMask = RfRelMid | RfRelUpper | RfRelLower;
    public const int RfClipLower = 1 << 3;
    public const int RfClipUpper = 1 << 4;
    public const int RfClipMid = 1 << 5;
    public const int MlDontpegtop = 1 << 4;
    public const int MlDontpegbottom = 1 << 5;
    public const int FfUppertexture = 1;
    public const int FfLowertexture = 2;
    public const int SideTop = 0;
    public const int SideMid = 1;
    public const int SideBottom = 2;
    public const int PlaneFloor = 0;
    public const int PlaneCeiling = 1;
    public const int InvalidTexture = -1;

    public sealed class DecalState
    {
        public double Z { get; set; }
        public double LeftDistance { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double Alpha { get; set; }
        public uint AlphaColor { get; set; }
        public int Translation { get; set; }
        public int PicNum { get; set; } = InvalidTexture;
        public int RenderFlags { get; set; }
        public int RenderStyle { get; set; }
        public object? Side { get; set; }
        public object? Sector { get; set; }
        public object? WallPrev { get; set; }
        public object? WallNext { get; set; }
    }

    public static void ConstructFromZ(DecalState d, double z)
    {
        d.Z = z;
        d.RenderStyle = 0;
        d.PicNum = InvalidTexture;
    }

    public static void ConstructFromActor(DecalState d, Func<double> z, double scaleX, double scaleY, double alpha, uint alphaColor, int translation, int picnum, int renderFlags, int renderStyle)
    {
        d.Z = z();
        d.ScaleX = scaleX;
        d.ScaleY = scaleY;
        d.Alpha = alpha;
        d.AlphaColor = alphaColor;
        d.Translation = translation;
        d.PicNum = picnum;
        d.RenderFlags = renderFlags;
        d.RenderStyle = renderStyle;
    }

    public static void ConstructFromDecal(DecalState d, DecalState basis)
    {
        d.LeftDistance = basis.LeftDistance;
        d.Z = basis.Z;
        d.ScaleX = basis.ScaleX;
        d.ScaleY = basis.ScaleY;
        d.Alpha = basis.Alpha;
        d.AlphaColor = basis.AlphaColor;
        d.Translation = basis.Translation;
        d.PicNum = basis.PicNum;
        d.RenderFlags = basis.RenderFlags;
        d.RenderStyle = basis.RenderStyle;
    }

    public static void RemoveFromWall(DecalState d, Action<object, object?> setHead, Func<object?, object?> getWallNext, Action<object?, object?> setWallNext, Action<object?, object?> setWallPrev)
    {
        if (d.WallPrev == null)
        {
            if (d.Side != null)
                setHead(d.Side, d.WallNext);
        }
        else
            setWallNext(d.WallPrev, d.WallNext);

        if (d.WallNext != null)
            setWallPrev(d.WallNext, d.WallPrev);

        d.WallPrev = null;
        d.WallNext = null;
    }

    public static void GetXY(DecalState d, bool wallIsSide0, double v1x, double v1y, double v2x, double v2y, out double ox, out double oy)
    {
        var dx = v2x - v1x;
        var dy = v2y - v1y;
        ox = v1x + d.LeftDistance * dx;
        oy = v1y + d.LeftDistance * dy;
    }

    public static void SetShadeRgb(DecalState d, uint rgb, Func<int, int, int, int> colorMatcherPick) =>
        d.AlphaColor = rgb | (uint)(colorMatcherPick((int)(rgb & 0xff), (int)((rgb >> 8) & 0xff), (int)((rgb >> 16) & 0xff)) << 24);

    public static void SetShadeRgb(DecalState d, int r, int g, int b, Func<int, int, int, int> colorMatcherPick) =>
        d.AlphaColor = (uint)(colorMatcherPick(r, g, b) << 24 | (r << 16) | (g << 8) | b);

    public static int StickToWall(
        DecalState d,
        object wall,
        double x,
        double y,
        object? ffloor,
        bool wallIsSide0,
        object? backSector,
        int lineFlags,
        Func<object, int, int> getTexture,
        Func<object, int, double> getPlaneTexZ,
        Func<double, double, double> floorZAt,
        Func<double, double, double> ceilingZAt,
        Func<object?, object?> getAttachedHead,
        Action<object?, object?> setAttachedHead,
        Func<object?, object?> getDecalWallNext,
        Action<object?, object?> linkDecalAfter,
        Func<int, bool> textureAllowsDecals,
        Action<object, double, double> calcFracPos)
    {
        d.Side = wall;
        d.WallPrev = getAttachedHead(wall);
        var tail = d.WallPrev;
        while (tail != null && getDecalWallNext(tail) != null)
            tail = getDecalWallNext(tail);
        if (tail != null)
            linkDecalAfter(tail, d);
        else
            setAttachedHead(wall, d);
        d.WallNext = null;

        int tex;
        if (backSector == null)
        {
            d.RenderFlags |= RfRelMid;
            d.Z -= (lineFlags & MlDontpegbottom) != 0
                ? getPlaneTexZ(wall, PlaneFloor)
                : getPlaneTexZ(wall, PlaneCeiling);
            tex = getTexture(wall, SideMid);
        }
        else if (floorZAt(x, y) >= d.Z)
        {
            d.RenderFlags |= RfRelLower | RfClipLower;
            d.Z -= (lineFlags & MlDontpegbottom) != 0
                ? getPlaneTexZ(wall, PlaneCeiling)
                : getPlaneTexZ(backSector, PlaneFloor);
            tex = getTexture(wall, SideBottom);
        }
        else if (ceilingZAt(x, y) <= d.Z)
        {
            d.RenderFlags |= RfRelUpper | RfClipUpper;
            d.Z -= (lineFlags & MlDontpegtop) != 0
                ? getPlaneTexZ(wall, PlaneCeiling)
                : getPlaneTexZ(backSector, PlaneCeiling);
            tex = getTexture(wall, SideTop);
        }
        else if (ffloor != null)
        {
            d.RenderFlags |= RfRelMid | RfClipMid;
            d.Z -= (lineFlags & MlDontpegbottom) != 0
                ? getPlaneTexZ(wall, PlaneFloor)
                : getPlaneTexZ(wall, PlaneCeiling);
            tex = getTexture(wall, SideMid);
        }
        else
            return InvalidTexture;

        calcFracPos(wall, x, y);
        if (!textureAllowsDecals(tex))
            return InvalidTexture;
        return tex;
    }

    public static double GetRealZ(DecalState d, bool wallIsSide0, int lineFlags, Func<object, int, double> frontPlaneTexZ, Func<object, int, double> backPlaneTexZ, object frontSector, object? backSector)
    {
        backSector ??= frontSector;
        switch (d.RenderFlags & RfRelMask)
        {
            default:
                return d.Z;
            case RfRelUpper:
                return d.Z + ((lineFlags & MlDontpegtop) != 0
                    ? frontPlaneTexZ(frontSector, PlaneCeiling)
                    : backPlaneTexZ(backSector, PlaneCeiling));
            case RfRelLower:
                return d.Z + ((lineFlags & MlDontpegbottom) != 0
                    ? frontPlaneTexZ(frontSector, PlaneCeiling)
                    : backPlaneTexZ(backSector, PlaneFloor));
            case RfRelMid:
                return d.Z + ((lineFlags & MlDontpegbottom) != 0
                    ? frontPlaneTexZ(frontSector, PlaneFloor)
                    : frontPlaneTexZ(frontSector, PlaneCeiling));
        }
    }

    public static void CalcFracPos(DecalState d, bool wallIsSide0, double x, double y, double v1x, double v1y, double v2x, double v2y)
    {
        var dx = v2x - v1x;
        var dy = v2y - v1y;
        if (Math.Abs(dx) > Math.Abs(dy))
            d.LeftDistance = (x - v1x) / dx;
        else if (dy != 0)
            d.LeftDistance = (y - v1y) / dy;
        else
            d.LeftDistance = 0;
    }
}
