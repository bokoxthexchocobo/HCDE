using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Particle projection, square sprite edges, and the masked segment test from
/// <c>r_particle.cpp</c> (1-293). Chunk 1282. Column drawing and colormap
/// lookup are host seams. The portal test uses the particle position before
/// velocity, and the vertical scale stored on the sprite is the horizontal scale.
/// </summary>
public sealed class VmParticlePlane1282
{
    public int Pic = -1;
    public Func<double, double, double> Height = FlatHeight;

    public static double FlatHeight(double x, double y)
    {
        return 0;
    }
    public int Colormap;
    public int SpriteColor;
}

public sealed class VmParticleSeg1282
{
    public int X1;
    public int X2;
    public float Sz1 = 1;
    public float Sz2 = 1;
    public float Sx1;
    public float Sx2 = 1;
    public int PortalUniq;
}

public sealed class VmParticleVis1282
{
    public int? HeightSector;
    public int XScaleFixed;
    public float YScale;
    public float Depth;
    public float IDepth;
    public float Gx;
    public float Gy;
    public float Gz;
    public int X1;
    public int X2;
    public int Y1;
    public int Y2;
    public int StartFrac;
    public short RenderFlags;
    public int FakeSide;
    public bool Foggy;
    public bool FullBright;
    public double LightTz;
    public int LightLevel;
    public int ColormapNum;
    public int MapColormap;
    public int MapSprite;
    public bool NoColored;
    public int PortalUniq;
    public List<string> Draws = new();
    public List<string> Masked = new();

    public void Render(VmParticle1282 host, short[]? clipTop, short[]? clipBottom, int minZ, int maxZ, int clipFloor)
    {
        int color = host.ColorMap[StartFrac];
        int yl = Y1;
        int yCount = Y2 - yl + 1;
        int countBase = X2 - X1;
        if (yCount <= 0 || countBase <= 0)
        {
            host.Reject = "empty";
            return;
        }
        DrawMasked(host, clipFloor);
        uint fg = host.Shade(color, ColormapNum);
        int fgLevel = ((RenderFlags + 1) << 8) & ~0x3ff;
        uint alpha = (uint)(fgLevel * 256 / 65536);
        uint step = (uint)(64 * 65536 / countBase);
        uint pos = step / 2;
        for (int x = X1; x < X1 + countBase; x++, pos += step)
        {
            if (host.ColumnBlocked(x))
                continue;
            Draws.Add(x + " " + yl + " " + yCount + " " + fg + " " + alpha + " " + pos);
        }
        host.LastClipTop = clipTop;
        host.LastClipBottom = clipBottom;
        host.LastZ = minZ + " " + maxZ;
    }

    public void DrawMasked(VmParticle1282 host, int clipFloor)
    {
        foreach (VmParticleSeg1282 seg in host.Segments)
        {
            if (seg.X1 >= X2 || seg.X2 <= X1)
                continue;
            float siz1 = 1f / seg.Sz1;
            float siz2 = 1f / seg.Sz2;
            float mid = (X2 + X1) / 2;
            float depth = (siz2 - siz1) * (mid - seg.Sx1) / (seg.Sx2 - seg.Sx1) + siz1;
            if (depth < IDepth && seg.PortalUniq == PortalUniq)
            {
                int from = Math.Max(seg.X1, X1);
                int to = Math.Min(seg.X2, X2);
                Masked.Add(from + " " + to + " " + clipFloor + " " + VmParticle1282.Num(depth));
            }
        }
    }
}

public sealed class VmParticle1282
{
    public const double MinZ = 8192.0 / 1048576.0;

    public double PosX;
    public double PosY;
    public double PosZ;
    public double VelX;
    public double VelY;
    public double VelZ;
    public double ViewX;
    public double ViewY;
    public double ViewZ;
    public double Sin;
    public double Cos = 1;
    public double TanCos = 1;
    public double TanSin;
    public int ViewCenterX = 160;
    public double CenterY = 100;
    public double YaspectMul = 1;
    public int ViewWidth = 320;
    public int WindowLeft;
    public int WindowRight = 320;
    public bool HasPortal;
    public bool Paused;
    public bool Frozen;
    public bool Mirror;
    public int Flags3;
    public int SkyPic = -2;
    public int PortalUniq;
    public double Size = 8;
    public float Alpha = 1;
    public uint Color;
    public int Flags;
    public int LightLevel;
    public int ColormapNum;
    public bool Foggy;
    public int FakeSide;
    public int? HeightSector;
    public VmParticlePlane1282 SectorCeiling = new();
    public VmParticlePlane1282 SectorFloor = new();
    public VmParticlePlane1282 HeightCeiling = new();
    public VmParticlePlane1282 HeightFloor = new();
    public int[] CeilingClip = Enumerable.Repeat(int.MinValue, 512).ToArray();
    public int[] FloorClip = Enumerable.Repeat(int.MaxValue, 512).ToArray();
    public Func<int, int, uint> Shade = (color, colormap) => (uint)(color + colormap * 65536);
    public int[] ColorMap = new int[256];
    public List<VmParticleSeg1282> Segments = new();
    public Func<double, double, int>? PortalSide;
    public Func<int, bool>? BlockColumn;
    public string Reject = "";
    public short[]? LastClipTop;
    public short[]? LastClipBottom;
    public string LastZ = "";
    public int ColormapCalls;
    public int LastMapColormap;
    public int LastMapSprite;
    public bool LastNoColored;

    public VmParticleVis1282? Project(double timeFrac)
    {
        Reject = "";
        if (Paused || Frozen)
            timeFrac = 0;
        double ipx = PosX + VelX * timeFrac;
        double ipy = PosY + VelY * timeFrac;
        double ipz = PosZ + VelZ * timeFrac;
        if (HasPortal && PortalSide!(PosX, PosY) != 0)
        {
            Reject = "portal";
            return null;
        }
        double trx = ipx - ViewX;
        double try_ = ipy - ViewY;
        double tz = trx * TanCos + try_ * TanSin;
        if (tz < MinZ)
        {
            Reject = "behind";
            return null;
        }
        double tx = trx * Sin - try_ * Cos;
        if (Mirror)
            tx = ViewWidth - tx - 1;
        if (tz <= Math.Abs(tx))
        {
            Reject = "side";
            return null;
        }
        double tiz = 1 / tz;
        double xscale = ViewCenterX * tiz;
        double psize = Size / 8.0;
        int x1 = Math.Max(WindowLeft, ViewCenterX + RoundHalfUp((tx - psize) * xscale));
        int x2 = Math.Min(WindowRight, ViewCenterX + RoundHalfUp((tx + psize) * xscale));
        if (x1 >= x2)
        {
            Reject = "xrange";
            return null;
        }
        double yscale = xscale;
        double ty = (ipz - ViewZ) * YaspectMul;
        int y1 = RoundHalfUp(CenterY - (ty + psize) * yscale);
        int y2 = RoundHalfUp(CenterY - (ty - psize) * yscale);
        if (y1 < CeilingClip[x1])
            y1 = CeilingClip[x1];
        if (y1 < CeilingClip[x2 - 1])
            y1 = CeilingClip[x2 - 1];
        if (y2 >= FloorClip[x1])
            y2 = FloorClip[x1] - 1;
        if (y2 >= FloorClip[x2 - 1])
            y2 = FloorClip[x2 - 1] - 1;
        if (y1 > y2)
        {
            Reject = "yrange";
            return null;
        }
        bool noColored = (Flags3 & 0x10) != 0;
        VmParticlePlane1282 top;
        VmParticlePlane1282 bot;
        int mapColormap;
        int mapSprite;
        if (HeightSector != null)
        {
            if (FakeSide == 2)
            {
                top = SectorCeiling;
                bot = HeightCeiling;
                mapColormap = HeightCeiling.Colormap;
                mapSprite = HeightCeiling.SpriteColor;
            }
            else if (FakeSide == 1)
            {
                top = HeightFloor;
                bot = SectorFloor;
                mapColormap = HeightFloor.Colormap;
                mapSprite = HeightFloor.SpriteColor;
            }
            else
            {
                top = HeightCeiling;
                bot = HeightFloor;
                mapColormap = SectorCeiling.Colormap;
                mapSprite = SectorCeiling.SpriteColor;
            }
        }
        else
        {
            top = SectorCeiling;
            bot = SectorFloor;
            mapColormap = SectorCeiling.Colormap;
            mapSprite = SectorCeiling.SpriteColor;
        }
        ColormapCalls++;
        LastMapColormap = mapColormap;
        LastMapSprite = mapSprite;
        LastNoColored = noColored;
        if (bot.Pic != SkyPic && ipz < bot.Height(PosX, PosY))
        {
            Reject = "floor";
            return null;
        }
        if (top.Pic != SkyPic && ipz >= top.Height(PosX, PosY))
        {
            Reject = "ceiling";
            return null;
        }
        var vis = new VmParticleVis1282
        {
            HeightSector = HeightSector,
            XScaleFixed = FloatToFixed(xscale),
            YScale = (float)xscale,
            Depth = (float)tz,
            IDepth = (float)(1 / tz),
            Gx = (float)ipx,
            Gy = (float)ipy,
            Gz = (float)ipz,
            X1 = x1,
            X2 = x2,
            Y1 = y1,
            Y2 = y2,
            StartFrac = (int)(255 & (Color >> 24)),
            RenderFlags = (short)(Alpha * 255f + 0.5f),
            FakeSide = FakeSide,
            Foggy = Foggy,
            FullBright = (Flags & 1) != 0,
            LightTz = tz,
            LightLevel = LightLevel,
            ColormapNum = ColormapNum,
            MapColormap = mapColormap,
            MapSprite = mapSprite,
            NoColored = noColored,
            PortalUniq = PortalUniq,
        };
        return vis;
    }

    public bool ColumnBlocked(int x)
    {
        return BlockColumn != null && BlockColumn(x);
    }

    public static int RoundHalfUp(double value)
    {
        return (int)Math.Floor(value + 0.5);
    }

    public static int FloatToFixed(double value)
    {
        return (int)Math.Round(value * 65536.0, MidpointRounding.ToEven);
    }

    public static string Num(float value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }
}
