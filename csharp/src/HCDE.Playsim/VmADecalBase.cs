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

    public enum StickPlacementBranch
    {
        None,
        OneSided,
        Lower,
        Upper,
        FFloor,
    }

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

    public static void RunOnDestroyPipeline(DecalState d, Action<DecalState>? remove, Action? superOnDestroy)
    {
        remove?.Invoke(d);
        superOnDestroy?.Invoke();
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

    public sealed class DecalSerializeSink
    {
        public object? WallPrev { get; set; }
        public object? WallNext { get; set; }
        public double LeftDistance { get; set; }
        public double Z { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double Alpha { get; set; }
        public uint AlphaColor { get; set; }
        public int Translation { get; set; }
        public int PicNum { get; set; }
        public int RenderFlags { get; set; }
        public int RenderStyle { get; set; }
        public object? Side { get; set; }
        public object? Sector { get; set; }
    }

    public static void RunSerializeFieldsPipeline(DecalState d, DecalSerializeSink sink)
    {
        sink.WallPrev = d.WallPrev;
        sink.WallNext = d.WallNext;
        sink.LeftDistance = d.LeftDistance;
        sink.Z = d.Z;
        sink.ScaleX = d.ScaleX;
        sink.ScaleY = d.ScaleY;
        sink.Alpha = d.Alpha;
        sink.AlphaColor = d.AlphaColor;
        sink.Translation = d.Translation;
        sink.PicNum = d.PicNum;
        sink.RenderFlags = d.RenderFlags;
        sink.RenderStyle = d.RenderStyle;
        sink.Side = d.Side;
        sink.Sector = d.Sector;
    }

    public static (double V1X, double V1Y, double V2X, double V2Y) ResolveWallVertices(
        bool wallIsSide0,
        double lineV1X,
        double lineV1Y,
        double lineV2X,
        double lineV2Y)
    {
        if (wallIsSide0)
            return (lineV1X, lineV1Y, lineV2X, lineV2Y);
        return (lineV2X, lineV2Y, lineV1X, lineV1Y);
    }

    public static void GetXY(DecalState d, bool wallIsSide0, double v1x, double v1y, double v2x, double v2y, out double ox, out double oy)
    {
        var verts = ResolveWallVertices(wallIsSide0, v1x, v1y, v2x, v2y);
        var dx = verts.V2X - verts.V1X;
        var dy = verts.V2Y - verts.V1Y;
        ox = verts.V1X + d.LeftDistance * dx;
        oy = verts.V1Y + d.LeftDistance * dy;
    }

    public static void SetShadeRgb(DecalState d, uint rgb, Func<int, int, int, int> colorMatcherPick) =>
        d.AlphaColor = rgb | (uint)(colorMatcherPick((int)(rgb & 0xff), (int)((rgb >> 8) & 0xff), (int)((rgb >> 16) & 0xff)) << 24);

    public static void SetShadeRgb(DecalState d, int r, int g, int b, Func<int, int, int, int> colorMatcherPick) =>
        d.AlphaColor = (uint)(colorMatcherPick(r, g, b) << 24 | (r << 16) | (g << 8) | b);

    public static object? FindWallListTail(object? head, Func<object?, object?> getDecalWallNext)
    {
        var tail = head;
        while (tail != null && getDecalWallNext(tail) != null)
            tail = getDecalWallNext(tail);
        return tail;
    }

    public static void RunLinkDecalToWall(
        DecalState d,
        object wall,
        Func<object?, object?> getAttachedHead,
        Action<object?, object?> setAttachedHead,
        Func<object?, object?> getDecalWallNext,
        Action<object?, object?> linkDecalAfter)
    {
        d.Side = wall;
        d.WallPrev = getAttachedHead(wall);
        var tail = FindWallListTail(d.WallPrev, getDecalWallNext);
        if (tail != null)
            linkDecalAfter(tail, d);
        else
            setAttachedHead(wall, d);
        d.WallNext = null;
    }

    public static StickPlacementBranch ClassifyStickPlacement(
        double z,
        double floorZAtPoint,
        double ceilingZAtPoint,
        bool hasFfloor,
        bool hasBackSector) =>
        !hasBackSector ? StickPlacementBranch.OneSided
        : floorZAtPoint >= z ? StickPlacementBranch.Lower
        : ceilingZAtPoint <= z ? StickPlacementBranch.Upper
        : hasFfloor ? StickPlacementBranch.FFloor
        : StickPlacementBranch.None;

    public static double PeggedZOffset(int lineFlags, bool dontpegBottom, double floorTexZ, double ceilingTexZ) =>
        dontpegBottom ? floorTexZ : ceilingTexZ;

    public static bool UsesDontpegBottom(int lineFlags) => (lineFlags & MlDontpegbottom) != 0;

    public static bool UsesDontpegTop(int lineFlags) => (lineFlags & MlDontpegtop) != 0;

    public static int ResolveFfTexture(
        int ffFlags,
        Func<int> wallTopTex,
        Func<int> wallBottomTex,
        Func<int> masterMidTex)
    {
        if ((ffFlags & FfUppertexture) != 0)
            return wallTopTex();
        if ((ffFlags & FfLowertexture) != 0)
            return wallBottomTex();
        return masterMidTex();
    }

    public static bool RunOneSidedStickBranch(
        DecalState d,
        int lineFlags,
        Func<double> floorTexZ,
        Func<double> ceilingTexZ,
        Func<int> midTex)
    {
        d.RenderFlags |= RfRelMid;
        d.Z -= PeggedZOffset(lineFlags, UsesDontpegBottom(lineFlags), floorTexZ(), ceilingTexZ());
        return true;
    }

    public static bool RunLowerStickBranch(
        DecalState d,
        int lineFlags,
        Func<double> frontCeilingTexZ,
        Func<double> backFloorTexZ,
        Func<int> bottomTex)
    {
        d.RenderFlags |= RfRelLower | RfClipLower;
        d.Z -= UsesDontpegBottom(lineFlags) ? frontCeilingTexZ() : backFloorTexZ();
        return true;
    }

    public static bool RunUpperStickBranch(
        DecalState d,
        int lineFlags,
        Func<double> frontCeilingTexZ,
        Func<double> backCeilingTexZ,
        Func<int> topTex)
    {
        d.RenderFlags |= RfRelUpper | RfClipUpper;
        d.Z -= UsesDontpegTop(lineFlags) ? frontCeilingTexZ() : backCeilingTexZ();
        return true;
    }

    public static bool RunFfStickBranch(
        DecalState d,
        object ffloorSector,
        int lineFlags,
        Func<double> sectorFloorTexZ,
        Func<double> sectorCeilingTexZ)
    {
        d.Sector = ffloorSector;
        d.RenderFlags |= RfRelMid | RfClipMid;
        d.Z -= PeggedZOffset(lineFlags, UsesDontpegBottom(lineFlags), sectorFloorTexZ(), sectorCeilingTexZ());
        return true;
    }

    public sealed class StickToWallContext
    {
        public required DecalState Decal { get; init; }
        public object Wall { get; set; } = new object();
        public double X { get; set; }
        public double Y { get; set; }
        public object? FFloor { get; set; }
        public object? FFloorSector { get; set; }
        public int FFloorFlags { get; set; }
        public bool WallIsSide0 { get; set; }
        public object? BackSector { get; set; }
        public int LineFlags { get; set; }
        public Func<object, int, int> GetTexture { get; set; } = (_, _) => InvalidTexture;
        public Func<object, int, double> GetPlaneTexZ { get; set; } = (_, _) => 0;
        public Func<double, double, double> FloorZAt { get; set; } = (_, _) => 0;
        public Func<double, double, double> CeilingZAt { get; set; } = (_, _) => 0;
        public Func<object?, object?> GetAttachedHead { get; set; } = _ => null;
        public Action<object?, object?> SetAttachedHead { get; set; } = (_, _) => { };
        public Func<object?, object?> GetDecalWallNext { get; set; } = _ => null;
        public Action<object?, object?> LinkDecalAfter { get; set; } = (_, _) => { };
        public Func<int, bool> TextureAllowsDecals { get; set; } = _ => true;
        public Action<object, double, double> CalcFracPos { get; set; } = (_, _, _) => { };
        public Func<int> MasterMidTexture { get; set; } = () => InvalidTexture;
    }

    public static int RunStickPlacementPipeline(StickToWallContext ctx, out int texture)
    {
        texture = InvalidTexture;
        var branch = ClassifyStickPlacement(
            ctx.Decal.Z,
            ctx.FloorZAt(ctx.X, ctx.Y),
            ctx.CeilingZAt(ctx.X, ctx.Y),
            ctx.FFloor != null,
            ctx.BackSector != null);
        switch (branch)
        {
            case StickPlacementBranch.OneSided:
                RunOneSidedStickBranch(
                    ctx.Decal,
                    ctx.LineFlags,
                    () => ctx.GetPlaneTexZ(ctx.Wall, PlaneFloor),
                    () => ctx.GetPlaneTexZ(ctx.Wall, PlaneCeiling),
                    () => ctx.GetTexture(ctx.Wall, SideMid));
                texture = ctx.GetTexture(ctx.Wall, SideMid);
                break;
            case StickPlacementBranch.Lower:
                RunLowerStickBranch(
                    ctx.Decal,
                    ctx.LineFlags,
                    () => ctx.GetPlaneTexZ(ctx.Wall, PlaneCeiling),
                    () => ctx.GetPlaneTexZ(ctx.BackSector!, PlaneFloor),
                    () => ctx.GetTexture(ctx.Wall, SideBottom));
                texture = ctx.GetTexture(ctx.Wall, SideBottom);
                break;
            case StickPlacementBranch.Upper:
                RunUpperStickBranch(
                    ctx.Decal,
                    ctx.LineFlags,
                    () => ctx.GetPlaneTexZ(ctx.Wall, PlaneCeiling),
                    () => ctx.GetPlaneTexZ(ctx.BackSector!, PlaneCeiling),
                    () => ctx.GetTexture(ctx.Wall, SideTop));
                texture = ctx.GetTexture(ctx.Wall, SideTop);
                break;
            case StickPlacementBranch.FFloor:
                RunFfStickBranch(
                    ctx.Decal,
                    ctx.FFloorSector!,
                    ctx.LineFlags,
                    () => ctx.GetPlaneTexZ(ctx.FFloorSector!, PlaneFloor),
                    () => ctx.GetPlaneTexZ(ctx.FFloorSector!, PlaneCeiling));
                texture = ResolveFfTexture(
                    ctx.FFloorFlags,
                    () => ctx.GetTexture(ctx.Wall, SideTop),
                    () => ctx.GetTexture(ctx.Wall, SideBottom),
                    ctx.MasterMidTexture);
                break;
            default:
                texture = InvalidTexture;
                break;
        }
        return texture;
    }

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
        RunLinkDecalToWall(d, wall, getAttachedHead, setAttachedHead, getDecalWallNext, linkDecalAfter);
        var ctx = new StickToWallContext
        {
            Decal = d,
            Wall = wall,
            X = x,
            Y = y,
            FFloor = ffloor,
            WallIsSide0 = wallIsSide0,
            BackSector = backSector,
            LineFlags = lineFlags,
            GetTexture = getTexture,
            GetPlaneTexZ = getPlaneTexZ,
            FloorZAt = floorZAt,
            CeilingZAt = ceilingZAt,
            GetAttachedHead = getAttachedHead,
            SetAttachedHead = setAttachedHead,
            GetDecalWallNext = getDecalWallNext,
            LinkDecalAfter = linkDecalAfter,
            TextureAllowsDecals = textureAllowsDecals,
            CalcFracPos = calcFracPos,
        };
        var tex = RunStickPlacementPipeline(ctx, out _);
        if (tex == InvalidTexture)
            return InvalidTexture;
        calcFracPos(wall, x, y);
        if (!textureAllowsDecals(tex))
            return InvalidTexture;
        return tex;
    }

    public static (object Front, object? Back) ResolveWallSectors(bool wallIsSide0, object frontSector, object? backSector) =>
        wallIsSide0 ? (frontSector, backSector) : (backSector ?? frontSector, frontSector);

    public static double RunGetRealZPipeline(
        DecalState d,
        int lineFlags,
        Func<object, int, double> frontPlaneTexZ,
        Func<object, int, double> backPlaneTexZ,
        object frontSector,
        object? backSector)
    {
        backSector ??= frontSector;
        switch (d.RenderFlags & RfRelMask)
        {
            default:
                return d.Z;
            case RfRelUpper:
                return d.Z + (UsesDontpegTop(lineFlags)
                    ? frontPlaneTexZ(frontSector, PlaneCeiling)
                    : backPlaneTexZ(backSector, PlaneCeiling));
            case RfRelLower:
                return d.Z + (UsesDontpegBottom(lineFlags)
                    ? frontPlaneTexZ(frontSector, PlaneCeiling)
                    : backPlaneTexZ(backSector, PlaneFloor));
            case RfRelMid:
                return d.Z + (UsesDontpegBottom(lineFlags)
                    ? frontPlaneTexZ(frontSector, PlaneFloor)
                    : frontPlaneTexZ(frontSector, PlaneCeiling));
        }
    }

    public static double GetRealZ(DecalState d, bool wallIsSide0, int lineFlags, Func<object, int, double> frontPlaneTexZ, Func<object, int, double> backPlaneTexZ, object frontSector, object? backSector) =>
        RunGetRealZPipeline(d, lineFlags, frontPlaneTexZ, backPlaneTexZ, frontSector, backSector);

    public static void CalcFracPos(DecalState d, bool wallIsSide0, double x, double y, double v1x, double v1y, double v2x, double v2y)
    {
        var verts = ResolveWallVertices(wallIsSide0, v1x, v1y, v2x, v2y);
        var dx = verts.V2X - verts.V1X;
        var dy = verts.V2Y - verts.V1Y;
        if (Math.Abs(dx) > Math.Abs(dy))
            d.LeftDistance = (x - verts.V1X) / dx;
        else if (dy != 0)
            d.LeftDistance = (y - verts.V1Y) / dy;
        else
            d.LeftDistance = 0;
    }
}
