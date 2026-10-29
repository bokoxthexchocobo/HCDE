using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Doom patch probe, column posts, and the bad-patch scan from <c>patchtexture.cpp</c>
/// (<b>1–311</b>). Chunk <b>1237</b>. A column that begins on the last three bytes is not
/// read. A column offset past the lump is a logged fault instead of a wild read. Alpha
/// matches go through <see cref="VmMd5Check1214Slice.CheckPatchForAlpha"/>.
/// </summary>
public sealed class VmPatchTexture1237
{
    public const int Normal = 0;
    public const int Luminance = 1;
    public const int NoRemap0 = 2;

    public readonly List<string> Log = new();
    public readonly Dictionary<int, byte[]> Files = new();
    public readonly byte[] Remap = new byte[256];
    public readonly byte[] GrayMap = new byte[256];
    public readonly byte[] GrayRamp = new byte[256];
    public readonly byte[] Grayscale = new byte[256];
    public readonly uint[] Base = new uint[256];
    public readonly uint[] GrayPal = new uint[256];
    int _nextId;

    public sealed class Image
    {
        public int Id;
        public int Lump;
        public int Width;
        public int Height;
        public int Left;
        public int Top;
        public bool Alpha;
        public bool GamePalette;
        public bool Bad;
        public bool Masked;
    }

    public VmPatchTexture1237()
    {
        for (int i = 0; i < 256; i++)
        {
            Remap[i] = (byte)i;
            GrayMap[i] = (byte)(i ^ 0x5a);
            GrayRamp[i] = (byte)(i ^ 0x11);
            Grayscale[i] = (byte)(i ^ 0x22);
            uint packed = (uint)(i | (((i + 1) & 255) << 8) | (((i + 2) & 255) << 16) | (255u << 24));
            Base[i] = packed;
            GrayPal[i] = (uint)((255 - i) | (i << 8) | (1 << 16) | (255u << 24));
        }

        Remap[0] = 7;
        Base[0] &= 0x00ffffffu;
    }

    public Image? TryCreate(int lump)
    {
        if (!Files.TryGetValue(lump, out byte[]? data) || data == null)
        {
            Log.Add("missing " + lump);
            return null;
        }

        int pos = 0;
        int width = ReadU16(data, ref pos);
        int height = ReadU16(data, ref pos);
        int left = ReadI16(data, ref pos);
        int top = ReadI16(data, ref pos);
        if (height > 0 && height <= 2048 && width > 0 && width <= 2048 && width < data.Length / 4
            && Math.Abs(left) < 4096 && Math.Abs(top) < 4096)
        {
            if (!Probe(data, out bool alpha))
            {
                Log.Add("miss");
                return null;
            }

            Log.Add("seek 8");
            return Make(lump, width, height, left, top, alpha);
        }

        Log.Add("reject w=" + width + " h=" + height + " lo=" + left + " to=" + top + " len=" + data.Length);
        return null;
    }

    public Image Make(int lump, int w, int h, int lo, int to, bool alpha)
    {
        if (!Files.TryGetValue(lump, out byte[]? data) || data == null)
            data = Array.Empty<byte>();
        var img = new Image
        {
            Id = ++_nextId,
            Lump = lump,
            Width = w,
            Height = h,
            Left = lo,
            Top = to,
            Alpha = alpha,
            GamePalette = !alpha,
            Bad = false,
            Masked = true
        };
        Detect(img, data);
        Log.Add("create id=" + img.Id + " lump=" + lump + " w=" + img.Width + " h=" + img.Height
            + " lo=" + img.Left + " to=" + img.Top + " alpha=" + (img.Alpha ? 1 : 0)
            + " pal=" + (img.GamePalette ? 1 : 0) + " bad=" + (img.Bad ? 1 : 0)
            + " mask=" + (img.Masked ? 1 : 0));
        return img;
    }

    public byte[] Pixels(Image img, int conversion, int frame)
    {
        if (!Files.TryGetValue(img.Lump, out byte[]? data) || data == null)
            data = Array.Empty<byte>();
        (byte[] table, string name) = PickRemap(conversion, img.Alpha);
        Log.Add("pixels conv=" + conversion + " frame=" + frame + " bad=" + (img.Bad ? 1 : 0) + " map=" + name);
        int count = img.Width > 0 && img.Height > 0 ? img.Width * img.Height : 0;
        var pix = new byte[count];
        if (img.Bad)
        {
            DrawBad(img, data, table, pix);
            Log.Add("pix " + Hex(pix));
            return pix;
        }

        int spans = img.Width;
        bool stopped = false;
        long max = (long)data.Length - 3;
        for (int x = 0; x < img.Width && !stopped; x++)
        {
            if (!ColumnOfs(data, x, out uint ofs))
            {
                Log.Add("draw oob x=" + x);
                break;
            }

            long col = ofs;
            int top = -1;
            while (col >= 0 && col < max)
            {
                int td = data[col];
                if (td == 0xff)
                    break;
                int raw = data[col + 1];
                if (td <= top)
                    top += td;
                else
                    top = td;
                int len = raw;
                if (len != 0)
                {
                    if (top + len > img.Height)
                        len = img.Height - top;
                    if (len > 0)
                    {
                        spans++;
                        for (int i = 0; i < len; i++)
                        {
                            long at = col + 3 + i;
                            int dst = x * img.Height + top + i;
                            if (at < 0 || at >= data.Length || dst < 0 || dst >= pix.Length)
                            {
                                Log.Add("draw oob x=" + x);
                                stopped = true;
                                break;
                            }

                            pix[dst] = table[data[at]];
                        }
                    }
                }

                if (stopped)
                    break;
                col += raw + 4;
            }
        }

        Log.Add("spans " + spans);
        Log.Add("pix " + Hex(pix));
        return pix;
    }

    public byte[] Copy(Image img, int conversion, int frame)
    {
        int used;
        uint[] pal;
        string palName;
        if (!img.Alpha)
        {
            used = conversion == Luminance ? Normal : conversion;
            Log.Add("copy base asked=" + conversion + " used=" + used + " frame=" + frame);
            pal = Base;
            palName = "base";
        }
        else
        {
            used = Normal;
            Log.Add("copy gray asked=" + conversion + " frame=" + frame);
            pal = GrayPal;
            palName = "gray";
        }

        byte[] pix = Pixels(img, used, frame);
        int width = img.Width;
        int height = img.Height;
        var bmp = new byte[width > 0 && height > 0 ? width * height * 4 : 0];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int v = pix[y + x * height];
                uint c = pal[v];
                if ((c >> 24) == 0)
                    continue;
                int pos = (y * width + x) * 4;
                bmp[pos] = (byte)c;
                bmp[pos + 1] = (byte)(c >> 8);
                bmp[pos + 2] = (byte)(c >> 16);
                bmp[pos + 3] = (byte)(c >> 24);
            }
        }

        Log.Add("bmp " + palName + " " + Hex(bmp));
        Log.Add("copy rc=0");
        return bmp;
    }

    public bool Remap0(Image img)
    {
        bool ok = !img.Bad;
        Log.Add("remap0 " + (ok ? 1 : 0));
        return ok;
    }

    bool Probe(byte[] data, out bool alpha)
    {
        alpha = false;
        if (data.Length < 13)
        {
            Log.Add("probe short");
            return false;
        }

        int width = I16(data, 0);
        int height = I16(data, 2);
        if (height > 0 && height <= 2048 && width > 0 && width <= 2048 && width < data.Length / 4)
        {
            bool gap = true;
            for (int x = 0; x < width; x++)
            {
                if (!ColumnOfs(data, x, out uint ofs))
                {
                    Log.Add("probe oob x=" + x);
                    return false;
                }

                uint end = (uint)(width * 4 + 8);
                if (ofs == end)
                    gap = false;
                else if (ofs >= (uint)data.Length)
                {
                    Log.Add("probe end x=" + x + " ofs=" + ofs);
                    return false;
                }
            }

            if (!gap)
            {
                alpha = VmMd5Check1214Slice.CheckPatchForAlpha(data, (uint)data.Length);
                Log.Add("alpha " + (alpha ? 1 : 0));
            }
            else
                Log.Add("probe gap");
            return !gap;
        }

        Log.Add("probe size");
        return false;
    }

    void Detect(Image img, byte[] data)
    {
        int need = img.Width * img.Height / 2;
        if (data.Length < need)
        {
            Log.Add("bad early len=" + data.Length + " need=" + need);
            return;
        }

        if (data.Length < 4)
        {
            Log.Add("bad short");
            return;
        }

        int hdrW = I16(data, 0);
        int hdrH = I16(data, 2);
        if (hdrH != 256)
            return;
        for (int x = 0; x < hdrW; x++)
        {
            if (!ColumnOfs(data, x, out uint ofs))
            {
                Log.Add("bad oob x=" + x);
                return;
            }

            long col = ofs;
            if (col < 0 || col + 1 >= data.Length)
            {
                Log.Add("bad oob x=" + x);
                return;
            }

            int td = data[col];
            int raw = data[col + 1];
            if (td != 0 || raw != 0)
            {
                Log.Add("bad post x=" + x + " top=" + td + " len=" + raw);
                return;
            }

            long term = col + 256 + 4;
            if (term < 0 || term >= data.Length)
            {
                Log.Add("bad oob x=" + x);
                return;
            }

            if (data[term] != 0xff)
            {
                Log.Add("bad extra x=" + x + " top=" + data[term]);
                return;
            }
        }

        img.Left = 0;
        img.Top = 0;
        img.Bad = true;
        img.Masked = false;
    }

    void DrawBad(Image img, byte[] data, byte[] table, byte[] pix)
    {
        int o = 0;
        for (int x = 0; x < img.Width; x++)
        {
            if (!ColumnOfs(data, x, out uint ofs))
            {
                Log.Add("draw oob x=" + x);
                return;
            }

            long inp = (long)ofs + 3;
            for (int y = img.Height; y > 0; y--)
            {
                if (inp < 0 || inp >= data.Length || o >= pix.Length)
                {
                    Log.Add("draw oob x=" + x);
                    return;
                }

                pix[o] = table[data[inp]];
                o++;
                inp++;
            }
        }
    }

    (byte[] Table, string Name) PickRemap(int conversion, bool alpha)
    {
        if (conversion == Luminance)
            return alpha ? (GrayRamp, "ramp") : (Grayscale, "gray");
        if (alpha)
            return (GrayMap, "graymap");
        if (conversion == NoRemap0)
        {
            var sky = new byte[256];
            Buffer.BlockCopy(Remap, 0, sky, 0, 256);
            sky[0] = 0;
            return (sky, "sky");
        }

        return (Remap, "remap");
    }

    static bool ColumnOfs(byte[] data, int x, out uint ofs)
    {
        ofs = 0;
        long at = 8L + (long)x * 4L;
        if (at < 0 || at + 4 > data.Length)
            return false;
        int i = (int)at;
        ofs = (uint)(data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24));
        return true;
    }

    static int ReadU16(byte[] data, ref int pos)
    {
        int v = 0;
        if (pos < data.Length)
        {
            v = data[pos];
            pos++;
        }

        if (pos < data.Length)
        {
            v |= data[pos] << 8;
            pos++;
        }

        return v;
    }

    static int ReadI16(byte[] data, ref int pos)
    {
        return (short)ReadU16(data, ref pos);
    }

    static int I16(byte[] data, int at)
    {
        return (short)(data[at] | (data[at + 1] << 8));
    }

    static string Hex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            sb.Append(bytes[i].ToString("x2"));
        return sb.ToString();
    }
}
