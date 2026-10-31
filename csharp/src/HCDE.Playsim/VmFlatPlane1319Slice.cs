namespace HCDE.Playsim;

/// <summary>
/// Flat-plane steps, span lighting, and the colored plane from
/// <c>r_flatplane.cpp</c> (1-288). Chunk 1319.
/// </summary>
public sealed class VmFlatPlane1319
{
    public const int XFlip = 0x0001;
    public const int HasFadeTable = 0x00000008;

    public bool DynLights { get; set; }
    public bool RangeCheck { get; set; }
    public double LodBias { get; set; }
    public int ViewWidth { get; set; } = 320;
    public int ViewHeight { get; set; } = 200;
    public List<Span> Spans { get; } = new();
    public List<ColoredSpan> Colored { get; } = new();
    public bool Skipped { get; private set; }

    public void Render(Plane plane, View view, double xScale, double yScale, int alpha, bool additive, bool masked, Colormap colormap, Texture tex)
    {
        Spans.Clear();
        if (alpha <= 0)
        {
            Skipped = true;
            return;
        }
        Skipped = false;
        double planeAng = plane.Angle + plane.BaseAngle;
        if (planeAng != 0)
        {
            double cosine = Math.Cos(planeAng);
            double sine = Math.Sin(planeAng);
            plane.ViewX = plane.XOff + view.X * cosine - view.Y * sine;
            plane.ViewY = plane.YOff + plane.BaseYOff - view.X * sine - view.Y * cosine;
        }
        else
        {
            plane.ViewX = plane.XOff + view.X;
            plane.ViewY = plane.YOff - view.Y;
        }
        plane.ViewX *= xScale;
        plane.ViewY *= yScale;
        planeAng += (view.YawDegrees - 90.0) * Math.PI / 180.0;
        plane.XStep = Math.Cos(planeAng) / view.FocalLengthX;
        plane.YStep = -Math.Sin(planeAng) / view.FocalLengthX;
        if ((view.MirrorFlags & XFlip) != 0)
        {
            plane.XStep = -plane.XStep;
            plane.YStep = -plane.YStep;
        }
        planeAng += Math.PI / 2.0;
        double cos2 = Math.Cos(planeAng);
        double sin2 = -Math.Sin(planeAng);
        double rightX = plane.Right - view.CenterX + 0.5;
        plane.RightXFrac = xScale * (cos2 + rightX * plane.XStep);
        plane.RightYFrac = yScale * (sin2 + rightX * plane.YStep);
        double leftX = plane.Left - view.CenterX + 0.5;
        plane.LeftXFrac = xScale * (cos2 + leftX * plane.XStep);
        plane.LeftYFrac = yScale * (sin2 + leftX * plane.YStep);
        plane.BaseXFrac = plane.LeftXFrac;
        plane.BaseYFrac = plane.LeftYFrac;
        if (plane.Left != plane.Right)
        {
            double width = plane.Right - plane.Left;
            plane.XStepScale = (plane.RightXFrac - plane.LeftXFrac) / width;
            plane.YStepScale = (plane.RightYFrac - plane.LeftYFrac) / width;
        }
        else
        {
            plane.XStepScale = 0;
            plane.YStepScale = 0;
        }
        plane.MinX = plane.Left;
        plane.Height = Math.Abs(plane.ZAt0 - view.Z);
        plane.Foggy = view.FadeTo != 0 || colormap.Fade != 0 || (view.LevelFlags & HasFadeTable) != 0;
        plane.LightLevel = plane.Light;
        plane.Shade = view.FixedLightLevel < 0 && !view.FixedColormap;
        plane.SolidColor = 3;
        plane.Masked = masked;
        plane.Additive = additive;
        plane.Alpha = alpha;
        plane.Map = colormap;
        plane.Tex = tex;
        foreach (Row row in plane.Rows)
            RenderLine(plane, view, row.Y, row.X1, row.X2);
    }

    public void RenderLine(Plane plane, View view, int y, int x1, int x2)
    {
        if (RangeCheck && (x2 < x1 || x1 < 0 || x2 >= ViewWidth || (uint)y >= (uint)ViewHeight))
            throw new InvalidOperationException($"R_MapPlane: {x1}, {x2} at {y}");
        double curX = plane.BaseXFrac + plane.XStepScale * (x1 - plane.MinX);
        double curY = plane.BaseYFrac + plane.YStepScale * (x1 - plane.MinX);
        double distance = view.PlaneDepth(y, plane.Height);
        double viewY = view.ScreenToViewY(y, 1.0);
        float zDepth = (float)(1.0 / Math.Abs(plane.Height / viewY));
        var span = new Span
        {
            Y = y,
            X1 = x1,
            X2 = x2,
            UStep = distance * plane.XStepScale / plane.Tex!.Width,
            U = (distance * curX + plane.ViewX) / plane.Tex.Width,
            VStep = distance * plane.YStepScale / plane.Tex.Height,
            V = (distance * curY + plane.ViewY) / plane.Tex.Height,
            ZDepth = zDepth
        };
        if (view.IsBgra)
        {
            double distance2 = view.PlaneDepth(y + 1, plane.Height);
            double xMag = Math.Abs(plane.YStepScale * (distance2 - distance) * view.FocalLengthX);
            double yMag = Math.Abs(plane.XStepScale * (distance2 - distance) * view.FocalLengthX);
            double magnitude = Math.Max(yMag, xMag);
            span.Lod = Math.Max(Math.Log2(magnitude) + LodBias, -1000.0);
        }
        if (plane.Shade)
            span.Light = (float)view.FlatPlaneVis(y, plane.Height, plane.Foggy);
        if (DynLights)
            span.Lights = LightsForRow(plane, view, y, x1);
        Spans.Add(span);
    }

    public void RenderColored(Plane plane, View view)
    {
        foreach (Row row in plane.Rows)
            Colored.Add(new ColoredSpan { Y = row.Y, X1 = row.X1, X2 = row.X2 });
    }

    private List<DrawerLight> LightsForRow(Plane plane, View view, int y, int x1)
    {
        int tx = x1;
        bool mirror = (view.MirrorFlags & XFlip) != 0;
        if (mirror)
            tx = ViewWidth - tx - 1;
        double denom = Math.Abs(y + 0.5 - view.CenterY) / view.InvZtoScale;
        float zspan = (float)(plane.Height / denom);
        float step = (float)(zspan / view.CenterX);
        if (mirror)
            step = -step;
        float viewPosX = (float)((tx + 0.5 - view.CenterX) / view.CenterX * zspan);
        float viewPosY = zspan;
        float viewPosZ = (float)((view.CenterY - y - 0.5) / view.InvZtoScale * zspan);
        float normalZ = y >= view.CenterY ? 1f : -1f;
        var lights = new List<DrawerLight>();
        for (LightNode? node = plane.Lights; node != null; node = node.Next)
        {
            if (node.Light == null || !node.Light.Active)
                continue;
            double lightX = node.Light.X - view.X;
            double lightY = node.Light.Y - view.Y;
            double lightZ = node.Light.Z - view.Z;
            float lx = (float)(lightX * view.Sin - lightY * view.Cos);
            float ly = (float)(lightX * view.TanCos + lightY * view.TanSin) - viewPosY;
            float lz = (float)lightZ - viewPosZ;
            float constant = ly * ly + lz * lz;
            float normalConstant = node.Light.Attenuated ? lz * normalZ : 0f;
            float radius = node.Light.Radius;
            if (radius * radius < constant || normalConstant < 0f)
                continue;
            lights.Add(new DrawerLight
            {
                X = lx,
                Y = constant,
                Z = normalConstant,
                Radius = 256f / radius,
                Color = (node.Light.Red << 16) | (node.Light.Green << 8) | node.Light.Blue,
                StepX = step,
                ViewX = viewPosX,
                ViewY = viewPosY,
                ViewZ = viewPosZ,
                NormalZ = normalZ
            });
        }
        return lights;
    }

    public sealed class View
    {
        public double X, Y, Z;
        public double CenterX = 160, CenterY = 100;
        public double FocalLengthX = 160, FocalLengthY = 100, InvZtoScale = 160;
        public double YawDegrees = 90;
        public double Sin, Cos = 1, TanCos = 1, TanSin;
        public int MirrorFlags, FadeTo, LevelFlags, FixedLightLevel = -1;
        public bool FixedColormap, IsBgra, NoLightFade;
        public double FloorVisibility = 1;

        public double PlaneDepth(int screenY, double planeHeight)
        {
            if (screenY + 0.5 < CenterY)
                return FocalLengthY / (CenterY - screenY - 0.5) * planeHeight;
            return FocalLengthY / (screenY + 0.5 - CenterY) * planeHeight;
        }

        public double ScreenToViewY(int screenY, double viewZ)
        {
            return (CenterY - screenY - 0.5) / FocalLengthY * viewZ;
        }

        public double FlatPlaneVis(int screenY, double planeHeight, bool foggy)
        {
            double glob = NoLightFade && !foggy ? 0 : FloorVisibility;
            return glob / planeHeight * Math.Abs(CenterY - screenY);
        }
    }

    public sealed class Plane
    {
        public double Angle, BaseAngle, XOff, YOff, BaseYOff, ZAt0;
        public int Left, Right = 1, Light;
        public Texture? Tex;
        public LightNode? Lights;
        public List<Row> Rows { get; } = new();
        public double ViewX, ViewY, XStep, YStep, XStepScale, YStepScale;
        public double LeftXFrac, LeftYFrac, RightXFrac, RightYFrac, BaseXFrac, BaseYFrac;
        public double Height;
        public int MinX, LightLevel, SolidColor, Alpha;
        public bool Foggy, Shade, Masked, Additive;
        public Colormap? Map;
    }

    public sealed class Row
    {
        public int Y, X1, X2;
    }

    public sealed class Texture
    {
        public double Width = 64, Height = 64;
    }

    public sealed class Colormap
    {
        public int Fade;
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
        public bool Active = true, Attenuated = true;
    }

    public sealed class Span
    {
        public int Y, X1, X2;
        public double U, V, UStep, VStep, Lod;
        public float ZDepth, Light;
        public List<DrawerLight> Lights { get; set; } = new();
    }

    public sealed class DrawerLight
    {
        public float X, Y, Z, Radius, ViewX, ViewY, ViewZ, StepX, NormalZ;
        public int Color;
    }

    public sealed class ColoredSpan
    {
        public int Y, X1, X2;
    }
}
