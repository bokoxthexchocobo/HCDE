namespace HCDE.Playsim;

/// <summary>
/// Visplane hash, the fake-plane keep, and the height redraw from
/// <c>r_visibleplanelist.cpp</c> (1-392). Chunk 1317.
/// </summary>
public sealed class VmVisiblePlanes1317
{
    public const int MaxVisplanes = 128;
    public const int Opaque = 65536;
    public const ushort EmptyColumn = 0x7fff;

    readonly VisPlane?[] _slots = new VisPlane?[MaxVisplanes + 1];

    public VmVisiblePlanes1317()
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i] = null;
    }

    public VmVisiblePlanes1317(bool mainThread)
        : this()
    {
        MainThread = mainThread;
    }

    public int ViewWidth { get; set; } = 320;

    public int SkyFlat { get; set; }

    public int ViewpointExtraLight { get; set; }

    public bool MainThread { get; set; }

    public bool ViewActive { get; set; }

    public int PortalUniq { get; set; }

    public int PortalMirror { get; set; }

    public int CurrentSkybox { get; set; }

    public int StackedExtraLight { get; set; }

    public double StackedVisibility { get; set; }

    public Vec3 StackedViewPos { get; set; }

    public double StackedYaw { get; set; }

    public Vec3 ViewPos { get; set; }

    public double ViewYaw { get; set; }

    public Func<PlanePortal, bool>? InsideSky { get; set; }

    public int Clocks { get; private set; }

    public int Unclocks { get; private set; }

    public List<PlaneDraw> Draws { get; } = new();

    public VisPlane? Slot(int hash)
    {
        return _slots[hash];
    }

    public VisPlane FindPlane(
        PlaneEq height,
        int picnum,
        int lightlevel,
        bool foggy,
        double alphaValue,
        bool additive,
        Xform xxform,
        int sky,
        PlanePortal? portal,
        object? colormap,
        FakeKind fake,
        int fakeAlpha)
    {
        Xform xform = xxform;
        int alpha = FloatToFixed(alphaValue);
        lightlevel += ActualExtraLight(foggy);
        bool skybox;
        PlaneEq plane;
        if (picnum == SkyFlat)
        {
            lightlevel = 0;
            xform = new Xform
            {
                XScale = 1,
                YScale = 1,
            };
            additive = false;
            plane = new PlaneEq();
            plane.Set(0, 0, height.Z, 0);
            skybox = portal != null && !InSky(portal);
        }
        else if (portal != null && !InSky(portal))
        {
            plane = height;
            skybox = true;
        }
        else
        {
            plane = height;
            skybox = false;
            if (fake == FakeKind.FakeFloor || fake == FakeKind.FakeCeiling)
                sky = unchecked((int)(0x80000000u | (uint)fakeAlpha));
            else
                sky = 0;
            portal = null;
            alpha = Opaque;
        }

        int hash = skybox
            ? MaxVisplanes
            : CalcHash(picnum, lightlevel, height.D);
        PlanePortal? sought = portal;
        for (VisPlane? check = _slots[hash]; check != null; check = check.Next)
        {
            if (skybox)
            {
                if (sought == check.Portal && plane.Same(check.Height))
                {
                    if (sought!.Kind != PortalKind.SkyViewpoint)
                    {
                        if (check.ExtraLight == StackedExtraLight
                            && check.Visibility == StackedVisibility
                            && check.ViewPos.Same(StackedViewPos)
                            && (sought.Kind != PortalKind.StackedSectorThing
                                || (check.Alpha == alpha
                                    && check.Additive == additive
                                    && (alpha == 0
                                        || (plane.Same(check.Height)
                                            && picnum == check.Picnum
                                            && lightlevel == check.LightLevel
                                            && colormap == check.Colormap
                                            && xform.Same(check.Xform)))
                                    && check.ViewAngle == StackedYaw)))
                            return check;
                    }
                    else
                    {
                        return check;
                    }
                }
            }
            else if (plane.Same(check.Height)
                && picnum == check.Picnum
                && lightlevel == check.LightLevel
                && colormap == check.Colormap
                && xform.Same(check.Xform)
                && sky == check.Sky
                && PortalUniq == check.CurrentPortalUniq
                && PortalMirror == check.MirrorFlags
                && CurrentSkybox == check.CurrentSkybox
                && ViewPos.Same(check.ViewPos))
            {
                return check;
            }
        }

        VisPlane created = Add(hash);
        created.Height = plane;
        created.Picnum = picnum;
        created.LightLevel = lightlevel;
        created.Xform = xform;
        created.Colormap = colormap;
        created.Sky = sky;
        created.Portal = sought;
        created.ExtraLight = StackedExtraLight;
        created.Visibility = StackedVisibility;
        created.ViewPos = StackedViewPos;
        created.ViewAngle = StackedYaw;
        created.Alpha = alpha;
        created.Additive = additive;
        created.CurrentPortalUniq = PortalUniq;
        created.MirrorFlags = PortalMirror;
        created.CurrentSkybox = CurrentSkybox;
        return created;
    }

    public VisPlane GetRange(VisPlane pl, int start, int stop)
    {
        if (start < 0 || start >= ViewWidth || stop <= start || stop > ViewWidth)
            throw new InvalidOperationException("visplane range");

        int intrl;
        int unionl;
        if (start < pl.Left)
        {
            intrl = pl.Left;
            unionl = start;
        }
        else
        {
            unionl = pl.Left;
            intrl = start;
        }

        int intrh;
        int unionh;
        if (stop > pl.Right)
        {
            intrh = pl.Right;
            unionh = stop;
        }
        else
        {
            unionh = pl.Right;
            intrh = stop;
        }

        int x = intrl;
        if (x < intrh)
        {
            ushort[] columns = pl.Top!;
            while (x < intrh && columns[x] == EmptyColumn)
                x++;
        }

        if (x >= intrh)
        {
            pl.Left = unionl;
            pl.Right = unionh;
            return pl;
        }

        int hash = pl.Portal != null && !InSky(pl.Portal) && ViewActive
            ? MaxVisplanes
            : CalcHash(pl.Picnum, pl.LightLevel, pl.Height.D);
        VisPlane created = Add(hash);
        created.Height = pl.Height;
        created.Picnum = pl.Picnum;
        created.LightLevel = pl.LightLevel;
        created.Xform = pl.Xform;
        created.Colormap = pl.Colormap;
        created.Portal = pl.Portal;
        created.ExtraLight = pl.ExtraLight;
        created.Visibility = pl.Visibility;
        created.ViewPos = pl.ViewPos;
        created.ViewAngle = pl.ViewAngle;
        created.Sky = pl.Sky;
        created.Alpha = pl.Alpha;
        created.Additive = pl.Additive;
        created.CurrentPortalUniq = pl.CurrentPortalUniq;
        created.MirrorFlags = pl.MirrorFlags;
        created.CurrentSkybox = pl.CurrentSkybox;
        created.Lights = pl.Lights;
        created.Left = start;
        created.Right = stop;
        return created;
    }

    public bool HasPortalPlanes()
    {
        return _slots[MaxVisplanes] != null;
    }

    public VisPlane? PopFirstPortalPlane()
    {
        VisPlane? pl = _slots[MaxVisplanes];
        if (pl != null)
        {
            _slots[MaxVisplanes] = pl.Next;
            pl.Next = null;
        }

        return pl;
    }

    public void ClearPortalPlanes()
    {
        _slots[MaxVisplanes] = null;
    }

    public int Render()
    {
        if (MainThread)
            Clocks++;

        int count = 0;
        for (int i = 0; i < MaxVisplanes; i++)
        {
            for (VisPlane? pl = _slots[i]; pl != null; pl = pl.Next)
            {
                if (pl.CurrentPortalUniq != PortalUniq || pl.CurrentSkybox != CurrentSkybox)
                    continue;
                if (pl.Sky >= 0)
                {
                    count++;
                    Draw(pl, Opaque, false, false);
                }
            }
        }

        if (MainThread)
            Unclocks++;
        return count;
    }

    public void RenderHeight(double height)
    {
        Vec3 savedPos = ViewPos;
        double savedYaw = ViewYaw;
        for (int i = 0; i < MaxVisplanes; i++)
        {
            for (VisPlane? pl = _slots[i]; pl != null; pl = pl.Next)
            {
                if (pl.CurrentSkybox != CurrentSkybox || pl.CurrentPortalUniq != PortalUniq)
                    continue;
                if (pl.Sky < 0 && pl.Height.Zat0() == height)
                {
                    ViewPos = pl.ViewPos;
                    ViewYaw = pl.ViewAngle;
                    PortalMirror = pl.MirrorFlags;
                    Draw(pl, pl.Sky & 0x7FFFFFFF, pl.Additive, true);
                }
            }
        }

        ViewPos = savedPos;
        ViewYaw = savedYaw;
    }

    public void Clear()
    {
        for (int i = 0; i <= MaxVisplanes; i++)
            _slots[i] = null;
    }

    public void ClearKeepFakePlanes()
    {
        for (int i = 0; i <= MaxVisplanes - 1; i++)
        {
            VisPlane? cursor = _slots[i];
            VisPlane? previous = null;
            while (cursor != null)
            {
                if (cursor.Sky < 0)
                {
                    previous = cursor;
                    cursor = cursor.Next;
                }
                else
                {
                    VisPlane removed = cursor;
                    cursor = cursor.Next;
                    removed.Next = null;
                    if (previous == null)
                        _slots[i] = cursor;
                    else
                        previous.Next = cursor;
                }
            }
        }
    }

    public static int FloatToFixed(double value)
    {
        return (int)Math.Round(value * 65536.0, MidpointRounding.ToEven);
    }

    public static int CalcHash(int picnum, int lightlevel, double distance)
    {
        int sum = unchecked(picnum * 3 + lightlevel + FloatToFixed(distance) * 7);
        return (int)((uint)sum & (uint)(MaxVisplanes - 1));
    }

    VisPlane Add(int hash)
    {
        var created = new VisPlane(ViewWidth)
        {
            Next = _slots[hash],
        };
        _slots[hash] = created;
        return created;
    }

    int ActualExtraLight(bool fog)
    {
        if (fog)
            return 0;
        return ViewpointExtraLight << 4;
    }

    bool InSky(PlanePortal portal)
    {
        Func<PlanePortal, bool>? inside = InsideSky;
        if (inside == null)
            return false;
        return inside(portal);
    }

    void Draw(VisPlane plane, int alpha, bool additive, bool masked)
    {
        Draws.Add(new PlaneDraw(plane, alpha, additive, masked, ViewPos, ViewYaw, PortalMirror));
    }

    public enum FakeKind
    {
        Normal,
        FakeFloor,
        FakeCeiling,
        FakeBack,
    }

    public enum PortalKind
    {
        SkyViewpoint,
        StackedSectorThing,
        Other,
    }

    public sealed class PlanePortal
    {
        public PortalKind Kind { get; init; }
    }

    public struct PlaneEq
    {
        public double X;
        public double Y;
        public double Z;
        public double D;
        public double NegiC;

        public void Set(double a, double b, double c, double d)
        {
            X = a;
            Y = b;
            Z = c;
            D = d;
            NegiC = -1 / c;
        }

        public double Zat0()
        {
            return NegiC * D;
        }

        public bool Same(PlaneEq other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && D == other.D;
        }
    }

    public struct Xform
    {
        public double XOffs;
        public double YOffs;
        public double BaseYOffs;
        public double XScale;
        public double YScale;
        public double Angle;
        public double BaseAngle;

        public bool Same(Xform other)
        {
            return XOffs == other.XOffs
                && YOffs + BaseYOffs == other.YOffs + other.BaseYOffs
                && XScale == other.XScale
                && YScale == other.YScale
                && Angle + BaseAngle == other.Angle + other.BaseAngle;
        }
    }

    public struct Vec3
    {
        public double X;
        public double Y;
        public double Z;

        public bool Same(Vec3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
    }

    public sealed class VisPlane
    {
        public VisPlane(int viewWidth)
        {
            Left = viewWidth;
            Right = 0;
        }

        public VisPlane? Next;
        public object? Colormap;
        public PlanePortal? Portal;
        public object? Lights;
        public Xform Xform;
        public PlaneEq Height;
        public int Picnum;
        public int LightLevel;
        public int Left;
        public int Right;
        public int Sky;
        public int ExtraLight;
        public double Visibility;
        public Vec3 ViewPos;
        public double ViewAngle;
        public int Alpha;
        public bool Additive;
        public int CurrentSkybox;
        public int CurrentPortalUniq;
        public int MirrorFlags;
        public ushort[]? Top;
    }

    public readonly record struct PlaneDraw(
        VisPlane Plane,
        int Alpha,
        bool Additive,
        bool Masked,
        Vec3 Pos,
        double Yaw,
        int Mirror);
}
