namespace HCDE.Playsim;

/// <summary>
/// Decal walk, relative wall Z, and the clipped sprite draw from <c>r_decal.cpp</c>
/// (1-255). Chunk 1273. Wall projection, the texture lookup, and the drawer are
/// host inputs. The commented full-height sprite clip stays out, so a draw-segment
/// pass keeps the wall rows. A two-sided full clip draws the lower wall once more
/// after swapping onto the floor clip.
/// </summary>
public sealed class VmDecal1273
{
    public int Id;
    public int RenderFlags;
    public bool PicValid = true;
    public bool PicPresent = true;
    public double Z;
    public double ScaleX = 1;
    public double ScaleY = 1;
    public double Alpha = 1;
    public int AlphaColor;
    public int Translation;
    public int Style;
    public double PicWidth;
    public double LeftOffset;
    public double TopOffset;
    public double X;
    public double Y;
    public bool ProjectReject;
    public int Sx1;
    public int Sx2;
    public bool Visible = true;
    public double SectorCeiling;
    public double SectorFloor;
    public VmDecal1273? WallNext;
}

public sealed class VmDecalLine1273
{
    public bool HasBack;
    public double FrontCeiling;
    public double FrontFloor;
    public double BackCeiling;
    public double BackFloor;
    public int LineFlags;
    public double V1X;
    public double V1Y;
    public double V2X;
    public double V2Y;
}

public sealed class VmDecalPass1273
{
    public const int StyleAdd = 7;
    public const int Invisible = 0x8000;
    public const int RelMask = 0x0300;
    public const int RelUpper = 0x0100;
    public const int RelLower = 0x0200;
    public const int RelMid = 0x0300;
    public const int ClipMask = 0x0c00;
    public const int ClipFull = 0x0000;
    public const int ClipUpper = 0x0400;
    public const int ClipMid = 0x0800;
    public const int ClipLower = 0x0c00;
    public const int FullBright = 0x0010;
    public const int XFlip = 0x0001;
    public const int YFlip = 0x0002;
    public const int DontPegTop = 0x0008;
    public const int DontPegBottom = 0x0010;
    public const int TierTop = 0;
    public const int TierMid = 1;
    public const int TierBottom = 2;

    public bool ThreadPresent = true;
    public bool ViewActive = true;
    public double ViewX;
    public double ViewY;
    public int ColormapColor;
    public int ColormapFade;
    public int ColormapDesaturate;
    public string BaseColormap = "base";
    public string WallTop = "walltop";
    public string WallBottom = "wallbottom";
    public string CeilingClip = "ceiling";
    public string FloorClip = "floor";
    public string SprTop = "sprtop";
    public string SprBottom = "sprbottom";
    public readonly List<string> Log = new();

    public void RenderDecals(
        VmDecal1273? head,
        VmDecalLine1273? line,
        int clipX1,
        int clipX2,
        bool drawsegPass)
    {
        if (line == null)
            throw new NullReferenceException();
        for (VmDecal1273? decal = head; decal != null; decal = decal.WallNext)
            Render(decal, line, clipX1, clipX2, drawsegPass);
    }

    void Render(VmDecal1273 decal, VmDecalLine1273 line, int clipX1, int clipX2, bool drawsegPass)
    {
        if ((decal.RenderFlags & Invisible) != 0 || !ViewActive || !decal.PicValid)
            return;

        double z = RelativeZ(decal, line);
        if (!decal.PicPresent)
            return;

        double edgeRight = decal.PicWidth;
        double edgeLeft = decal.LeftOffset;
        edgeRight = (edgeRight - edgeLeft) * decal.ScaleX;
        edgeLeft *= decal.ScaleX;
        double vx = line.V2X - line.V1X;
        double vy = line.V2Y - line.V1Y;
        double len = Math.Sqrt(vx * vx + vy * vy);
        if (len != 0)
            len = 1 / len;
        double ux = vx * len;
        double uy = vy * len;
        double leftX = decal.X - edgeLeft * ux - ViewX;
        double leftY = decal.Y - edgeLeft * uy - ViewY;
        double rightX = decal.X + edgeRight * ux - ViewX;
        double rightY = decal.Y + edgeRight * uy - ViewY;

        if (!ThreadPresent)
            throw new NullReferenceException();
        if (decal.ProjectReject)
            return;

        int x1 = decal.Sx1;
        int x2 = decal.Sx2;
        if (x1 >= clipX2 || x2 <= clipX1)
            return;

        int tier = TierMid;
        string ceiling;
        string floor;
        int needrepeat = 0;
        if (drawsegPass)
        {
            int clipMode = decal.RenderFlags & ClipMask;
            if (clipMode != ClipMid && clipMode != ClipFull)
                return;
            ceiling = WallTop;
            floor = WallBottom;
        }
        else
        {
            switch (decal.RenderFlags & ClipMask)
            {
                case ClipFull:
                    ceiling = WallTop;
                    if (!line.HasBack)
                        floor = WallBottom;
                    else
                    {
                        floor = CeilingClip;
                        needrepeat = 1;
                    }

                    break;
                case ClipUpper:
                    tier = TierTop;
                    ceiling = WallTop;
                    floor = CeilingClip;
                    break;
                case ClipMid:
                    return;
                case ClipLower:
                    tier = TierBottom;
                    ceiling = FloorClip;
                    floor = WallBottom;
                    break;
                default:
                    return;
            }
        }

        string map = BaseColormap;
        if (decal.Style == StyleAdd && ColormapFade != 0)
            map = "special " + ColormapColor.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + " 0 " + ColormapDesaturate.ToString(System.Globalization.CultureInfo.InvariantCulture);

        int repeat = needrepeat;
        do
        {
            if (decal.Visible)
            {
                double drawn = z + decal.TopOffset * decal.ScaleY;
                float alpha = (float)decal.Alpha;
                Log.Add(
                    decal.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " z " + Text(z) +
                    " drawn " + Text(drawn) +
                    " tier " + tier.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " map " + map +
                    " flip " + (((decal.RenderFlags & XFlip) != 0) ? 1 : 0) +
                    " " + (((decal.RenderFlags & YFlip) != 0) ? 1 : 0) +
                    " bright " + (((decal.RenderFlags & FullBright) != 0) ? 1 : 0) +
                    " proj " + x1.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " " + x2.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " clip " + clipX1.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " " + clipX2.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " floor " + floor +
                    " ceil " + ceiling +
                    " edge " + Text(leftX) + " " + Text(leftY) + " " + Text(rightX) + " " + Text(rightY) +
                    " style " + decal.Style.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " alpha " + alpha.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) +
                    " trans " + decal.Translation.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    " color " + decal.AlphaColor.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            ceiling = FloorClip;
            floor = WallBottom;
        }
        while (repeat-- != 0);
    }

    static double RelativeZ(VmDecal1273 decal, VmDecalLine1273 line)
    {
        bool mid = (decal.RenderFlags & ClipMask) == ClipMid;
        double frontCeil = mid ? decal.SectorCeiling : line.FrontCeiling;
        double frontFloor = mid ? decal.SectorFloor : line.FrontFloor;
        double backCeil = line.HasBack ? line.BackCeiling : line.FrontCeiling;
        double backFloor = line.HasBack ? line.BackFloor : line.FrontFloor;
        switch (decal.RenderFlags & RelMask)
        {
            case RelUpper:
                if ((line.LineFlags & DontPegTop) != 0)
                    return decal.Z + frontCeil;
                return decal.Z + backCeil;
            case RelLower:
                if ((line.LineFlags & DontPegBottom) != 0)
                    return decal.Z + frontCeil;
                return decal.Z + backFloor;
            case RelMid:
                if ((line.LineFlags & DontPegBottom) != 0)
                    return decal.Z + frontFloor;
                return decal.Z + frontCeil;
            default:
                return decal.Z;
        }
    }

    static string Text(double value) =>
        value.ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
}
