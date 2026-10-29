namespace HCDE.Playsim;

/// <summary>
/// TEXTUREx composite, blend map, and true-color downconvert from
/// <c>multipatchtexture.cpp</c> (<b>1–345</b>). Chunk <b>1238</b>. Clip and the
/// BGRA blit are the host routines this file calls. Palette match, desaturate
/// tables, special-color tables, and the paletted ice remap are slice seams.
/// The 16-entry true-color ice palette is the table in <c>bitmap.cpp</c>.
/// </summary>
public sealed class VmMultiPatch1238
{
    public const int Normal = 0;
    public const int Luminance = 1;
    public const int NoRemap0 = 2;
    public const int BlendNone = 0;
    public const int BlendIce = 1;
    public const int BlendDesat1 = 2;
    public const int BlendDesat31 = 32;
    public const int BlendSpecial1 = 33;
    public const int BlendModulate = -1;
    public const int BlendOverlay = -2;
    public const int OpCopy = 0;
    public const int OpBlend = 1;
    public const int OpAdd = 2;
    public const int OpSubtract = 3;
    public const int OpReverseSubtract = 4;
    public const int OpModulate = 5;
    public const int OpCopyAlpha = 6;
    public const int OpCopyNewAlpha = 7;
    public const int OpOverlay = 8;
    public const int OpOverwrite = 9;
    public const int BlendUnit = 65536;
    const int SpecialCount = 4;

    public readonly List<string> Log = new();
    public readonly Dictionary<int, (int True, int Pal)> Counts = new();
    public readonly byte[] IceRemap = new byte[256];
    public readonly byte[][] DesatPal = new byte[31][];
    public readonly byte[][] SpecialPal = new byte[SpecialCount][];
    public readonly uint[] Base = new uint[256];
    int _nextId;

    static readonly byte[] IceRgb =
    {
        10, 8, 18, 15, 15, 26, 20, 16, 36, 30, 26, 46,
        40, 36, 57, 50, 46, 67, 59, 57, 78, 69, 67, 88,
        79, 77, 99, 89, 87, 109, 99, 97, 120, 109, 107, 130,
        118, 118, 141, 128, 128, 151, 138, 138, 162, 148, 148, 172
    };

    public sealed class Source
    {
        public int Id;
        public int Width;
        public int Height;
        public bool GamePalette;
        public bool Remap0;
        public byte[] Pal = Array.Empty<byte>();
        public byte[]? Rgba;
        public int Trans;
    }

    public sealed class Part
    {
        public Source Image = null!;
        public int OriginX;
        public int OriginY;
        public int Rotate;
        public int Op;
        public int Alpha = BlendUnit;
        public uint Blend;
        public byte[]? Remap;
        public uint[]? Palette;
    }

    public sealed class Texture
    {
        public int Id;
        public int Width;
        public int Height;
        public bool Complex;
        public bool Textual;
        public bool GamePalette;
        public Part[] Parts = Array.Empty<Part>();
    }

    struct Info
    {
        public int Op;
        public int Blend;
        public int Alpha;
        public int Inv;
        public int Bc0, Bc1, Bc2, Bc3;
        public int PixA;
    }

    public VmMultiPatch1238()
    {
        for (int i = 0; i < 256; i++)
        {
            IceRemap[i] = (byte)(i ^ 0x3c);
            Base[i] = (uint)(i | (((i + 1) & 255) << 8) | (((i + 2) & 255) << 16) | (255u << 24));
        }

        Base[0] &= 0x00ffffffu;
        for (int m = 0; m < DesatPal.Length; m++)
        {
            var row = new byte[256];
            for (int i = 0; i < 256; i++)
                row[i] = (byte)(i ^ (0x20 + m));
            DesatPal[m] = row;
        }

        for (int k = 0; k < SpecialCount; k++)
        {
            var row = new byte[256];
            for (int i = 0; i < 256; i++)
                row[i] = (byte)((i + 9 * (k + 1)) & 255);
            SpecialPal[k] = row;
        }
    }

    public static uint Pack(int a, int r, int g, int b)
    {
        return (uint)((b & 255) | ((g & 255) << 8) | ((r & 255) << 16) | ((a & 255) << 24));
    }

    public Texture Make(int w, int h, bool complex, bool textual, Part[] parts)
    {
        var copy = new Part[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            Part src = parts[i];
            copy[i] = new Part
            {
                Image = src.Image,
                OriginX = src.OriginX,
                OriginY = src.OriginY,
                Rotate = src.Rotate,
                Op = src.Op,
                Alpha = src.Alpha,
                Blend = src.Blend,
                Remap = src.Remap,
                Palette = src.Palette
            };
        }

        bool pal = false;
        if (!complex)
        {
            pal = true;
            for (int i = 0; i < copy.Length; i++)
            {
                if (!copy[i].Image.GamePalette)
                {
                    pal = false;
                    break;
                }
            }
        }

        var tex = new Texture
        {
            Id = ++_nextId,
            Width = w,
            Height = h,
            Complex = complex,
            Textual = textual,
            GamePalette = pal,
            Parts = copy
        };
        Log.Add("make id=" + tex.Id + " w=" + w + " h=" + h + " cx=" + (complex ? 1 : 0)
            + " tx=" + (textual ? 1 : 0) + " pal=" + (pal ? 1 : 0) + " n=" + copy.Length);
        return tex;
    }

    public bool Remap0(Texture tex)
    {
        bool ok = !tex.Textual && !tex.GamePalette;
        if (ok)
        {
            for (int i = 0; i < tex.Parts.Length; i++)
            {
                if (!tex.Parts[i].Image.Remap0)
                {
                    ok = false;
                    break;
                }
            }
        }

        Log.Add("remap0 id=" + tex.Id + " " + (ok ? 1 : 0));
        return ok;
    }

    public byte[] Pixels(Texture tex, int conversion, int frame)
    {
        int count = tex.Width * tex.Height;
        var pix = new byte[count];
        bool build = tex.Complex;
        int asked = conversion;
        if (conversion == Luminance)
            build = !tex.GamePalette;
        else if (!build)
        {
            for (int i = 0; i < tex.Parts.Length; i++)
            {
                if (tex.Parts[i].Op != OpCopy)
                    build = true;
            }
        }

        if (conversion == NoRemap0 && (tex.Textual || !tex.GamePalette))
            conversion = Normal;
        Log.Add("pixels id=" + tex.Id + " asked=" + asked + " used=" + conversion + " frame=" + frame
            + " build=" + (build ? "rgb" : "pal") + " pal=" + (tex.GamePalette ? 1 : 0));
        if (!build)
        {
            var work = new byte[256];
            for (int i = 0; i < tex.Parts.Length; i++)
            {
                Part part = tex.Parts[i];
                byte[]? trans = part.Remap;
                if (part.Blend != 0)
                    trans = BlendMap(part.Blend, work);
                Log.Add("part " + i + " " + Kind(part.Blend, trans));
                CopyToBlock(pix, tex.Width, tex.Height, part.Image, part.OriginX, part.OriginY, part.Rotate, trans, conversion);
            }
        }
        else
        {
            var bmp = new byte[count * 4];
            CopyInto(tex, bmp, Normal, 0);
            bool lum = conversion == Luminance;
            for (int y = 0; y < tex.Height; y++)
            {
                int inn = tex.Width * y * 4;
                int outAt = y;
                for (int x = 0; x < tex.Width; x++)
                {
                    if (pix[outAt] == 0 && bmp[inn + 3] != 0)
                        pix[outAt] = RgbToPal(lum, bmp[inn + 2], bmp[inn + 1], bmp[inn]);
                    outAt += tex.Height;
                    inn += 4;
                }
            }
        }

        Log.Add("pix " + Hex(pix));
        return pix;
    }

    public int Copy(Texture tex, int conversion, int frame, out byte[] bmp)
    {
        bmp = new byte[tex.Width * tex.Height * 4];
        return CopyInto(tex, bmp, conversion, frame);
    }

    public void Precache(Texture tex, bool requireTrue)
    {
        Note(tex.Id, requireTrue);
        bool child = requireTrue;
        if (!child)
        {
            child = tex.Complex;
            if (!child)
            {
                for (int i = 0; i < tex.Parts.Length; i++)
                {
                    if (tex.Parts[i].Op != OpCopy)
                        child = true;
                }
            }
        }

        Log.Add("pre id=" + tex.Id + " in=" + (requireTrue ? 1 : 0) + " child=" + (child ? 1 : 0));
        for (int i = 0; i < tex.Parts.Length; i++)
        {
            int id = tex.Parts[i].Image.Id;
            Note(id, child);
            (int tc, int pal) = Counts[id];
            Log.Add("child " + id + " tc=" + tc + " pal=" + pal);
        }

        (int selfTc, int selfPal) = Counts[tex.Id];
        Log.Add("self tc=" + selfTc + " pal=" + selfPal);
    }

    int CopyInto(Texture tex, byte[] bmp, int conversion, int frame)
    {
        int retv = -1;
        int asked = conversion;
        if (conversion == NoRemap0 && (tex.Textual || !tex.GamePalette))
            conversion = Normal;
        Log.Add("copy id=" + tex.Id + " asked=" + asked + " used=" + conversion + " frame=" + frame);
        for (int i = 0; i < tex.Parts.Length; i++)
        {
            Part part = tex.Parts[i];
            int ret = -1;
            var info = new Info { Alpha = part.Alpha, Inv = BlendUnit - part.Alpha, Op = part.Op };
            uint blend = part.Blend;
            int ba = (int)(blend >> 24);
            if (ba == 0 && blend != 0)
                info.Blend = unchecked((int)blend);
            else if (ba != 0)
            {
                int br = (int)((blend >> 16) & 255);
                int bg = (int)((blend >> 8) & 255);
                int bb = (int)(blend & 255);
                if (ba == 255)
                {
                    info.Bc0 = br * BlendUnit / 255;
                    info.Bc1 = bg * BlendUnit / 255;
                    info.Bc2 = bb * BlendUnit / 255;
                    info.Blend = BlendModulate;
                }
                else
                {
                    int blendAlpha = ba * BlendUnit / 255;
                    info.Bc0 = br * blendAlpha;
                    info.Bc1 = bg * blendAlpha;
                    info.Bc2 = bb * blendAlpha;
                    info.Bc3 = BlendUnit - blendAlpha;
                    info.Blend = BlendOverlay;
                }
            }

            byte[] src = CacheBitmap(part, conversion, ref ret);
            Log.Add("info i=" + i + " op=" + info.Op + " a=" + info.Alpha + " inv=" + info.Inv
                + " blend=" + info.Blend + " bc=" + info.Bc0 + "," + info.Bc1 + "," + info.Bc2 + "," + info.Bc3
                + " ret=" + ret);
            Blit(bmp, tex.Width, tex.Height, src, part.Image.Width, part.Image.Height, part.OriginX, part.OriginY, part.Rotate, info);
            if (ret == -1)
                retv = ret;
            else if (retv != -1 && ret > retv)
                retv = ret;
        }

        Log.Add("retv " + retv);
        Log.Add("bmp " + Hex(bmp));
        return retv;
    }

    byte[]? BlendMap(uint blend, byte[] work)
    {
        if (blend == (uint)BlendIce)
            return IceRemap;
        if (blend >= (uint)BlendSpecial1 && blend < (uint)(BlendSpecial1 + SpecialCount))
            return SpecialPal[blend - (uint)BlendSpecial1];
        if (blend >= (uint)BlendDesat1 && blend <= (uint)BlendDesat31)
            return DesatPal[blend - (uint)BlendDesat1];
        work[0] = 0;
        int a = (int)(blend >> 24);
        int r = (int)((blend >> 16) & 255);
        int g = (int)((blend >> 8) & 255);
        int b = (int)(blend & 255);
        if (a == 255)
        {
            for (int i = 1; i < 256; i++)
                work[i] = Pick(r * R(i) / 255, g * G(i) / 255, b * i / 255);
            return work;
        }

        if (a != 0)
        {
            int inv = 255 - a;
            for (int i = 1; i < 256; i++)
                work[i] = Pick((r * a + R(i) * inv) / 255, (g * a + G(i) * inv) / 255, (b * a + i * inv) / 255);
            return work;
        }

        return null;
    }

    void CopyToBlock(byte[] dest, int dw, int dh, Source source, int xpos, int ypos, int rotate, byte[]? trans, int style)
    {
        if (style != 0)
        {
            if (trans != null)
                Log.Add("style clear");
            trans = null;
        }

        int srcW = source.Width;
        int srcH = source.Height;
        int stepX = srcH;
        int stepY = 1;
        int pix = 0;
        int x0 = xpos;
        int y0 = ypos;
        if (!Clip(ref xpos, ref ypos, ref pix, ref srcW, ref srcH, ref stepX, ref stepY, rotate, 0, 0, dw, dh))
        {
            Log.Add("clip miss x=" + x0 + " y=" + y0);
            return;
        }

        Log.Add("block x=" + xpos + " y=" + ypos + " w=" + srcW + " h=" + srcH + " rot=" + rotate + " map=" + (trans != null ? 1 : 0));
        int destAt = ypos + dh * xpos;
        for (int x = 0; x < srcW; x++)
        {
            int pos = x * dh;
            for (int y = 0; y < srcH; y++, pos++)
            {
                int at = pix + y * stepY + x * stepX;
                if ((uint)at >= (uint)source.Pal.Length)
                {
                    Log.Add("src oob");
                    return;
                }

                byte v = source.Pal[at];
                if (v == 0)
                    continue;
                if (trans != null)
                    v = trans[v];
                dest[destAt + pos] = v;
            }
        }
    }

    byte[] CacheBitmap(Part part, int conversion, ref int ret)
    {
        ret = part.Image.Trans;
        Log.Add("cache id=" + part.Image.Id + " conv=" + conversion + " ret=" + ret + " pal=" + (part.Palette != null ? 1 : 0));
        if (part.Palette != null)
            return Expand(part.Image, part.Palette);
        if (part.Image.Rgba != null)
            return part.Image.Rgba;
        return Expand(part.Image, Base);
    }

    static byte[] Expand(Source source, uint[] pal)
    {
        int w = source.Width;
        int h = source.Height;
        var bmp = new byte[w * h * 4];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int v = source.Pal[y + x * h];
                uint pe = pal[v];
                int a = (int)(pe >> 24);
                if (a == 0)
                    continue;
                int o = (y * w + x) * 4;
                bmp[o] = (byte)pe;
                bmp[o + 1] = (byte)(pe >> 8);
                bmp[o + 2] = (byte)(pe >> 16);
                bmp[o + 3] = (byte)a;
            }
        }

        return bmp;
    }

    void Blit(byte[] dest, int dw, int dh, byte[] src, int sw, int sh, int ox, int oy, int rotate, Info info)
    {
        int stepX = 4;
        int stepY = sw * 4;
        int srcW = sw;
        int srcH = sh;
        int pix = 0;
        int x0 = ox;
        int y0 = oy;
        if (!Clip(ref ox, ref oy, ref pix, ref srcW, ref srcH, ref stepX, ref stepY, rotate, 0, 0, dw, dh))
        {
            Log.Add("clip miss x=" + x0 + " y=" + y0);
            return;
        }

        Log.Add("blit x=" + ox + " y=" + oy + " w=" + srcW + " h=" + srcH + " rot=" + rotate);
        int pitch = dw * 4;
        for (int y = 0; y < srcH; y++)
        {
            int row = pix + y * stepY;
            int dout = (oy + y) * pitch + ox * 4;
            for (int x = 0; x < srcW; x++)
            {
                int si = row + x * stepX;
                if (si < 0 || si + 3 >= src.Length)
                {
                    Log.Add("blit oob");
                    return;
                }

                Paint(dest, dout + x * 4, src[si], src[si + 1], src[si + 2], src[si + 3], info);
            }
        }
    }

    void Paint(byte[] dest, int at, byte sb, byte sg, byte sr, int a, Info info)
    {
        if ((uint)info.Op > OpOverwrite)
        {
            Log.Add("op oob " + info.Op);
            return;
        }

        if (info.Op != OpOverwrite && a == 0)
            return;
        int r = sr;
        int g = sg;
        int b = sb;
        int blend = info.Blend;
        if (blend == BlendIce)
        {
            int gray = Gray(sb, sg, sr) >> 4;
            r = IceRgb[gray * 3];
            g = IceRgb[gray * 3 + 1];
            b = IceRgb[gray * 3 + 2];
        }
        else if (blend == BlendModulate)
        {
            r = (sr * info.Bc0) >> 16;
            g = (sg * info.Bc1) >> 16;
            b = (sb * info.Bc2) >> 16;
        }
        else if (blend == BlendOverlay)
        {
            r = (sr * info.Bc3 + info.Bc0) >> 16;
            g = (sg * info.Bc3 + info.Bc1) >> 16;
            b = (sb * info.Bc3 + info.Bc2) >> 16;
        }
        else if (blend >= BlendSpecial1)
        {
            int k = blend - BlendSpecial1;
            if ((uint)k >= SpecialCount)
            {
                Log.Add("special oob " + blend);
                return;
            }

            int gray = Math.Clamp(Gray(sb, sg, sr), 0, 255);
            r = gray;
            g = k + 1;
            b = 255 - gray;
        }
        else if (blend >= BlendDesat1 && blend <= BlendDesat31)
        {
            int fac = blend - BlendDesat1 + 1;
            int gray = Gray(sb, sg, sr);
            r = (sr * (31 - fac) + gray * fac) / 31;
            g = (sg * (31 - fac) + gray * fac) / 31;
            b = (sb * (31 - fac) + gray * fac) / 31;
        }
        else if (blend != BlendNone)
            return;

        info.PixA = a;
        OpC(dest, at + 2, r & 255, info);
        OpC(dest, at + 1, g & 255, info);
        OpC(dest, at, b & 255, info);
        OpA(dest, at + 3, a, info);
    }

    static void OpC(byte[] dest, int i, int s, Info info)
    {
        int d = dest[i];
        int n = info.Op switch
        {
            OpBlend => (d * info.Inv + s * info.Alpha) >> 16,
            OpAdd => Math.Min((d * BlendUnit + s * info.Alpha) >> 16, 255),
            OpSubtract => Math.Max((d * BlendUnit - s * info.Alpha) >> 16, 0),
            OpReverseSubtract => Math.Max((-d * BlendUnit + s * info.Alpha) >> 16, 0),
            OpModulate => s * d / 255,
            OpCopyAlpha => (s * info.PixA + d * (255 - info.PixA)) / 255,
            OpOverlay => (s * info.PixA + d * (255 - info.PixA)) / 255,
            _ => s
        };
        dest[i] = (byte)n;
    }

    static void OpA(byte[] dest, int i, int a, Info info)
    {
        int d = dest[i];
        int n = info.Op switch
        {
            OpCopyNewAlpha => (a * info.Alpha) >> 16,
            OpOverlay => Math.Max(a, d),
            _ => a
        };
        dest[i] = (byte)n;
    }

    void Note(int id, bool trueColor)
    {
        if (Counts.TryGetValue(id, out (int True, int Pal) cur))
            Counts[id] = (cur.True + (trueColor ? 1 : 0), cur.Pal + (trueColor ? 0 : 1));
        else
            Counts[id] = (trueColor ? 1 : 0, trueColor ? 0 : 1);
    }

    static string Kind(uint blend, byte[]? map)
    {
        if (blend == 0)
            return "off";
        if (map == null)
            return "null";
        if (blend == (uint)BlendIce)
            return "ice";
        if (blend >= (uint)BlendDesat1 && blend <= (uint)BlendDesat31)
            return "desat " + (blend - (uint)BlendDesat1);
        if (blend >= (uint)BlendSpecial1 && blend < (uint)(BlendSpecial1 + SpecialCount))
            return "special " + (blend - (uint)BlendSpecial1);
        int a = (int)(blend >> 24);
        if (a == 255)
            return "mod";
        if (a != 0)
            return "over";
        return "other";
    }

    static bool Clip(ref int ox, ref int oy, ref int patch, ref int srcW, ref int srcH, ref int stepX, ref int stepY, int rotate, int cx, int cy, int cw, int ch)
    {
        int psx = stepX;
        int psy = stepY;
        int pixX;
        int pixY;
        int sx;
        int sy;
        switch (rotate)
        {
            case 1:
                pixX = 0;
                pixY = srcH - 1;
                sx = -psy;
                sy = psx;
                break;
            case 2:
                pixX = srcW - 1;
                pixY = srcH - 1;
                sx = -psx;
                sy = -psy;
                break;
            case 3:
                pixX = srcW - 1;
                pixY = 0;
                sx = psy;
                sy = -psx;
                break;
            case 4:
                pixX = srcW - 1;
                pixY = 0;
                sx = -psx;
                sy = psy;
                break;
            case 5:
                pixX = srcW - 1;
                pixY = srcH - 1;
                sx = -psy;
                sy = -psx;
                break;
            case 6:
                pixX = 0;
                pixY = srcH - 1;
                sx = psx;
                sy = -psy;
                break;
            case 7:
                pixX = 0;
                pixY = 0;
                sx = psy;
                sy = psx;
                break;
            default:
                pixX = 0;
                pixY = 0;
                sx = psx;
                sy = psy;
                break;
        }

        if ((rotate & 1) != 0)
        {
            int v = srcW;
            srcW = srcH;
            srcH = v;
        }

        patch += pixX * psx + pixY * psy;
        stepX = sx;
        stepY = sy;
        if (ox < cx)
        {
            int skip = cx - ox;
            srcW -= skip;
            patch += skip * sx;
            ox = cx;
            if (srcW <= 0)
                return false;
        }

        if (ox + srcW > cx + cw)
        {
            srcW = cx + cw - ox;
            if (srcW <= 0)
                return false;
        }

        if (oy < cy)
        {
            int skip = cy - oy;
            srcH -= skip;
            patch += skip * sy;
            oy = cy;
            if (srcH <= 0)
                return false;
        }

        if (oy + srcH > cy + ch)
        {
            srcH = cy + ch - oy;
            if (srcH <= 0)
                return false;
        }

        return true;
    }

    static int Gray(byte b, byte g, byte r) => (r * 77 + g * 143 + b * 36) >> 8;

    static int Lum(int r, int g, int b) => (r * 77 + g * 143 + b * 37) >> 8;

    static int R(int i) => (i + 2) & 255;

    static int G(int i) => (i + 1) & 255;

    static byte Pick(int r, int g, int b) => (byte)((r * 3 + g * 5 + b * 7) & 255);

    static byte RgbToPal(bool lum, int r, int g, int b)
    {
        if (lum)
            return (byte)(Lum(r, g, b) * 255 / 255);
        return (byte)(((r >> 2) * 3 + (g >> 2) * 5 + (b >> 2) * 7) & 255);
    }

    static string Hex(byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];
        const string digits = "0123456789abcdef";
        for (int i = 0; i < bytes.Length; i++)
        {
            c[i * 2] = digits[bytes[i] >> 4];
            c[i * 2 + 1] = digits[bytes[i] & 15];
        }

        return new string(c);
    }
}
