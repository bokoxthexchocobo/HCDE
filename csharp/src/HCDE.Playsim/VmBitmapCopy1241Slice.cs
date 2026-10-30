namespace HCDE.Playsim;

/// <summary>
/// Copy dispatch, clip rect, paletted copy, and bitmap clear from <c>bitmap.cpp</c>
/// (<b>188–483</b>). Chunk <b>1241</b>. Format readers and the ten copy operators live in
/// the header; this slice is the table <c>CopyPixelData</c> and <c>CopyPixelDataRGB</c> call.
/// <c>Zero</c> counts rows with <c>y &lt; clip.height</c> and still starts at the first row.
/// <c>clip.x</c> is a byte offset. Blend shifts use 16 bits.
/// </summary>
public sealed class VmBitmapCopy1241
{
    public const int Copy = 0;
    public const int Blend = 1;
    public const int Add = 2;
    public const int Subtract = 3;
    public const int ReverseSubtract = 4;
    public const int Modulate = 5;
    public const int CopyAlpha = 6;
    public const int CopyNewAlpha = 7;
    public const int Overlay = 8;
    public const int Overwrite = 9;

    public const int Rgb = 0;
    public const int RgbT = 1;
    public const int Rgba = 2;
    public const int Ia = 3;
    public const int Cmyk = 4;
    public const int Ycbcr = 5;
    public const int Ycck = 6;
    public const int Bgr = 7;
    public const int Bgra = 8;
    public const int I16 = 9;
    public const int Rgb555 = 10;
    public const int Pal = 11;

    public readonly List<string> Log = new();

    static readonly byte[,] Ice =
    {
        { 10, 8, 18 }, { 15, 15, 26 }, { 20, 16, 36 }, { 30, 26, 46 },
        { 40, 36, 57 }, { 50, 46, 67 }, { 59, 57, 78 }, { 69, 67, 88 },
        { 79, 77, 99 }, { 89, 87, 109 }, { 99, 97, 120 }, { 109, 107, 130 },
        { 118, 118, 141 }, { 128, 128, 151 }, { 138, 138, 162 }, { 148, 148, 172 }
    };

    public sealed class Rect
    {
        public int X;
        public int Y;
        public int W;
        public int H;
    }

    public sealed class Surface
    {
        public byte[] Data = Array.Empty<byte>();
        public int Pitch;
        public Rect Clip = new();
    }

    public sealed class Info
    {
        public int Op;
        public int Blend;
        public int[] Color = new int[4];
        public int Alpha;
        public int InvAlpha;
        public uint[]? Palette;
        public uint[]? Special;
    }

    public Surface New(int w, int h, byte fill)
    {
        var data = new byte[w * h * 4];
        Array.Fill(data, fill);
        return new Surface
        {
            Data = data,
            Pitch = w * 4,
            Clip = new Rect { X = 0, Y = 0, W = w, H = h }
        };
    }

    public bool Intersect(Rect box, int ix, int iy, int iw, int ih)
    {
        if (ix > box.X)
        {
            box.W -= ix - box.X;
            box.X = ix;
        }
        else
        {
            iw -= box.X - ix;
        }

        if (iy > box.Y)
        {
            box.H -= iy - box.Y;
            box.Y = iy;
        }
        else
        {
            ih -= box.Y - iy;
        }

        if (iw < box.W)
            box.W = iw;
        if (ih < box.H)
            box.H = ih;
        bool ok = box.W > 0 && box.H > 0;
        Log.Add("intersect x=" + box.X + " y=" + box.Y + " w=" + box.W + " h=" + box.H + " ok=" + (ok ? 1 : 0));
        return ok;
    }

    public void Zero(Surface bmp)
    {
        int rows = 0;
        int buffer = 0;
        for (int y = bmp.Clip.Y; y < bmp.Clip.H; y++)
        {
            int at = buffer + bmp.Clip.X;
            int n = bmp.Clip.W * 4;
            for (int i = 0; i < n; i++)
                bmp.Data[at + i] = 0;
            buffer += bmp.Pitch;
            rows++;
        }

        Log.Add("zero rows=" + rows + " x=" + bmp.Clip.X);
        for (int row = 0; row < bmp.Data.Length / bmp.Pitch; row++)
            Log.Add("row " + Hex(bmp.Data.AsSpan(row * bmp.Pitch, bmp.Pitch)));
    }

    public void CopyRgb(
        Surface bmp, int ox, int oy, byte[] src, int srcAt, int sw, int sh, int sx, int sy,
        int rotate, int ct, Info? inf, byte tr, byte tg, byte tb)
    {
        if (!Clip(bmp.Clip, ref ox, ref oy, ref srcAt, ref sw, ref sh, ref sx, ref sy, rotate))
        {
            Log.Add("copy ct=" + ct + " rot=" + rotate + " no " + Hex(bmp.Data));
            return;
        }

        int op = inf == null ? Copy : inf.Op;
        if (ct < 0 || ct > Pal || op < 0 || op > Overwrite)
        {
            Log.Add("dispatch oob ct=" + ct + " op=" + op);
            return;
        }

        int origin = oy * bmp.Pitch + ox * 4;
        for (int y = 0; y < sh; y++)
            CopyRow(bmp.Data, origin + y * bmp.Pitch, src, srcAt + y * sy, sw, sx, ct, op, inf, tr, tg, tb);
        Log.Add("copy ct=" + ct + " rot=" + rotate + " ok " + Hex(bmp.Data));
    }

    public void CopyPal(
        Surface bmp, int ox, int oy, byte[] patch, int patchAt, int sw, int sh, int sx, int sy,
        int rotate, uint[] palette, Info? inf)
    {
        if (!Clip(bmp.Clip, ref ox, ref oy, ref patchAt, ref sw, ref sh, ref sx, ref sy, rotate))
        {
            Log.Add("pal no " + Hex(bmp.Data));
            return;
        }

        int op = inf == null ? Copy : inf.Op;
        if (op < 0 || op > Overwrite)
        {
            Log.Add("dispatch oob ct=pal op=" + op);
            return;
        }

        uint[] used = palette;
        if (inf != null && inf.Blend != 0)
        {
            used = Rebuild(palette, inf);
            Log.Add("pal rebuild");
        }
        else if (inf != null && inf.Palette != null)
        {
            used = inf.Palette;
            Log.Add("pal replace");
        }

        int origin = oy * bmp.Pitch + ox * 4;
        for (int y = 0; y < sh; y++)
        {
            for (int x = 0; x < sw; x++)
            {
                int v = patch[patchAt + y * sy + x * sx];
                uint pe = used[v];
                int a = (int)(pe >> 24);
                WriteChannels(bmp.Data, origin + y * bmp.Pitch + x * 4, (byte)pe, (byte)(pe >> 8), (byte)(pe >> 16), a, op, inf);
            }
        }

        Log.Add("pal op=" + op + " " + Hex(bmp.Data));
    }

    bool Clip(Rect cr, ref int ox, ref int oy, ref int patch, ref int sw, ref int sh, ref int sx, ref int sy, int rotate)
    {
        int pixX;
        int pixY;
        int stepX;
        int stepY;
        switch (rotate)
        {
            case 1:
                pixX = 0;
                pixY = sh - 1;
                stepX = -sy;
                stepY = sx;
                break;
            case 2:
                pixX = sw - 1;
                pixY = sh - 1;
                stepX = -sx;
                stepY = -sy;
                break;
            case 3:
                pixX = sw - 1;
                pixY = 0;
                stepX = sy;
                stepY = -sx;
                break;
            case 4:
                pixX = sw - 1;
                pixY = 0;
                stepX = -sx;
                stepY = sy;
                break;
            case 5:
                pixX = sw - 1;
                pixY = sh - 1;
                stepX = -sy;
                stepY = -sx;
                break;
            case 6:
                pixX = 0;
                pixY = sh - 1;
                stepX = sx;
                stepY = -sy;
                break;
            case 7:
                pixX = 0;
                pixY = 0;
                stepX = sy;
                stepY = sx;
                break;
            default:
                pixX = 0;
                pixY = 0;
                stepX = sx;
                stepY = sy;
                break;
        }

        if ((rotate & 1) != 0)
        {
            int swap = sw;
            sw = sh;
            sh = swap;
        }

        patch += pixX * sx + pixY * sy;
        sx = stepX;
        sy = stepY;
        if (ox < cr.X)
        {
            int skip = cr.X - ox;
            sw -= skip;
            patch += skip * sx;
            ox = cr.X;
            if (sw <= 0)
            {
                Log.Add("clip no");
                return false;
            }
        }

        if (ox + sw > cr.X + cr.W)
        {
            sw = cr.X + cr.W - ox;
            if (sw <= 0)
            {
                Log.Add("clip no");
                return false;
            }
        }

        if (oy < cr.Y)
        {
            int skip = cr.Y - oy;
            sh -= skip;
            patch += skip * sy;
            oy = cr.Y;
            if (sh <= 0)
            {
                Log.Add("clip no");
                return false;
            }
        }

        if (oy + sh > cr.Y + cr.H)
        {
            sh = cr.Y + cr.H - oy;
            if (sh <= 0)
            {
                Log.Add("clip no");
                return false;
            }
        }

        Log.Add("clip w=" + sw + " h=" + sh + " ox=" + ox + " oy=" + oy + " sx=" + sx + " sy=" + sy + " at=" + patch);
        return true;
    }

    void CopyRow(byte[] dst, int dstAt, byte[] src, int srcAt, int count, int step, int ct, int op, Info? inf, byte tr, byte tg, byte tb)
    {
        int blend = inf == null ? 0 : inf.Blend;
        if (blend <= -3)
        {
            Log.Add("blend skip");
            return;
        }

        if (blend >= 33 && (inf == null || inf.Special == null || blend != 33))
        {
            Log.Add("special oob " + (blend - 33));
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Read(ct, src, srcAt, tr, tg, tb, out int r, out int g, out int b, out int a, out int gray);
            ApplyBlend(ref r, ref g, ref b, gray, blend, inf);
            WriteChannels(dst, dstAt, (byte)b, (byte)g, (byte)r, a, op, inf);
            dstAt += 4;
            srcAt += step;
        }
    }

    uint[] Rebuild(uint[] palette, Info inf)
    {
        var built = new uint[256];
        var forced = new Info
        {
            Op = Copy,
            Blend = inf.Blend,
            Color = inf.Color,
            Alpha = inf.Alpha,
            InvAlpha = inf.InvAlpha,
            Special = inf.Special
        };
        var one = new byte[4];
        for (int i = 0; i < 256; i++)
        {
            byte[] raw = Bytes(palette[i]);
            Array.Clear(one);
            CopyRow(one, 0, raw, 0, 1, 4, Pal, Copy, forced, 0, 0, 0);
            built[i] = (uint)(one[0] | (one[1] << 8) | (one[2] << 16) | (one[3] << 24));
        }

        return built;
    }

    static void ApplyBlend(ref int r, ref int g, ref int b, int gray, int blend, Info? inf)
    {
        if (blend == 0)
            return;
        if (blend == 1)
        {
            int index = gray >> 4;
            r = Ice[index, 0];
            g = Ice[index, 1];
            b = Ice[index, 2];
            return;
        }

        if (blend == -1)
        {
            r = (r * inf!.Color[0]) >> 16;
            g = (g * inf.Color[1]) >> 16;
            b = (b * inf.Color[2]) >> 16;
            return;
        }

        if (blend == -2)
        {
            r = (r * inf!.Color[3] + inf.Color[0]) >> 16;
            g = (g * inf.Color[3] + inf.Color[1]) >> 16;
            b = (b * inf.Color[3] + inf.Color[2]) >> 16;
            return;
        }

        if (blend >= 33)
        {
            int grayIndex = gray < 0 ? 0 : gray > 255 ? 255 : gray;
            uint pe = inf!.Special![grayIndex];
            b = (int)(pe & 255);
            g = (int)((pe >> 8) & 255);
            r = (int)((pe >> 16) & 255);
            return;
        }

        int fac = blend - 2 + 1;
        int rr = (r * (31 - fac) + gray * fac) / 31;
        int gg = (g * (31 - fac) + gray * fac) / 31;
        int bb = (b * (31 - fac) + gray * fac) / 31;
        r = rr;
        g = gg;
        b = bb;
    }

    static void WriteChannels(byte[] dst, int at, byte b, byte g, byte r, int a, int op, Info? inf)
    {
        if (op != Overwrite && a == 0)
            return;
        OpC(op, dst, at + 2, r, a, inf);
        OpC(op, dst, at + 1, g, a, inf);
        OpC(op, dst, at, b, a, inf);
        OpA(op, dst, at + 3, a, inf);
    }

    static void OpC(int op, byte[] dst, int at, int s, int a, Info? inf)
    {
        int d = dst[at];
        int alpha = inf == null ? 0 : inf.Alpha;
        int inv = inf == null ? 0 : inf.InvAlpha;
        int n = op switch
        {
            Blend => (d * inv + s * alpha) >> 16,
            Add => Math.Min((d * 65536 + s * alpha) >> 16, 255),
            Subtract => Math.Max((d * 65536 - s * alpha) >> 16, 0),
            ReverseSubtract => Math.Max((-d * 65536 + s * alpha) >> 16, 0),
            Modulate => (s * d) / 255,
            CopyAlpha or Overlay => (s * a + d * (255 - a)) / 255,
            _ => s
        };
        dst[at] = (byte)n;
    }

    static void OpA(int op, byte[] dst, int at, int s, Info? inf)
    {
        int d = dst[at];
        int n = op switch
        {
            CopyNewAlpha => (s * (inf == null ? 0 : inf.Alpha)) >> 16,
            Overlay => Math.Max(s, d),
            _ => s
        };
        dst[at] = (byte)n;
    }

    static void Read(int ct, byte[] src, int i, byte tr, byte tg, byte tb, out int r, out int g, out int b, out int a, out int gray)
    {
        byte p0 = src[i];
        byte p1 = src[i + 1];
        byte p2 = ct == Ia || ct == I16 || ct == Rgb555 ? (byte)0 : src[i + 2];
        if (ct == Rgb || ct == RgbT || ct == Rgba || ct == Cmyk || ct == Ycbcr || ct == Ycck || ct == Bgr || ct == Bgra || ct == Pal)
            p2 = src[i + 2];
        byte p3 = 0;
        if (ct == Rgba || ct == Cmyk || ct == Ycck || ct == Bgra || ct == Pal)
            p3 = src[i + 3];
        switch (ct)
        {
            case RgbT:
                r = p0;
                g = p1;
                b = p2;
                a = p0 != tr || p1 != tg || p2 != tb ? 255 : 0;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            case Rgba:
                r = p0;
                g = p1;
                b = p2;
                a = p3;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            case Ia:
                r = g = b = p0;
                a = p1;
                gray = p0;
                return;
            case Cmyk:
                r = p3 - (((256 - p0) * p3) >> 8);
                g = p3 - (((256 - p1) * p3) >> 8);
                b = p3 - (((256 - p2) * p3) >> 8);
                a = 255;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            case Ycbcr:
                r = Clamp((int)(p0 + 1.40200 * (p2 - 128)));
                g = Clamp((int)(p0 - 0.34414 * (p1 - 128) - 0.71414 * (p2 - 128)));
                b = Clamp((int)(p0 + 1.77200 * (p1 - 128)));
                a = 255;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            case Ycck:
                int yr = Clamp((int)(p0 + 1.40200 * (p2 - 128)));
                int yg = Clamp((int)(p0 - 0.34414 * (p1 - 128) - 0.71414 * (p2 - 128)));
                int yb = Clamp((int)(p0 + 1.77200 * (p1 - 128)));
                r = p3 - ((yr * p3) >> 8);
                g = p3 - ((yg * p3) >> 8);
                b = p3 - ((yb * p3) >> 8);
                a = 255;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            case Bgr:
                r = p2;
                g = p1;
                b = p0;
                a = 255;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            case Bgra:
                r = p2;
                g = p1;
                b = p0;
                a = p3;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            case I16:
                r = g = b = p1;
                a = 255;
                gray = p1;
                return;
            case Rgb555:
                int pix = p0 | (p1 << 8);
                r = (pix & 0x1f) << 3;
                g = (pix & 0x3e0) >> 2;
                b = (pix & 0x7c00) >> 7;
                a = 255;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            case Pal:
                b = p0;
                g = p1;
                r = p2;
                a = p3;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
            default:
                r = p0;
                g = p1;
                b = p2;
                a = 255;
                gray = (r * 77 + g * 143 + b * 36) >> 8;
                return;
        }
    }

    static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

    static byte[] Bytes(uint pe) => new[] { (byte)pe, (byte)(pe >> 8), (byte)(pe >> 16), (byte)(pe >> 24) };

    static string Hex(ReadOnlySpan<byte> bytes)
    {
        int n = Math.Min(bytes.Length, 48);
        char[] c = new char[n * 2];
        const string digits = "0123456789abcdef";
        for (int i = 0; i < n; i++)
        {
            c[i * 2] = digits[bytes[i] >> 4];
            c[i * 2 + 1] = digits[bytes[i] & 15];
        }

        return new string(c);
    }
}
