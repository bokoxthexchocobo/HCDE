using System.Globalization;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Flag stripes, the mix, and the banner frame from <c>launcherbanner.cpp</c>
/// (1-153). Chunk 1289. The header color, the theme mode, and the clock are host inputs.
/// </summary>
public sealed class VmLauncherBanner1289
{
    public float HeaderR = 1;
    public float HeaderG = 1;
    public float HeaderB = 1;
    public bool LightMode;
    public double Width = 100;
    public double LogoWidth = 10;
    public double LogoHeight = 40;
    public long NowMs;
    public int FlagIndex = -1;
    public string LogoPath = "";
    public string ListText = "";
    public List<uint> Solids = new();
    public List<uint> Mixed = new();
    public List<string> Frames = new();

    public double PreferredHeight()
    {
        return LogoHeight;
    }

    public void Build(string id, float mix)
    {
        Solids.Clear();
        Mixed.Clear();
        ListText = "";
        FlagIndex = -1;
        bool useColors = id.Length != 0;
        if (useColors)
        {
            bool white = true;
            float t = mix;
            if (t <= 0f)
            {
                useColors = false;
                white = false;
                t = -t;
            }
            if (t < 0f)
                t = 0f;
            if (t > 1f)
                t = 1f;
            FlagIndex = Find(id);
            if (FlagIndex >= 0)
            {
                var basis = white
                    ? VmColorSpace1211Slice.Rgb(0xFFFFFFu)
                    : VmColorSpace1211Slice.Rgb(HeaderR, HeaderG, HeaderB);
                foreach (uint rgb in Expand(Flags[FlagIndex].Stops))
                {
                    var color = VmColorSpace1211Slice.Rgb(rgb);
                    Solids.Add(color.Rgb24());
                    Mixed.Add(VmColorSpace1211Slice.Mix(basis, color, t).Rgb24());
                }
            }
        }
        LogoPath = useColors || LightMode ? "ui/banner-light.png" : "ui/banner-dark.png";
    }

    public void Layout()
    {
        Frames.Clear();
        double width = Width;
        double height = LogoHeight;
        double logo = LogoWidth;
        int count = Solids.Count;
        if (count > 0)
        {
            double band = height / count;
            double thick = Math.Ceiling(band);
            for (int i = 0; i < count; i++)
            {
                double y = Math.Floor(band * i);
                Frame("band", 0, y, width, thick);
                Frame("mix", (width - logo) / 2.0, y, logo, thick);
            }
        }
        Frame("logo", 0, 0, width, height);
    }

    int Find(string id)
    {
        if (Same(id, "random") || Same(id, "list"))
        {
            int index = (int)((ulong)NowMs % (ulong)Flags.Length);
            if (Same(id, "list"))
                ListText = Describe(index);
            return index;
        }
        for (int i = 0; i < Flags.Length; i++)
        {
            if (Same(Flags[i].Name, id))
                return i;
        }
        return -1;
    }

    static string Describe(int index)
    {
        var text = new StringBuilder("Available flag colors: random");
        foreach (Flag flag in Flags)
            text.Append(", ").Append(flag.Name);
        text.Append("\nSelecting: ").Append(Flags[index].Name);
        text.Append("\nOpen an issue on our github if your flag is missing\n");
        return text.ToString();
    }

    static List<uint> Expand(uint[] stops)
    {
        var colors = new List<uint>();
        for (int i = 0; stops[i] != 0; i += 2)
        {
            for (int j = (int)stops[i]; j >= 0; j--)
                colors.Add(stops[i + 1]);
        }
        return colors;
    }

    static bool Same(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            if (Fold(left[i]) != Fold(right[i]))
                return false;
        }
        return true;
    }

    static char Fold(char c)
    {
        return c is >= 'A' and <= 'Z' ? (char)(c + 32) : c;
    }

    void Frame(string name, double x, double y, double width, double height)
    {
        Frames.Add(string.Join(' ', name, Num(x), Num(y), Num(width), Num(height)));
    }

    static string Num(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    readonly record struct Flag(string Name, uint[] Stops);

    static readonly Flag[] Flags =
    {
        new("agender", new uint[] { 1, 0x000000, 1, 0xBCC4C7, 1, 0xFFFFFF, 1, 0xB7F684, 1, 0xFFFFFF, 1, 0xBCC4C7, 1, 0x000000, 0 }),
        new("aroace", new uint[] { 1, 0xE28C00, 1, 0xECCD00, 1, 0xFFFFFF, 1, 0x62AEDC, 1, 0x203856, 0 }),
        new("aroflux", new uint[] { 1, 0xE7516A, 1, 0xD86D65, 1, 0xB7A55D, 1, 0xA3C95A, 1, 0x92E454, 0 }),
        new("aromantic", new uint[] { 1, 0x3DA542, 1, 0xA7D379, 1, 0xFFFFFF, 1, 0xA9A9A9, 1, 0x000000, 0 }),
        new("asexual", new uint[] { 1, 0x000000, 1, 0xA3A3A3, 1, 0xFFFFFF, 1, 0x800080, 0 }),
        new("bear", new uint[] { 1, 0x613704, 1, 0xD46300, 1, 0xFDDC62, 1, 0xFDE5B7, 1, 0xFFFFFF, 1, 0x545454, 1, 0x000000, 0 }),
        new("bisexual", new uint[] { 2, 0xD60270, 1, 0x9B4F96, 2, 0x0038A8, 0 }),
        new("demisexual", new uint[] { 5, 0xFFFFFF, 1, 0x6E0070, 1, 0x000000, 1, 0x6E0070, 5, 0xD2D2D2, 0 }),
        new("gay", new uint[] { 1, 0x078D70, 1, 0x26CEAA, 1, 0x98E8C1, 1, 0xFFFFFF, 1, 0x7BADE2, 1, 0x5049CC, 1, 0x3D1A78, 0 }),
        new("genderfluid", new uint[] { 1, 0xFF76A4, 1, 0xFFFFFF, 1, 0xC011D7, 1, 0x000000, 1, 0x2F3CBE, 0 }),
        new("genderqueer", new uint[] { 1, 0xB57EDC, 1, 0xFFFFFF, 1, 0x4A8123, 0 }),
        new("intersex", new uint[] { 5, 0xFFD800, 1, 0x7902AA, 5, 0xFFD800, 0 }),
        new("lesbian", new uint[] { 1, 0xD52D00, 1, 0xEF7627, 1, 0xFF9A56, 1, 0xFFFFFF, 1, 0xD162A4, 1, 0xB55690, 1, 0xA30262, 0 }),
        new("nonbinary", new uint[] { 1, 0xFCF434, 1, 0xFFFFFF, 1, 0x9C59D1, 1, 0x2C2C2C, 0 }),
        new("omnisexual", new uint[] { 1, 0xFE9ACE, 1, 0xFF53BF, 1, 0x200044, 1, 0x6760FE, 1, 0x8EA6FF, 0 }),
        new("pansexual", new uint[] { 1, 0xFF218C, 1, 0xFFD800, 1, 0x21B1FF, 0 }),
        new("philadelphia", new uint[] { 1, 0x000000, 1, 0x784F17, 1, 0xD12229, 1, 0xF68A1E, 1, 0xFDE01A, 1, 0x007940, 1, 0x24408E, 1, 0x732982, 0 }),
        new("polysexual", new uint[] { 1, 0xF714BA, 1, 0x01D66A, 1, 0x1594F6, 0 }),
        new("queer", new uint[] { 1, 0x000000, 1, 0x99D9EA, 1, 0x00A2E8, 1, 0xB5E61D, 1, 0xFFFFFF, 1, 0xFFC90E, 1, 0xFD6666, 1, 0xFFAEC9, 1, 0x000000, 0 }),
        new("rainbow", new uint[] { 1, 0xE40303, 1, 0xFF8C00, 1, 0xFFED00, 1, 0x008026, 1, 0x004CFF, 1, 0x732982, 0 }),
        new("transgender", new uint[] { 1, 0x5BCEFA, 1, 0xF5A9B8, 1, 0xFFFFFF, 1, 0xF5A9B8, 1, 0x5BCEFA, 0 }),
    };
}
