namespace HCDE.Playsim;

/// <summary>
/// Wall-sprite projection and the portal column clip from <c>r_wallsprite.cpp</c>
/// (1-196). Chunk 1274. The wall projection and the drawer are host inputs.
/// The sprite yaw is turned 90 degrees before the edges are placed. A bright
/// actor forces the fullbright render bit, and a foggy view still clears the
/// light passed to the colormap. The mirror assignment stays unused.
/// </summary>
public sealed class VmWallSprite1274
{
    public int Id;
    public double Yaw;
    public double PosX;
    public double PosY;
    public double PosZ;
    public double ScaleX = 1;
    public double ScaleY = 1;
    public int RenderFlags;
    public int Flags5;
    public bool PicPresent = true;
    public double ScaledWidth;
    public double ScaledLeft;
    public double ScaledTop;
    public double ScaledHeight;
    public bool ProjectReject;
    public int Sx1;
    public int Sx2;
    public int SectorId;
    public int Style = 1;
    public int StyleFlags;
    public int FillColor;
    public int Translation;
    public double Alpha = 1;
    public bool Foggy;
    public int LightLevel;
    public bool TexPresent = true;
    public bool FullbrightDisabled;
    public bool TextureFullbright;
    public bool Visible = true;
    public int X1;
    public int X2;
    public int VisFlags;
    public float Gzt;
    public float YScale;
    public string Map = "base";
    public bool Drew;
}

public sealed class VmWallSpritePass1274
{
    public const int StyleAdd = 7;
    public const int FullBright = 0x0010;
    public const int BrightActor = 0x08000000;
    public const int XFlip = 0x0001;
    public const int YFlip = 0x0002;
    public const int InvertSource = 16;
    public const int InvertOverlay = 32;
    public const int FadeToBlack = 64;
    public bool ThreadPresent = true;
    public double ViewX;
    public double ViewY;
    public double TanCos;
    public double TanSin;
    public int WindowLeft;
    public int WindowRight = 320;
    public bool FullbrightOverrides;
    public int ColormapColor;
    public int ColormapFade;
    public int ColormapDesaturate;
    public string BaseColormap = "base";
    public int PortalUniq;
    public bool StyleCalled;
    public readonly HashSet<int> PortalColumns = new();
    public readonly List<VmWallSprite1274> Sprites = new();
    public readonly List<string> Log = new();

    public void Project(VmWallSprite1274 sprite)
    {
        if (!sprite.PicPresent)
            throw new NullReferenceException();
        double degrees = sprite.Yaw + 90;
        double angcos = Math.Cos(degrees * Math.PI / 180.0);
        double angsin = Math.Sin(degrees * Math.PI / 180.0);
        double x1 = sprite.ScaledLeft * sprite.ScaleX;
        double x2 = sprite.ScaledWidth * sprite.ScaleX;
        if (!ThreadPresent)
            throw new NullReferenceException();
        double leftX = sprite.PosX - x1 * angcos - ViewX;
        double leftY = sprite.PosY - x1 * angsin - ViewY;
        double rightX = leftX + x2 * angcos;
        double rightY = leftY + x2 * angsin;
        if (sprite.ProjectReject)
            return;
        if (sprite.Sx1 >= WindowRight || sprite.Sx2 <= WindowLeft)
            return;

        double tz = (sprite.PosX - ViewX) * TanCos + (sprite.PosY - ViewY) * TanSin;
        double scaledBottom = sprite.ScaledTop - sprite.ScaledHeight;
        double gzt = sprite.PosZ + sprite.ScaleY * sprite.ScaledTop;
        double gzb = sprite.PosZ + sprite.ScaleY * scaledBottom;
        int clippedLeft = sprite.Sx1 < WindowLeft ? WindowLeft : sprite.Sx1;
        int clippedRight = sprite.Sx2 >= WindowRight ? WindowRight : sprite.Sx2;
        int flags = sprite.RenderFlags;
        if ((sprite.Flags5 & BrightActor) != 0)
            flags |= FullBright;

        string map = BaseColormap;
        if (sprite.Style == StyleAdd && ColormapFade != 0)
            map = "special " + ColormapColor.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + " 0 " + ColormapDesaturate.ToString(System.Globalization.CultureInfo.InvariantCulture);

        bool disables = sprite.TexPresent && sprite.FullbrightDisabled;
        bool forces = FullbrightOverrides && sprite.TexPresent && sprite.TextureFullbright;
        bool fullbright = !sprite.Foggy && (((sprite.Flags5 & BrightActor) != 0)
            || (!disables && ((flags & FullBright) != 0 || forces)));
        bool invert = (sprite.StyleFlags & InvertOverlay) != 0;
        if ((sprite.StyleFlags & InvertSource) != 0)
            invert = !invert;
        bool fade = (sprite.StyleFlags & FadeToBlack) != 0;
        float yscale = (float)sprite.ScaleY;
        float depth = (float)tz;
        float idepth = (float)(1 / tz);
        float alpha = (float)sprite.Alpha;
        float gztF = (float)gzt;
        float gzbF = (float)gzb;
        sprite.X1 = clippedLeft;
        sprite.X2 = clippedRight;
        sprite.Map = map;
        sprite.VisFlags = flags;
        sprite.Gzt = gztF;
        sprite.YScale = yscale;
        Sprites.Add(sprite);
        Log.Add(
            "vis " + sprite.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " edge " + Text(leftX) + " " + Text(leftY) + " " + Text(rightX) + " " + Text(rightY) +
            " tz " + Text(tz) +
            " z " + gztF.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " " + gzbF.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " x " + clippedLeft.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + clippedRight.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " depth " + depth.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " idepth " + idepth.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " yscale " + yscale.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " flags " + flags.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " style " + sprite.Style.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " map " + map +
            " bright " + (fullbright ? 1 : 0) +
            " invert " + (invert ? 1 : 0) +
            " fade " + (fade ? 1 : 0) +
            " alpha " + alpha.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " delta " + ((float)(sprite.PosX - ViewX)).ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " " + ((float)(sprite.PosY - ViewY)).ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " g " + ((float)sprite.PosX).ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " " + ((float)sprite.PosY).ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " " + ((float)sprite.PosZ).ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " light " + sprite.LightLevel.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " fog " + (sprite.Foggy ? 1 : 0) +
            " portal " + PortalUniq.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " sector " + sprite.SectorId.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " fill " + sprite.FillColor.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " trans " + sprite.Translation.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " side 0 null null");
    }

    public void Render(VmWallSprite1274 sprite, short[] floorClip, short[] ceilingClip)
    {
        int x1 = Math.Max(sprite.X1, sprite.Sx1);
        int x2 = Math.Min(sprite.X2, sprite.Sx2);
        if (x1 >= x2)
            return;
        StyleCalled = true;
        if (!sprite.Visible)
            return;
        var samples = new short[x2 - x1];
        for (int x = x1; x < x2; x++)
            samples[x - x1] = PortalColumns.Contains(x) ? ceilingClip[x] : floorClip[x];
        sprite.Drew = true;
        Log.Add(
            "draw " + sprite.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + x1.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + x2.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " " + Join(samples) +
            " ceil " + Join(ceilingClip, x1, x2) +
            " z " + sprite.Gzt.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " y " + sprite.YScale.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
            " flip " + (((sprite.VisFlags & XFlip) != 0) ? 1 : 0) +
            " " + (((sprite.VisFlags & YFlip) != 0) ? 1 : 0) +
            " map " + sprite.Map);
    }

    static string Join(short[] data) => Join(data, 0, data.Length);

    static string Join(short[] data, int start, int end)
    {
        var text = new System.Text.StringBuilder();
        for (int i = start; i < end; i++)
        {
            if (i != start)
                text.Append(',');
            text.Append(data[i].ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return text.ToString();
    }

    static string Text(double value) =>
        value.ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
}
