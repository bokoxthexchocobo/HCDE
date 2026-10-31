namespace HCDE.Playsim;

/// <summary>
/// Sky offset, cylinder width, and the column stripe from <c>r_skyplane.cpp</c>
/// (1-307). Chunk 1285. Cap colors are a host pair. The short draw height on
/// the tall-sky path is computed and not passed to the drawer.
/// </summary>
public sealed class VmSkyTex1285
{
    public int Id;
    public int ScaledHeight;
    public int Height = 1;
    public int PhysicalHeight = 1;
    public int HeightBits;
    public int Width;
    public double ScaleX = 1;
    public double ScaleY = 1;
    public double SkyOffset;
    public bool Valid = true;
    public bool Missing;
    public double XOffset;
    public double YOffset;
}

public sealed class VmSkyLine1285
{
    public VmSkyTex1285? Top;
    public VmSkyTex1285? Bottom;
    public int Arg2;
}

public sealed class VmSkyPlane1285
{
    public const int SkyFlatFlag = 0x40000000;
    public const int LevelDoubleSky = 0x4;
    public const int LevelForceTiled = 0x2000;
    public const int LevelSwapSkies = 0x10000000;
    public const int StretchHeight = 228;
    public const uint Angle90 = 0x40000000;

    public VmSkyTex1285? Sky1;
    public VmSkyTex1285? Sky2;
    public bool OldFreeLook;
    public int SkyMode;
    public int Flags;
    public bool Stretch;
    public int ViewWidth;
    public int ViewHeight;
    public double FreeLookHeight;
    public double FovDegrees = 90;
    public double YawDegrees;
    public double CenterY = 100;
    public double FocalTangent = 1;
    public bool Mirror;
    public bool LinearSky;
    public double Sky1Pos;
    public double Sky2Pos;
    public int SkyFlatPic = 1;
    public uint[] XToView = new uint[512];
    public List<VmSkyLine1285> Lines = new();
    public Func<VmSkyTex1285, (int Top, int Bottom)> Cap = DefaultCap;

    public double SkyTextureMid;
    public double SkyScale;
    public float SkyIScale;
    public int Sky1Cyl;
    public int Sky2Cyl;
    public int SkyOffset;
    public int FrontId;
    public int BackId;
    public uint SkyFlip;
    public int FrontPos;
    public int BackPos;
    public int FrontCyl;
    public int BackCyl;
    public double SkyMid;
    public uint SkyAngle;
    public bool Fade;
    public int StyleCalls;
    public short LastDrawHeight;
    public List<string> Columns = new();

    public void Build()
    {
        SkyTextureMid = 0;
        SkyScale = 0;
        SkyIScale = 0;
        Sky1Cyl = 0;
        Sky2Cyl = 0;
        SkyOffset = 0;
        if (Sky1 == null || Sky2 == null)
            return;
        int skyHeight = Sky1.ScaledHeight;
        SkyOffset = OldFreeLook ? 0 : skyHeight == 256 ? 166 : skyHeight >= 240 ? 150 : skyHeight >= 200 ? 110 : 138;
        if (skyHeight >= 128 && skyHeight < 200)
            SkyTextureMid = -28;
        else if (skyHeight >= 200)
        {
            double tiled = SkyMode == 2 && (Flags & LevelForceTiled) == 0 ? Sky1.SkyOffset : 0;
            SkyTextureMid = (200 - skyHeight) * Sky1.ScaleY + tiled;
        }
        if (ViewWidth != 0 && ViewHeight != 0)
        {
            SkyIScale = (float)(200.0 / FreeLookHeight);
            SkyScale = FreeLookHeight / 200.0;
            SkyIScale *= (float)(FovDegrees / 90.0);
            SkyScale *= (float)(90.0 / FovDegrees);
        }
        if (Stretch)
        {
            SkyScale *= (StretchHeight + SkyOffset) / (double)skyHeight;
            SkyIScale *= skyHeight / (float)(StretchHeight + SkyOffset);
            SkyTextureMid *= skyHeight / (double)(StretchHeight + SkyOffset);
        }
        Sky1Cyl = Math.Max(Sky1.Width, Trunc(Sky1.ScaleX * 1024));
        Sky2Cyl = Math.Max(Sky2.Width, Trunc(Sky2.ScaleY * 1024));
    }

    public void Render(int pic, int sky, int left, int right, int[] top, int[] bottom)
    {
        Columns.Clear();
        VmSkyTex1285? sky1 = Sky1;
        if ((Flags & LevelSwapSkies) != 0 && (Flags & LevelDoubleSky) == 0)
            sky1 = Sky2;
        VmSkyTex1285? sky2 = Sky2;
        SkyMid = SkyTextureMid;
        SkyAngle = Bams(YawDegrees);
        double frontDelta = 0;
        double backDelta = 0;
        VmSkyTex1285? front = null;
        VmSkyTex1285? back = null;
        if (pic == SkyFlatPic)
        {
            if ((sky & SkyFlatFlag) == 0)
            {
                front = Resolve(sky1);
                back = (Flags & LevelDoubleSky) != 0 ? Resolve(sky2) : null;
                SkyFlip = 0;
                frontDelta = Sky1Pos;
                backDelta = Sky2Pos;
                FrontCyl = Sky1Cyl;
                BackCyl = Sky2Cyl;
            }
            else if (sky == SkyFlatFlag)
            {
                front = Resolve(sky2);
                back = null;
                FrontCyl = Sky2Cyl;
                SkyFlip = 0;
                frontDelta = Sky2Pos;
            }
            else
            {
                int index = (int)((uint)sky & ~SkyFlatFlag) - 1;
                VmSkyLine1285 line = Lines[index];
                VmSkyTex1285? side = line.Top;
                if ((Flags & LevelSwapSkies) != 0 && line.Bottom != null && line.Bottom.Valid)
                    side = line.Bottom;
                front = Resolve(side);
                if (front == null)
                {
                    front = Resolve(sky1);
                    back = (Flags & LevelDoubleSky) != 0 ? Resolve(sky2) : null;
                    SkyFlip = 0;
                    frontDelta = Sky1Pos;
                    backDelta = Sky2Pos;
                    FrontCyl = Sky1Cyl;
                    BackCyl = Sky2Cyl;
                }
                else
                {
                    back = null;
                    SkyAngle = unchecked(SkyAngle + (uint)FloatToFixed(side!.XOffset));
                    SkyMid = side.YOffset - 28.0;
                    SkyFlip = line.Arg2 != 0 ? 0u : ~0u;
                    FrontCyl = Math.Max(front.Width, Trunc(front.ScaleX * 1024));
                    if (Stretch)
                        SkyMid = SkyMid * front.ScaledHeight / (StretchHeight + SkyOffset);
                }
            }
        }
        FrontId = front?.Id ?? 0;
        BackId = back?.Id ?? 0;
        FrontPos = Trunc(Fmod(frontDelta, Sky1Cyl * 65536.0));
        if (back != null)
            BackPos = Trunc(Fmod(backDelta, Sky2Cyl * 65536.0));
        StyleCalls++;
        Fade = SkyMode == 2 && (Flags & LevelForceTiled) == 0;
        for (int x = left; x < right; x++)
        {
            int y1 = top[x];
            int y2 = bottom[x];
            if (y2 <= y1)
                continue;
            DrawColumn(front!, back, x, y1, y2);
        }
    }

    void DrawColumn(VmSkyTex1285 front, VmSkyTex1285? back, int startX, int y1, int y2)
    {
        if ((1 << front.HeightBits) >= front.PhysicalHeight)
        {
            double mid = SkyMid * front.ScaleY + front.Height;
            Stripe(front, back, startX, y1, y2, mid, front.ScaleY);
        }
        else
        {
            double yrepeat = front.ScaleY;
            double scale = front.ScaleY * SkyScale;
            double iscale = 1 / scale;
            LastDrawHeight = unchecked((short)Math.Truncate(front.Height * scale));
            double topFrac = Fmod(SkyMid + iscale * (1 - CenterY), front.Height);
            if (topFrac < 0)
                topFrac += front.Height;
            double mid = topFrac - iscale * (1 - CenterY);
            Stripe(front, back, startX, y1, y2, mid, yrepeat);
        }
    }

    void Stripe(VmSkyTex1285 front, VmSkyTex1285? back, int startX, int y1, int y2, double textureMid, double yrepeat)
    {
        double stepD = SkyIScale * yrepeat;
        double v = (textureMid + stepD * (y1 - CenterY + 0.5)) / front.Height;
        double vStep = stepD / front.Height;
        uint uvPos = Uv(v);
        uint uvStep = Uv(vStep);
        int x = startX;
        if (Mirror)
            x = ViewWidth - x;
        uint ang;
        if (LinearSky)
        {
            uint xAngle = ToAngle((0.5 - x / (double)ViewWidth) * FocalTangent * Angle90);
            ang = (SkyAngle + xAngle) ^ SkyFlip;
        }
        else
            ang = (SkyAngle + XToView[x]) ^ SkyFlip;
        uint angle1 = Mul16(ang, unchecked((uint)FrontCyl)) + unchecked((uint)FrontPos);
        uint angle2 = Mul16(ang, unchecked((uint)BackCyl)) + unchecked((uint)BackPos);
        (int top, int bottom) = Cap(front);
        string kind = back == null ? "single" : "double";
        Columns.Add(kind + " " + startX + " " + y1 + " " + (y2 - y1) + " " + uvPos + " " + uvStep + " " + angle1 + " " + angle2 + " " + (Fade ? 1 : 0) + " " + top + " " + bottom + " " + x);
    }

    static VmSkyTex1285? Resolve(VmSkyTex1285? tex)
    {
        if (tex == null || tex.Missing)
            return null;
        return tex;
    }

    public static uint Bams(double degrees)
    {
        return unchecked((uint)RoundHalfEven(degrees * (Angle90 / 90.0)));
    }

    public static int FloatToFixed(double value)
    {
        return (int)Math.Round(value * 65536.0, MidpointRounding.ToEven);
    }

    public static int RoundHalfEven(double value)
    {
        return (int)Math.Round(value, MidpointRounding.ToEven);
    }

    public static int Trunc(double value)
    {
        return (int)Math.Truncate(value);
    }

    public static double Fmod(double value, double divisor)
    {
        return value - Math.Truncate(value / divisor) * divisor;
    }

    public static uint Uv(double value)
    {
        return unchecked((uint)Trunc(value * 0x01000000));
    }

    public static uint ToAngle(double value)
    {
        return unchecked((uint)(long)Math.Truncate(value));
    }

    public static uint Mul16(uint a, uint b)
    {
        return (uint)(((ulong)a * b) >> 16);
    }

    static (int Top, int Bottom) DefaultCap(VmSkyTex1285 tex)
    {
        return (0, 0);
    }
}
