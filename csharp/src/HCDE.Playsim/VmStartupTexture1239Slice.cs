namespace HCDE.Playsim;

/// <summary>
/// Startup picture, notch, and Strife background pixels from <c>startuptexture.cpp</c>
/// (<b>1–368</b>). Chunk <b>1239</b>. <c>CheckIfRaw</c> is the host probe this file calls.
/// Palette matching and the game remap are slice seams. The notch paletted loop condition
/// is false on the first test, so that loop does not paint. The work buffer native leaves
/// uninitialized is zero here, and the log says so.
/// </summary>
public sealed class VmStartupTexture1239
{
    public const int Normal = 0;
    public const int Luminance = 1;

    public readonly List<string> Log = new();
    public readonly Dictionary<int, (string Name, byte[] Bytes)> Files = new();
    public readonly byte[] Remap = new byte[256];
    public readonly byte[] Gray = new byte[256];
    public readonly byte[] Pal8 = new byte[16];
    public readonly uint[] Pal32 = new uint[16];
    int _nextId;

    static readonly (string Name, int W, int H)[] StrifePics =
    {
        ("STRTPA1", 32, 64),
        ("STRTPB1", 32, 64),
        ("STRTPC1", 32, 64),
        ("STRTPD1", 32, 64),
        ("STRTLZ1", 16, 16),
        ("STRTLZ2", 16, 16),
        ("STRTBOT", 48, 48)
    };

    public sealed class Image
    {
        public int Id;
        public int Lump;
        public string Kind = "";
        public int Width;
        public int Height;
        public bool GamePalette;
    }

    public VmStartupTexture1239()
    {
        for (int i = 0; i < 256; i++)
        {
            Remap[i] = (byte)i;
            Gray[i] = (byte)(i ^ 0x22);
        }

        Remap[0] = 7;
    }

    public Image? TryCreate(int lump)
    {
        if (!Files.TryGetValue(lump, out (string Name, byte[] Bytes) file))
        {
            Log.Add("missing " + lump);
            return null;
        }

        string name = file.Name;
        byte[] data = file.Bytes;
        if (name == "STARTUP")
            return Gate(lump, data, "STARTUP", 153648, "startup", 640, 480, true);
        if (name == "NOTCH")
            return Gate(lump, data, "NOTCH", 16 * 23 / 2, "notch", 16, 23, false);
        if (name == "NETNOTCH")
            return Gate(lump, data, "NETNOTCH", 4 * 16 / 2, "notch", 4, 16, false);
        if (name == "STARTUP0")
            return Gate(lump, data, "STARTUP0", 64000, "back", 320, 200, false);
        for (int i = 0; i < StrifePics.Length; i++)
        {
            if (name == StrifePics[i].Name)
                return Gate(lump, data, name, StrifePics[i].W * StrifePics[i].H, "strife", StrifePics[i].W, StrifePics[i].H, false);
        }

        Log.Add("unknown " + name);
        return null;
    }

    public byte[] Pixels(Image img, int conversion, int frame)
    {
        byte[] data = Files[img.Lump].Bytes;
        byte[] map = conversion == Luminance ? Gray : Remap;
        Log.Add("pixels id=" + img.Id + " kind=" + img.Kind + " conv=" + conversion + " frame=" + frame + " map=" + (conversion == Luminance ? "gray" : "game"));
        byte[] pix = img.Kind switch
        {
            "startup" => StartupPixels(data, map),
            "notch" => NotchPixels(img, data, map),
            "strife" => Flip(data, img.Width, img.Height, map),
            _ => Background(data, map)
        };
        Log.Add("pix " + Sample(pix));
        return pix;
    }

    public int Copy(Image img, int conversion, int frame, out byte[] bmp)
    {
        Log.Add("copy id=" + img.Id + " kind=" + img.Kind + " conv=" + conversion + " frame=" + frame);
        if (img.Kind != "startup" && img.Kind != "notch")
        {
            bmp = Array.Empty<byte>();
            Log.Add("copy none");
            return -1;
        }

        byte[] data = Files[img.Lump].Bytes;
        if (img.Kind == "startup")
        {
            var raw = new byte[img.Width * img.Height * 4];
            int n = 0;
            Planar(data, 48, img.Width, img.Height, index =>
            {
                uint pe = Pal32[index];
                int at = n * 4;
                raw[at] = (byte)pe;
                raw[at + 1] = (byte)(pe >> 8);
                raw[at + 2] = (byte)(pe >> 16);
                raw[at + 3] = (byte)(pe >> 24);
                n++;
            });
            bmp = raw;
        }
        else
        {
            bmp = new byte[img.Width * img.Height * 4];
            int pairs = img.Width * img.Height / 2;
            for (int i = 0; i < pairs; i++)
            {
                Put(bmp, i * 2, Pal32[data[i] >> 4]);
                Put(bmp, i * 2 + 1, Pal32[data[i] & 15]);
            }
        }

        Log.Add("ret 0");
        Log.Add("bmp " + Sample(bmp));
        return 0;
    }

    Image? Gate(int lump, byte[] data, string name, int need, string kind, int w, int h, bool loadPalette)
    {
        if (!IsRaw(data, need))
        {
            Log.Add("reject " + name);
            return null;
        }

        if (loadPalette)
            LoadPalette(data);
        var img = new Image
        {
            Id = ++_nextId,
            Lump = lump,
            Kind = kind,
            Width = w,
            Height = h,
            GamePalette = false
        };
        Log.Add("make id=" + img.Id + " kind=" + kind + " w=" + w + " h=" + h + " pal=0");
        return img;
    }

    void LoadPalette(byte[] source)
    {
        for (int i = 0; i < 16; i++)
        {
            uint pe = Expand(source[i * 3], source[i * 3 + 1], source[i * 3 + 2]);
            Pal32[i] = pe;
            Pal8[i] = Pick(pe);
        }

        Log.Add("pal8 " + Hex(Pal8));
        Log.Add("pal32 " + HexU(Pal32));
    }

    byte[] StartupPixels(byte[] data, byte[] map)
    {
        var work = new byte[640 * 480];
        int n = 0;
        Planar(data, 48, 640, 480, index => work[n++] = Pal8[index]);
        return Flip(work, 640, 480, map);
    }

    byte[] NotchPixels(Image img, byte[] data, byte[] map)
    {
        var work = new byte[img.Width * img.Height];
        int steps = 0;
        for (int i = 0; i * img.Width * img.Height / 2 != 0; i++)
        {
            work[i * 2] = Pal8[data[i] >> 4];
            work[i * 2 + 1] = Pal8[data[i] & 15];
            steps++;
        }

        Log.Add("notch pal steps " + steps);
        if (steps == 0)
            Log.Add("notch work zero");
        return Flip(work, img.Width, img.Height, map);
    }

    byte[] Background(byte[] data, byte[] map)
    {
        var source = new byte[64000];
        Array.Fill(source, (byte)0xF0);
        int seek = 57 * 320;
        int count = 95 * 320;
        int dest = 41 * 320;
        int have = data.Length > seek ? Math.Min(count, data.Length - seek) : 0;
        if (have > 0)
            Buffer.BlockCopy(data, seek, source, dest, have);
        int last = dest + count - 1;
        Log.Add("back seek=" + seek + " read=" + have + " at=" + dest
            + " r0=" + source[0].ToString("x2")
            + " r41=" + source[dest].ToString("x2")
            + " rend=" + source[last].ToString("x2")
            + " r136=" + source[136 * 320].ToString("x2"));
        return Flip(source, 320, 200, map);
    }

    bool IsRaw(byte[] data, int desired)
    {
        if (data.Length != desired)
        {
            Log.Add("raw len " + data.Length + " need " + desired);
            return false;
        }

        if (data.Length < 8)
            return true;
        int height = (short)(data[2] | (data[3] << 8));
        int width = (short)(data[0] | (data[1] << 8));
        if (height > 0 && height < 510 && width > 0 && width < 15997)
        {
            bool gap = true;
            for (int x = 0; x < width; x++)
            {
                int pos = 8 + x * 4;
                if (pos + 4 > data.Length)
                {
                    Log.Add("raw oob");
                    return true;
                }

                uint ofs = (uint)(data[pos] | (data[pos + 1] << 8) | (data[pos + 2] << 16) | (data[pos + 3] << 24));
                if (ofs == (uint)width * 4 + 8)
                    gap = false;
                else if (ofs >= (uint)desired - 1)
                    return true;
                else
                {
                    while (ofs < (uint)desired)
                    {
                        if (ofs >= (uint)data.Length)
                        {
                            Log.Add("raw oob");
                            return true;
                        }

                        if (data[ofs] == 255)
                            return true;
                        if (ofs + 1 >= (uint)data.Length)
                        {
                            Log.Add("raw oob");
                            return true;
                        }

                        ofs += (uint)data[ofs + 1] + 4;
                    }

                    return true;
                }
            }

            if (gap)
                return true;
            Log.Add("raw no");
            return false;
        }

        return true;
    }

    void Planar(byte[] src, int origin, int width, int height, Action<int> write)
    {
        int plane = width / 8 * height;
        int s1 = origin;
        int s2 = s1 + plane;
        int s3 = s2 + plane;
        int s4 = s3 + plane;
        for (int y = height; y > 0; --y)
        {
            for (int x = width; x > 0; x -= 8)
            {
                byte p1 = At(src, s1);
                byte p2 = At(src, s2);
                byte p3 = At(src, s3);
                byte p4 = At(src, s4);
                write(((p4 & 0x80) | ((p3 & 0x80) >> 1) | ((p2 & 0x80) >> 2) | ((p1 & 0x80) >> 3)) >> 4);
                write(((p4 & 0x40) >> 3) | ((p3 & 0x40) >> 4) | ((p2 & 0x40) >> 5) | ((p1 & 0x40) >> 6));
                write((((p4 & 0x20) << 2) | ((p3 & 0x20) << 1) | (p2 & 0x20) | ((p1 & 0x20) >> 1)) >> 4);
                write(((p4 & 0x10) >> 1) | ((p3 & 0x10) >> 2) | ((p2 & 0x10) >> 3) | ((p1 & 0x10) >> 4));
                write((((p4 & 0x08) << 4) | ((p3 & 0x08) << 3) | ((p2 & 0x08) << 2) | ((p1 & 0x08) << 1)) >> 4);
                write(((p4 & 0x04) << 1) | (p3 & 0x04) | ((p2 & 0x04) >> 1) | ((p1 & 0x04) >> 2));
                write((((p4 & 0x02) << 6) | ((p3 & 0x02) << 5) | ((p2 & 0x02) << 4) | ((p1 & 0x02) << 3)) >> 4);
                write(((p4 & 0x01) << 3) | ((p3 & 0x01) << 2) | ((p2 & 0x01) << 1) | (p1 & 0x01));
                s1++;
                s2++;
                s3++;
                s4++;
            }
        }
    }

    static byte At(byte[] src, int i) => (uint)i < (uint)src.Length ? src[i] : (byte)0;

    static void Put(byte[] bmp, int pixel, uint pe)
    {
        int at = pixel * 4;
        bmp[at] = (byte)pe;
        bmp[at + 1] = (byte)(pe >> 8);
        bmp[at + 2] = (byte)(pe >> 16);
        bmp[at + 3] = (byte)(pe >> 24);
    }

    static byte[] Flip(byte[] src, int w, int h, byte[] map)
    {
        var dst = new byte[w * h];
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                int at = i + j * w;
                byte v = (uint)at < (uint)src.Length ? src[at] : (byte)0;
                dst[i * h + j] = map[v];
            }
        }

        return dst;
    }

    static uint Expand(byte r, byte g, byte b)
    {
        uint d = (uint)(b | (g << 8) | (r << 16) | (63 << 24));
        return (d << 2) | ((d >> 4) & 0x03030303);
    }

    static byte Pick(uint pe)
    {
        int r = (int)((pe >> 16) & 255);
        int g = (int)((pe >> 8) & 255);
        int b = (int)(pe & 255);
        return (byte)((r * 3 + g * 5 + b * 7) & 255);
    }

    static string Sample(byte[] bytes)
    {
        int n = Math.Min(16, bytes.Length);
        int fill = n == 0 ? 0 : bytes[0];
        int rest = bytes.Length > 1 ? bytes[1] : fill;
        int count = 0;
        int restCount = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == fill)
                count++;
            if (bytes[i] == rest)
                restCount++;
        }

        return Hex(bytes.AsSpan(0, n)) + " fill=" + fill.ToString("x2") + " n=" + count
            + " rest=" + rest.ToString("x2") + " m=" + restCount + "/" + bytes.Length;
    }

    static string Hex(ReadOnlySpan<byte> bytes)
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

    static string HexU(uint[] values)
    {
        var raw = new byte[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            raw[i * 4] = (byte)values[i];
            raw[i * 4 + 1] = (byte)(values[i] >> 8);
            raw[i * 4 + 2] = (byte)(values[i] >> 16);
            raw[i * 4 + 3] = (byte)(values[i] >> 24);
        }

        return Hex(raw);
    }
}
