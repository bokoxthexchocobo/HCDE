namespace HCDE.Playsim;

/// <summary>
/// Span drawer setup and the four span draws from r_spandrawer.cpp 1-150.
/// Chunk 1407. The file ends at line 150.
/// Blend tables, the camera light, and the shade lookup stay callees.
/// </summary>
public static class VmSpanDrawer1407Slice
{
    public const int BandStart = 1;
    public const int BandEnd = 150;
    public const int Opaque = 65536;
    public const int FracUnit = 65536;
    public const int BlendShift = 10;
    public const string FullNormalLight = "FullNormalLight";
    public const string PlainSpan = "DrawSpan";
    public const string MaskedTranslucent = "DrawSpanMaskedTranslucent";
    public const string MaskedAdd = "DrawSpanMaskedAddClamp";
    public const string MaskedSpan = "DrawSpanMasked";
    public const string TranslucentSpan = "DrawSpanTranslucent";
    public const string AddSpan = "DrawSpanAddClamp";

    public enum BlendTable
    {
        Col2Rgb8,
        LessPrecision,
    }

    public readonly record struct Blend(BlendTable Table, int Index);

    public readonly record struct Camera(int FixedLightLevel, int FixedLightShade, string? FixedColormap);

    public readonly record struct Texture(
        int Width,
        int Height,
        int WidthBits,
        int HeightBits,
        bool Mipmapped,
        Func<string> PixelsBgra,
        Func<string, string> Pixels);

    public readonly record struct Tilted(
        string? Viewport,
        int Y,
        int X1,
        int X2,
        string PlaneSz,
        string PlaneSu,
        string PlaneSv,
        bool PlaneShade,
        int Shade,
        float PlaneLight,
        int ViewX,
        int ViewY,
        string? BaseColormap);

    public sealed class Args
    {
        public Args()
        {
            SpanFunc = PlainSpan;
        }

        public string SpanFunc { get; private set; }
        public int TexWidth { get; private set; }
        public int TexHeight { get; private set; }
        public int XBits { get; private set; }
        public int YBits { get; private set; }
        public string? Source { get; private set; }
        public bool Mipmapped { get; private set; }
        public Blend? SrcBlend { get; private set; }
        public Blend? DestBlend { get; private set; }
        public int? SrcAlpha { get; private set; }
        public int? DestAlpha { get; private set; }
        public string? Colormap { get; private set; }
        public float Light { get; private set; }
        public int Shade { get; private set; }
        public int DestY { get; private set; }
        public int DestX1 { get; private set; }
        public int DestX2 { get; private set; }
        public string? Viewport { get; private set; }

        public void SetTexture(bool bgra, Texture tex, Func<string> defaultStyle)
        {
            if (tex.PixelsBgra == null || tex.Pixels == null)
                throw new ArgumentNullException(nameof(tex));
            if (defaultStyle == null)
                throw new ArgumentNullException(nameof(defaultStyle));
            TexWidth = tex.Width;
            TexHeight = tex.Height;
            XBits = tex.WidthBits;
            YBits = tex.HeightBits;
            if ((1 << XBits) > TexWidth)
                XBits--;
            if ((1 << YBits) > TexHeight)
                YBits--;
            if (bgra)
                Source = tex.PixelsBgra();
            else
                Source = tex.Pixels(defaultStyle());
            Mipmapped = tex.Mipmapped && TexWidth > 1 && TexHeight > 1;
        }

        public void SetStyle(bool masked, bool additive, int alpha, string? baseColormap, bool ignoreSectorColor, Camera camera)
        {
            if (masked)
            {
                if (alpha < Opaque || additive)
                {
                    if (!additive)
                        AssignBlend(MaskedTranslucent, BlendTable.Col2Rgb8, alpha, Opaque - alpha);
                    else
                        AssignBlend(MaskedAdd, BlendTable.LessPrecision, alpha, FracUnit);
                }
                else
                    SpanFunc = MaskedSpan;
            }
            else if (alpha < Opaque || additive)
            {
                if (!additive)
                    AssignBlend(TranslucentSpan, BlendTable.Col2Rgb8, alpha, Opaque - alpha);
                else
                    AssignBlend(AddSpan, BlendTable.LessPrecision, alpha, FracUnit);
            }
            else
                SpanFunc = PlainSpan;

            if (camera.FixedLightLevel >= 0)
            {
                Colormap = ignoreSectorColor ? FullNormalLight : baseColormap;
                ApplyLight(0, camera.FixedLightShade);
            }
            else if (camera.FixedColormap != null)
            {
                Colormap = camera.FixedColormap;
                ApplyLight(0, 0);
            }
            else
                Colormap = baseColormap;
        }

        public void DrawSpan(Action<string, string?> draw)
        {
            if (draw == null)
                throw new ArgumentNullException(nameof(draw));
            draw(SpanFunc, Viewport);
        }

        public void DrawTiltedSpan(
            string? viewport,
            int y,
            int x1,
            int x2,
            string planeSz,
            string planeSu,
            string planeSv,
            bool planeShade,
            int lightLevel,
            bool foggy,
            float planeLight,
            int viewX,
            int viewY,
            string? baseColormap,
            Func<int, bool, string?, int> shade,
            Action<Tilted> draw)
        {
            if (shade == null)
                throw new ArgumentNullException(nameof(shade));
            if (draw == null)
                throw new ArgumentNullException(nameof(draw));
            SetDest(viewport, y, x1, x2);
            int computed = shade(lightLevel, foggy, viewport);
            draw(new Tilted(Viewport, DestY, DestX1, DestX2, planeSz, planeSu, planeSv, planeShade, computed, planeLight, viewX, viewY, baseColormap));
        }

        public void DrawFogBoundaryLine(string? viewport, int y, int x1, int x2, Action<string?, int, int, int> draw)
        {
            if (draw == null)
                throw new ArgumentNullException(nameof(draw));
            SetDest(viewport, y, x1, x2);
            draw(Viewport, DestY, DestX1, DestX2);
        }

        public void DrawColoredSpan(string? viewport, int y, int x1, int x2, Action<string?, int, int, int> draw)
        {
            if (draw == null)
                throw new ArgumentNullException(nameof(draw));
            SetDest(viewport, y, x1, x2);
            draw(Viewport, DestY, DestX1, DestX2);
        }

        private void AssignBlend(string spanFunc, BlendTable table, int srcAlpha, int destAlpha)
        {
            SpanFunc = spanFunc;
            SrcBlend = new Blend(table, srcAlpha >> BlendShift);
            DestBlend = new Blend(table, destAlpha >> BlendShift);
            SrcAlpha = srcAlpha;
            DestAlpha = destAlpha;
        }

        private void ApplyLight(float light, int shade)
        {
            Light = light;
            Shade = shade;
        }

        private void SetDest(string? viewport, int y, int x1, int x2)
        {
            Viewport = viewport;
            DestY = y;
            DestX1 = x1;
            DestX2 = x2;
        }
    }
}
