namespace HCDE.Playsim;

/// <summary>
/// Colormap lumps and the invulnerability override from <c>colormaps.cpp</c> (<b>1–224</b>).
/// Chunk <b>1190</b>. The palette, lumps, and special-colormap builder are host data.
/// <c>allowCustomColormap</c> is unused by this function.
/// </summary>
public static class VmColormaps1190Slice
{
    public const int CvarArchive = 1;
    public const int CvarNoInitCall = 1 << 9;
    public const int InvulFlags = CvarArchive | CvarNoInitCall;
    public const int InverseColormap = 0;
    public const int RealInverseColormap = 5;
    public const uint WaterMap = (128u << 24) | (0x4fu << 8) | 0xa5u;

    public static bool CustomizeInvulMap;
    public static uint Color1 = 0x00001a;
    public static uint Color2 = 0xa6a67a;

    public static readonly List<FakeCmap> FakeCmaps = new();
    public static readonly List<SpecialMap> SpecialColormaps = new();
    public static Rgb[] BaseColors = new Rgb[256];
    public static List<Lump> Lumps = new();
    public static Action? InitSpecialColormaps;
    public static Action<int, float, float, float, float, float, float>? UpdateSpecialColormap;

    public struct Rgb
    {
        public int R;
        public int G;
        public int B;
    }

    public sealed class Lump
    {
        public string Name = "";
        public bool Colormap;
        public byte[] Data = System.Array.Empty<byte>();
    }

    public sealed class FakeCmap
    {
        public string Name = "";
        public uint Blend;
        public int Lump;
    }

    public sealed class SpecialMap
    {
        public string Id = "";
        public float R1;
        public float G1;
        public float B1;
        public float R2;
        public float G2;
        public float B2;

        public SpecialMap Clone()
        {
            return new SpecialMap
            {
                Id = Id,
                R1 = R1,
                G1 = G1,
                B1 = B1,
                R2 = R2,
                G2 = G2,
                B2 = B2
            };
        }
    }

    public static void Clear()
    {
        CustomizeInvulMap = false;
        Color1 = 0x00001a;
        Color2 = 0xa6a67a;
        FakeCmaps.Clear();
        SpecialColormaps.Clear();
        BaseColors = new Rgb[256];
        Lumps = new();
        InitSpecialColormaps = null;
        UpdateSpecialColormap = null;
    }

    public static void SetCustomizeInvulMap(bool value)
    {
        CustomizeInvulMap = value;
        UpdateInvulnerabilityColormap();
    }

    public static void SetCustomInvulColor1(uint value)
    {
        Color1 = value;
        if (CustomizeInvulMap)
            UpdateInvulnerabilityColormap();
    }

    public static void SetCustomInvulColor2(uint value)
    {
        Color2 = value;
        if (CustomizeInvulMap)
            UpdateInvulnerabilityColormap();
    }

    public static void Deinit()
    {
        SpecialColormaps.Clear();
        FakeCmaps.Clear();
    }

    public static void Init(bool allowCustomColormap)
    {
        Deinit();

        FakeCmaps.Add(new FakeCmap());

        for (int i = 0; i < Lumps.Count; i++)
        {
            Lump lump = Lumps[i];
            if (!lump.Colormap)
                continue;
            if (CheckNumForName(lump.Name) != i)
                continue;

            FakeCmaps.Add(new FakeCmap
            {
                Name = Take8(lump.Name),
                Blend = 0,
                Lump = i
            });
        }

        int rr = 0;
        int gg = 0;
        int bb = 0;
        for (int x = 0; x < 256; x++)
        {
            Rgb color = x < BaseColors.Length ? BaseColors[x] : default;
            rr += color.R;
            gg += color.G;
            bb += color.B;
        }

        rr >>= 8;
        gg >>= 8;
        bb >>= 8;
        int paletteBrightness = (rr * 77 + gg * 143 + bb * 35) / 255;

        if (FakeCmaps.Count > 1)
        {
            for (int j = 1; j < FakeCmaps.Count; j++)
            {
                Lump source = Lumps[FakeCmaps[j].Lump];
                if (source.Data.Length < 256)
                    continue;

                int r = 0;
                int g = 0;
                int b = 0;
                for (int k = 0; k < 256; k++)
                {
                    int index = source.Data[k];
                    Rgb color = index < BaseColors.Length ? BaseColors[index] : default;
                    r += color.R;
                    g += color.G;
                    b += color.B;
                }

                r /= 256;
                g /= 256;
                b /= 256;
                int maxCol = paletteBrightness;
                if (r > maxCol)
                    maxCol = r;
                if (g > maxCol)
                    maxCol = g;
                if (b > maxCol)
                    maxCol = b;
                FakeCmaps[j].Blend = (255u << 24)
                    | (uint)(r * 255 / maxCol) << 16
                    | (uint)(g * 255 / maxCol) << 8
                    | (uint)(b * 255 / maxCol);
            }
        }

        InitSpecialColormaps?.Invoke();
        UpdateInvulnerabilityColormap();
    }

    public static uint ColormapNumForName(string name)
    {
        if (!Same8(name, "COLORMAP"))
        {
            for (int i = FakeCmaps.Count - 1; i > 0; i--)
            {
                if (Same8(name, FakeCmaps[i].Name))
                    return (uint)i;
            }

            if (Same8(name, "WATERMAP"))
                return WaterMap;
        }

        return 0;
    }

    public static uint BlendForColormap(uint map)
    {
        if (((map >> 24) & 0xff) != 0)
            return map;
        if (map < (uint)FakeCmaps.Count)
            return FakeCmaps[(int)map].Blend;
        return 0;
    }

    public static void UpdateInvulnerabilityColormap()
    {
        if (CustomizeInvulMap)
        {
            float r1 = ((Color1 & 0xff0000) >> 16) / 128f;
            float g1 = ((Color1 & 0x00ff00) >> 8) / 128f;
            float b1 = ((Color1 & 0x0000ff) >> 0) / 128f;
            float r2 = ((Color2 & 0xff0000) >> 16) / 128f;
            float g2 = ((Color2 & 0x00ff00) >> 8) / 128f;
            float b2 = ((Color2 & 0x0000ff) >> 0) / 128f;
            UpdateSpecialColormap?.Invoke(InverseColormap, r1, g1, b1, r2, g2, b2);
        }
        else
        {
            SpecialColormaps[InverseColormap] = SpecialColormaps[RealInverseColormap].Clone();
        }
    }

    static int CheckNumForName(string name)
    {
        for (int i = 0; i < Lumps.Count; i++)
        {
            if (!Lumps[i].Colormap)
                continue;
            if (Same8(name, Lumps[i].Name))
                return i;
        }

        return -1;
    }

    static string Take8(string name)
    {
        if (name.Length <= 8)
            return name;
        return name.Substring(0, 8);
    }

    static bool Same8(string a, string b)
    {
        for (int i = 0; i < 8; i++)
        {
            char ca = i < a.Length ? a[i] : '\0';
            char cb = i < b.Length ? b[i] : '\0';
            if (ca == '\0' || cb == '\0')
                return ca == '\0' && cb == '\0';
            if (char.ToUpperInvariant(ca) != char.ToUpperInvariant(cb))
                return false;
        }

        return true;
    }
}
