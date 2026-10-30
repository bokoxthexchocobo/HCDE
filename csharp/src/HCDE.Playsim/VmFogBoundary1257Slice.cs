using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Fog-boundary span walk from <c>r_fogboundary.cpp</c> (<b>1–157</b>).
/// Chunk <b>1257</b>. The shade comes from the light host. The colormap index is the same
/// clamp the renderer uses at the call: shade minus the visibility fixed point, then a
/// 16-bit shift, held inside 0..31. Column 0 of that table is the identity map and is not drawn.
/// </summary>
public sealed class VmFogBoundary1257
{
    public const int NumColorMaps = 32;
    public const int ColorMapShift = 8;
    public const int FracBits = 16;
    public const double MaxLightVis = 24.0;

    public readonly List<string> Log = new();
    public readonly short[] SpanEnd = new short[64];
    public Func<int, bool, int>? ToShade;
    public int WallShade;
    public float Light;
    public int Map;
    public int MapOffset;

    public static int PalLookup(float vis, int shade)
    {
        double limited = Math.Min(MaxLightVis, vis);
        int fixedVis = RoundHalfEven(limited * (1 << FracBits));
        int index = (shade - fixedVis) >> FracBits;
        if (index < 0)
            return 0;
        if (index > NumColorMaps - 1)
            return NumColorMaps - 1;
        return index;
    }

    public void Render(
        int x1,
        int x2,
        short[] uclip,
        short[] dclip,
        int lightLevel,
        bool foggy,
        float lightAtRight,
        float lightStep)
    {
        int wallshade = ToShade != null ? ToShade(lightLevel, foggy) : 0;
        WallShade = wallshade;
        Log.Add($"shade {wallshade}");
        int x = x2 - 1;
        int t2 = uclip[x];
        int b2 = dclip[x];
        float light = lightAtRight;
        int rcolormap = PalLookup(light, wallshade);
        if (b2 > t2)
            Fill(t2, b2 - t2, x);
        Log.Add("base");
        Log.Add($"light3 {Fmt(light)} {lightLevel} {(foggy ? 1 : 0)}");
        int fake = PalLookup(light, wallshade) << ColorMapShift;
        Remember(light, rcolormap, fake);

        for (x--; x >= x1; x--)
        {
            int t1 = uclip[x];
            int b1 = dclip[x];
            int xr = x + 1;
            light -= lightStep;
            int lcolormap = PalLookup(light, wallshade);
            if (lcolormap != rcolormap)
            {
                if (t2 < b2 && rcolormap != 0)
                    Section(t2, b2, xr);
                if (t1 < t2)
                    t2 = t1;
                if (b1 > b2)
                    b2 = b1;
                if (t2 < b2)
                    Fill(t2, b2 - t2, x);
                rcolormap = lcolormap;
                Log.Add($"light2 {Fmt(light)} {wallshade}");
                fake = PalLookup(light, wallshade) << ColorMapShift;
                Remember(light, rcolormap, fake);
            }
            else if (fake != 0)
            {
                int stop = Math.Min(t1, b2);
                while (t2 < stop)
                {
                    int y = t2++;
                    Line(y, xr, SpanEnd[y]);
                }
                stop = Math.Max(b1, t2);
                while (b2 > stop)
                {
                    int y = --b2;
                    Line(y, xr, SpanEnd[y]);
                }
                MarkOpen(ref t1, ref b1, t2, b2, x);
            }
            else
            {
                t2 = Math.Max(t2, Math.Min(t1, b2));
                b2 = Math.Min(b2, Math.Max(b1, t2));
                Log.Add($"ident {x}");
                MarkOpen(ref t1, ref b1, t2, b2, x);
            }

            t2 = uclip[x];
            b2 = dclip[x];
        }

        if (t2 < b2 && rcolormap != 0)
            Section(t2, b2, x1);
        Light = light;
    }

    void Section(int y, int y2, int x1)
    {
        Log.Add($"section {y} {y2} {x1}");
        for (; y < y2; y++)
            Line(y, x1, SpanEnd[y]);
    }

    void MarkOpen(ref int t1, ref int b1, int t2, int b2, int x)
    {
        int stop = Math.Min(t2, b1);
        while (t1 < stop)
        {
            SpanEnd[t1] = (short)x;
            Log.Add($"mark {t1} {x}");
            t1++;
        }
        stop = Math.Max(b2, t2);
        while (b1 > stop)
        {
            b1--;
            SpanEnd[b1] = (short)x;
            Log.Add($"mark {b1} {x}");
        }
    }

    void Line(int y, int x1, int x2) => Log.Add($"fog {y} {x1} {x2}");

    void Fill(int start, int count, int value)
    {
        for (int i = 0; i < count; i++)
            SpanEnd[start + i] = (short)value;
        Log.Add($"fill {start} {count} {value}");
    }

    void Remember(float light, int map, int offset)
    {
        Light = light;
        Map = map;
        MapOffset = offset;
        Log.Add($"map {map} off {offset}");
    }

    static int RoundHalfEven(double val) => (int)Math.Round(val, MidpointRounding.ToEven);

    static string Fmt(float value) => value.ToString("F6", CultureInfo.InvariantCulture);
}
