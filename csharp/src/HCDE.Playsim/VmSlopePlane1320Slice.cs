namespace HCDE.Playsim;

/// <summary>
/// Slope normal, fixed-point texture steps, and the tilted span light from
/// <c>r_slopeplane.cpp</c> (1-283). Chunk 1320.
/// </summary>
public sealed class VmSlopePlane1320
{
    public const int XFlip = 0x0001;
    public const int HasFadeTable = 0x00000008;
    public const int Opaque = 65536;
    public const float FixedScale = 4294967296f;

    public static readonly float[] InvPow2 =
    {
        64f, 32f, 16f, 8f, 4f, 2f, 1f, 0.5f,
        0.25f, 0.125f, 0.0625f, 0.03125f, 0.015625f, 0.0078125f,
        0.00390625f, 0.001953125f
    };

    public bool DynLights { get; set; }
    public List<Span> Spans { get; } = new();
    public bool Skipped { get; private set; }

    public Plane? Render(
        Plane plane,
        View view,
        double xScale,
        double yScale,
        int alpha,
        bool additive,
        bool masked,
        int fade,
        Texture tex)
    {
        if (alpha <= 0)
        {
            Skipped = true;
            return null;
        }
        Skipped = false;
        Spans.Clear();
        double z0 = plane.ZAt(view.X, view.Y);
        Vec world0 = new((float)view.X, (float)view.Y, (float)z0);
        Vec normal = plane.Normal;
        Vec world1 = world0 + normal;
        Vec view0 = view.WorldToView(world0);
        Vec view1 = view.WorldToView(world1);
        Vec planeNormal = view1 - view0;
        float planeD = -(view0.Dot(planeNormal));
        if ((view.MirrorFlags & XFlip) != 0)
            planeNormal = new Vec(-planeNormal.X, planeNormal.Y, planeNormal.Z);

        double scaledX = xScale / tex.PhysicalScale;
        double scaledY = yScale / tex.PhysicalScale;
        double lx = scaledX * InvPow2[tex.WidthBits];
        double ly = scaledY * InvPow2[tex.HeightBits];
        double stepX = 64.0 / lx;
        double stepY = 64.0 / ly;
        double zero = plane.ZAt(view.X, view.Y);
        int pViewX = FloatToFixed(plane.XOff * plane.XFormScale, 32 - tex.WidthBits);
        int pViewY = FloatToFixed(plane.YOff * plane.YFormScale, 32 - tex.HeightBits);
        double planeAng = plane.Angle + plane.BaseAngle;

        double ang = Math.PI * 1.5 - view.Yaw;
        double cosine = Math.Cos(ang);
        double sine = Math.Sin(ang);
        var origin = new Vec(
            (float)(view.X * cosine - view.Y * sine),
            (float)(plane.ZAt(0, 0) - view.Z),
            (float)(view.X * sine + view.Y * cosine));
        ang -= Math.PI / 2.0 + planeAng;
        cosine = Math.Cos(ang);
        sine = Math.Sin(ang);
        var axisV = new Vec((float)(stepY * cosine), 0, (float)(stepY * sine));
        var axisU = new Vec((float)(stepX * sine), 0, (float)(-stepX * cosine));
        cosine = Math.Cos(planeAng);
        sine = Math.Sin(planeAng);
        axisV = new Vec(axisV.X, (float)(plane.ZAt(view.X + stepY * sine, view.Y + stepY * cosine) - zero), axisV.Z);
        axisU = new Vec(axisU.X, (float)-(plane.ZAt(view.X - stepX * cosine, view.Y + stepX * sine) - zero), axisU.Z);

        Vec su = origin.Cross(axisV);
        Vec sv = origin.Cross(axisU);
        Vec sz = axisV.Cross(axisU);
        su = new Vec(su.X, (float)(su.Y * view.IYAspect), (float)(su.Z * view.FocalLengthX));
        sv = new Vec(sv.X, (float)(sv.Y * view.IYAspect), (float)(sv.Z * view.FocalLengthX));
        sz = new Vec(sz.X, (float)(sz.Y * view.IYAspect), (float)(sz.Z * view.FocalLengthX));
        su = su.Scale(FixedScale);
        sv = sv.Scale(FixedScale);
        if ((view.MirrorFlags & XFlip) != 0)
        {
            su = new Vec(-su.X, su.Y, su.Z);
            sv = new Vec(-sv.X, sv.Y, sv.Z);
            sz = new Vec(-sz.X, sz.Y, sz.Z);
        }

        bool foggy = view.FadeTo != 0 || fade != 0 || (view.LevelFlags & HasFadeTable) != 0;
        double glob = view.NoLightFade && !foggy ? 0 : view.TiltVisibility;
        double light = glob * lx * ly / Math.Abs(plane.ZAt(view.X, view.Y) - view.Z) / 65536.0;
        if (plane.Normal.Z > 0)
            light = -light;
        bool shade = view.FixedLightLevel < 0 && !view.FixedColormap;
        if (tex.HeightBits == 0)
            sv = new Vec(0, 0, 0);
        if (tex.WidthBits == 0)
            su = new Vec(0, 0, 0);

        plane.PViewX = pViewX;
        plane.PViewY = pViewY;
        plane.PlaneNormal = planeNormal;
        plane.PlaneD = planeD;
        plane.Su = su;
        plane.Sv = sv;
        plane.Sz = sz;
        plane.Foggy = foggy;
        plane.Shade = shade;
        plane.LightFloat = light;
        plane.StyleAlpha = Opaque;
        plane.StyleAdditive = false;
        plane.StyleMasked = false;
        plane.IgnoredAdditive = additive;
        plane.IgnoredMasked = masked;
        foreach (Row row in plane.Rows)
            RenderLine(plane, view, row.Y, row.X1, row.X2);
        return plane;
    }

    public void RenderLine(Plane plane, View view, int y, int x1, int x2)
    {
        var lights = new List<DrawerLight>();
        float stepX = 0, stepY = 0, stepZ = 0;
        float posX = 0, posY = 0, posZ = 0;
        if (DynLights)
        {
            Vec a = view.ScreenToView(x1, y, plane.PlaneNormal, plane.PlaneD);
            Vec b = view.ScreenToView(x2, y, plane.PlaneNormal, plane.PlaneD);
            a = new Vec((float)(a.X / a.Z), (float)(a.Y / a.Z), (float)(1.0 / a.Z));
            b = new Vec((float)(b.X / b.Z), (float)(b.Y / b.Z), (float)(1.0 / b.Z));
            posX = a.X;
            posY = a.Y;
            posZ = a.Z;
            float span = x2 - x1;
            stepX = (b.X - a.X) / span;
            stepY = (b.Y - a.Y) / span;
            stepZ = (b.Z - a.Z) / span;
            for (LightNode? node = plane.Lights; node != null; node = node.Next)
            {
                if (node.Light == null || !node.Light.Active)
                    continue;
                Vec at = view.WorldToView(new Vec((float)node.Light.X, (float)node.Light.Y, (float)node.Light.Z));
                float radius = 256f / node.Light.Radius;
                if (node.Light.Attenuated)
                    radius = -radius;
                lights.Add(new DrawerLight
                {
                    X = at.X,
                    Y = at.Y,
                    Z = at.Z,
                    Radius = radius,
                    Color = (node.Light.Red << 16) | (node.Light.Green << 8) | node.Light.Blue
                });
            }
        }
        Spans.Add(new Span
        {
            Y = y,
            X1 = x1,
            X2 = x2,
            Su = plane.Su,
            Sv = plane.Sv,
            Sz = plane.Sz,
            Shade = plane.Shade,
            LightLevel = plane.Light,
            Foggy = plane.Foggy,
            LightFloat = plane.LightFloat,
            PViewX = plane.PViewX,
            PViewY = plane.PViewY,
            ViewX = posX,
            ViewY = posY,
            ViewZ = posZ,
            StepX = stepX,
            StepY = stepY,
            StepZ = stepZ,
            Normal = plane.PlaneNormal,
            Lights = lights
        });
    }

    public static int FloatToFixed(double value, int bits)
    {
        return (int)Math.Round(value * (1L << bits), MidpointRounding.ToEven);
    }

    public readonly record struct Vec(float X, float Y, float Z)
    {
        public static Vec operator +(Vec a, Vec b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec operator -(Vec a, Vec b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public float Dot(Vec other) => X * other.X + Y * other.Y + Z * other.Z;
        public Vec Cross(Vec other) => new(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X);
        public Vec Scale(float scalar) => new(X * scalar, Y * scalar, Z * scalar);
    }

    public sealed class View
    {
        public double X, Y, Z;
        public double Sin, Cos = 1, Yaw, FocalLengthX = 1, FocalLengthY = 1, IYAspect = 1;
        public double CenterX = 160, CenterY = 100;
        public int MirrorFlags, FadeTo, LevelFlags, FixedLightLevel = -1;
        public bool FixedColormap, NoLightFade;
        public double TiltVisibility = 1;

        public Vec WorldToView(Vec world)
        {
            double tx = world.X - X;
            double ty = world.Y - Y;
            double tz = world.Z - Z;
            return new Vec((float)(tx * Sin - ty * Cos), (float)tz, (float)(tx * Cos + ty * Sin));
        }

        public Vec ScreenToView(int screenX, int screenY, Vec plane, float planeD)
        {
            double viewZ = -planeD / ((screenX + 0.5 - CenterX) / FocalLengthX * plane.X
                + (CenterY - screenY - 0.5) / FocalLengthY * plane.Y + plane.Z);
            return new Vec(
                (float)((screenX + 0.5 - CenterX) / FocalLengthX * viewZ),
                (float)((CenterY - screenY - 0.5) / FocalLengthY * viewZ),
                (float)viewZ);
        }
    }

    public sealed class Plane
    {
        public Vec Normal = new(0, 0, 1);
        public double D;
        public double Angle, BaseAngle, XOff, YOff = 1, XFormScale = 1, YFormScale = 1;
        public int Light = 128;
        public LightNode? Lights;
        public List<Row> Rows { get; } = new();
        public int PViewX, PViewY, StyleAlpha;
        public Vec PlaneNormal, Su, Sv, Sz;
        public float PlaneD;
        public double LightFloat;
        public bool Foggy, Shade, StyleAdditive, StyleMasked, IgnoredAdditive, IgnoredMasked;

        public double ZAt(double x, double y)
        {
            double inverse = -1.0 / Normal.Z;
            return (D + Normal.X * x + Normal.Y * y) * inverse;
        }
    }

    public sealed class Row
    {
        public int Y, X1, X2;
    }

    public sealed class Texture
    {
        public double PhysicalScale = 1;
        public int WidthBits = 6, HeightBits = 6;
    }

    public sealed class LightNode
    {
        public DynLight? Light;
        public LightNode? Next;
    }

    public sealed class DynLight
    {
        public double X, Y, Z;
        public float Radius = 1;
        public int Red, Green, Blue;
        public bool Active = true, Attenuated;
    }

    public sealed class DrawerLight
    {
        public float X, Y, Z, Radius;
        public int Color;
    }

    public sealed class Span
    {
        public int Y, X1, X2, LightLevel, PViewX, PViewY;
        public Vec Su, Sv, Sz, Normal;
        public bool Shade, Foggy;
        public double LightFloat;
        public float ViewX, ViewY, ViewZ, StepX, StepY, StepZ;
        public List<DrawerLight> Lights { get; set; } = new();
    }
}
