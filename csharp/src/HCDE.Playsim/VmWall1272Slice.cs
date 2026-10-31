namespace HCDE.Playsim;

/// <summary>
/// Striped wall bands and the normal wall drawer setup from <c>r_walldraw.cpp</c>
/// (1-226). Chunk 1272. A light plane above the wall is skipped, and every plane
/// still installs its colormap after that decision. The band edge is
/// max(min(screen, top), bottom), which keeps the bottom when the range is
/// inverted. The two clip rows share one allocation, stored minus x1.
/// </summary>
public sealed class VmWallPlane1272
{
    public int Cull;
    public string Name = "";
    public short[] Screen = [];
}

public sealed class VmWall1272
{
    public const int Opaque = 65536;
    public const int OutsideAbove = 1;
    public bool ThreadPresent = true;
    public bool PicPresent = true;
    public bool PicMasked;
    public int PicWidth = 16;
    public int PicHeight = 16;
    public int HeightBits = 4;
    public bool PicMipmapped;
    public bool Bgra;
    public bool MipmapCvar;
    public bool Mask;
    public bool Additive;
    public int Alpha;
    public int FixedLightLevel = -1;
    public bool HasFixedColormap;
    public string FixedColormap = "fixed";
    public bool IgnoreSectorColor;
    public int Shade;
    public int LightLevel;
    public bool Foggy;
    public string BaseColormap = "base";
    public bool HasCurline = true;
    public bool HasSidedef = true;
    public int SideIndex;
    public int LightListCount;
    public int LightListId;
    public int CenterX;
    public int CenterY;
    public readonly List<VmWallPlane1272> Planes = new();
    public readonly List<string> Log = new();
    string _colormap = "base";
    int? _lights;

    public void Render(int x1, int x2, short[] top, short[] bottom)
    {
        if (!PicPresent)
            return;
        if (!ThreadPresent)
            throw new NullReferenceException();
        _colormap = BaseColormap;
        _lights = GetLights();
        if (HasFixedColormap || FixedLightLevel >= 0 || Planes.Count == 0)
            DrawNormal(x1, x2, top, bottom);
        else
            DrawStriped(x1, x2, top, bottom);
    }

    public int? GetLights()
    {
        if (FixedLightLevel >= 0 || HasFixedColormap)
            return null;
        if (HasCurline && HasSidedef && LightListCount > SideIndex)
            return LightListId;
        return null;
    }

    void DrawStriped(int x1, int x2, short[] top, short[] bottom)
    {
        var most1 = new short[256];
        var most2 = new short[256];
        short[] up = top;
        short[] down = most1;
        bool first = true;
        foreach (var plane in Planes)
        {
            if (plane.Cull != OutsideAbove)
            {
                for (int x = x1; x < x2; x++)
                {
                    int sample = Math.Min((int)plane.Screen[x], (int)up[x]);
                    down[x] = (short)Math.Max(sample, (int)bottom[x]);
                }

                DrawNormal(x1, x2, up, down);
                up = down;
                down = first ? most2 : most1;
                first = !first;
            }

            _colormap = plane.Name;
        }

        DrawNormal(x1, x2, up, bottom);
    }

    void DrawNormal(int x1, int x2, short[] up, short[] down)
    {
        bool solid = !PicMasked && Mask && Alpha >= Opaque && !Additive;
        int styleMask = solid ? 1 : (Mask ? 1 : 0);
        int styleAdd = solid ? 0 : (Additive ? 1 : 0);
        int styleAlpha = solid ? Opaque : Alpha;
        string mode;
        string map;
        int shade;
        int vary = 0;
        if (FixedLightLevel >= 0)
        {
            mode = "fixed";
            map = IgnoreSectorColor ? "full" : _colormap;
            shade = Shade;
        }
        else if (HasFixedColormap)
        {
            mode = "map";
            map = FixedColormap;
            shade = 0;
        }
        else
        {
            mode = "vary";
            map = _colormap;
            shade = LightLevel;
            vary = Foggy ? 1 : 0;
        }

        int count = x2 - x1;
        var data = new short[count << 1];
        for (int i = 0; i < count; i++)
        {
            data[i] = up[x1 + i];
            data[count + i] = down[x1 + i];
        }

        int frac;
        int mip;
        string pixels;
        if (Bgra)
        {
            pixels = "bgra";
            frac = 32;
            mip = MipmapCvar && PicMipmapped ? 1 : 0;
        }
        else
        {
            pixels = "pal";
            frac = 32 - HeightBits;
            if (frac == 32)
                frac = 0;
            mip = 0;
        }

        Log.Add(
            "draw " + map + " " + mode + " " + shade + " " + vary + " " +
            styleMask + " " + styleAdd + " " + styleAlpha + " " +
            pixels + " " + frac + " " + mip + " " +
            (_lights.HasValue ? _lights.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "null") +
            " " + CenterX + " " + CenterY + " " + PicWidth + " " + PicHeight + " " +
            Join(data, 0, count) + " " + Join(data, count, count));
    }

    static string Join(short[] data, int start, int count)
    {
        var text = new System.Text.StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if (i != 0)
                text.Append(',');
            text.Append(data[start + i].ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return text.ToString();
    }
}
