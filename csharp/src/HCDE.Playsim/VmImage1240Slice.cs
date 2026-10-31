namespace HCDE.Playsim;

/// <summary>
/// Paletted cache, true-color cache, and the format probe order from <c>image.cpp</c>
/// (<b>1–383</b>). Chunk <b>1240</b>. The paletted cache does not store the requested
/// frame. The slice keeps that field at 0 and the log says it was unset.
/// <c>CopyPixelData</c> with a null copy-info is the column-major palette copy below.
/// Index 0 of the base palette is transparent. A null true-color cache bitmap is the
/// native fault path and is not removed.
/// </summary>
public sealed class VmImage1240
{
    public const int Normal = 0;
    public const int Luminance = 1;
    public const int NoRemap0 = 2;

    public readonly List<string> Log = new();
    public readonly uint[] BaseColors = new uint[256];
    public readonly Dictionary<int, byte[]> Open = new();
    public readonly Dictionary<string, Func<int, bool, Image?>> Try = new();

    readonly List<Image?> _forLump = new();
    readonly Dictionary<int, Counts> _counts = new();
    readonly List<PalSlot> _pal = new();
    readonly List<RgbaSlot> _rgba = new();
    int _nextId;

    static readonly (string Name, bool Flat)[] Probes =
    {
        ("IMGZ", false),
        ("PNG", false),
        ("DDS", false),
        ("PCX", false),
        ("STB", false),
        ("QOI", false),
        ("WEBP", false),
        ("TGA", false),
        ("ANM", false),
        ("STARTUP", false),
        ("RAW", false),
        ("FLAT", true),
        ("PATCH", false),
        ("EMPTY", false),
        ("AUTOMAP", false)
    };

    public sealed class Image
    {
        public int Id;
        public int Width;
        public int Height;
        public int NumOfFrames = 1;
        public Func<int, int, byte[]>? Paint;
    }

    sealed class Counts
    {
        public int Tc;
        public int Pal;
    }

    sealed class PalSlot
    {
        public int Id;
        public int Frame;
        public int Ref;
        public byte[] Data = new byte[0];
    }

    sealed class RgbaSlot
    {
        public int Id;
        public int Frame;
        public int Ref;
        public int Trans;
        public byte[]? Data;
        public int Width;
        public int Height;
    }

    sealed class Pix
    {
        public byte[] Data = new byte[0];
        public bool Owned;
    }

    public VmImage1240()
    {
        for (int i = 0; i < 256; i++)
            BaseColors[i] = Pack((byte)i, (byte)((i + 1) & 255), (byte)((i + 2) & 255), i == 0 ? (byte)0 : (byte)255);
    }

    public static uint Pack(byte b, byte g, byte r, byte a) =>
        (uint)(b | (g << 8) | (r << 16) | (a << 24));

    public Image Make(int w, int h, int frames, Func<int, int, byte[]>? paint)
    {
        var img = new Image
        {
            Id = ++_nextId,
            Width = w,
            Height = h,
            NumOfFrames = frames,
            Paint = paint
        };
        Log.Add("make id=" + img.Id + " w=" + w + " h=" + h + " frames=" + frames);
        return img;
    }

    public void Precache(Image? img, bool trueColor)
    {
        if (img == null)
        {
            Log.Add("precache null");
            return;
        }

        if (!_counts.TryGetValue(img.Id, out Counts? row))
        {
            row = new Counts();
            _counts[img.Id] = row;
        }

        row.Tc += trueColor ? 1 : 0;
        row.Pal += trueColor ? 0 : 1;
        Log.Add("count id=" + img.Id + " tc=" + row.Tc + " pal=" + row.Pal);
    }

    public void Begin()
    {
        _counts.Clear();
        Log.Add("begin");
    }

    public void End()
    {
        Log.Add("end was pal=" + _pal.Count + " rgba=" + _rgba.Count);
        _pal.Clear();
        _rgba.Clear();
    }

    public byte[] Cached(Image img, int conversion, int frame)
    {
        Pix pix = ReadPal(img, conversion, frame);
        Log.Add((pix.Owned ? "own " : "view ") + Hex(pix.Data));
        return pix.Data;
    }

    public byte[] Paletted(Image img, int conversion, int frame)
    {
        Pix pix = ReadPal(img, conversion, frame);
        if (pix.Owned)
        {
            Log.Add("take own " + Hex(pix.Data));
            return pix.Data;
        }

        byte[] copy = pix.Data.ToArray();
        Log.Add("take copy " + Hex(copy));
        return copy;
    }

    public byte[] Copy(Image img, int conversion, int frame)
    {
        byte[] bmp = Blank(img.Width, img.Height);
        int ret = CopyPixels(img, bmp, conversion, frame);
        Log.Add("ret " + ret);
        Log.Add("bmp " + Sample(bmp));
        return bmp;
    }

    public byte[] Bitmap(Image img, uint[]? remap, int conversion, int frame, out int trans)
    {
        Log.Add("bmp id=" + img.Id + " conv=" + conversion + " frame=" + frame + " remap=" + (remap == null ? 0 : 1));
        trans = -1;
        if (img.NumOfFrames == 1 && frame == 1)
        {
            Log.Add("frame 1 -> 0");
            frame = 0;
        }

        byte[] bmp;
        if (remap != null)
        {
            bmp = Blank(img.Width, img.Height);
            trans = CopyTranslated(img, bmp, remap, frame);
            Log.Add("remap trans=" + trans + " " + Sample(bmp));
            return bmp;
        }

        if (conversion == Luminance)
        {
            Log.Add("lum -> normal");
            conversion = Normal;
        }

        int index = conversion == Normal ? FindRgba(img.Id, frame) : -1;
        if (index >= 0)
        {
            RgbaSlot cache = _rgba[index];
            trans = cache.Trans;
            if (cache.Ref > 1)
            {
                cache.Ref--;
                if (cache.Data == null)
                {
                    Log.Add("share null trans=" + trans);
                    return new byte[0];
                }

                Log.Add("share trans=" + trans + " " + Sample(cache.Data));
                return cache.Data;
            }

            if (cache.Data != null)
            {
                bmp = cache.Data;
                _rgba.RemoveAt(index);
                Log.Add("move trans=" + trans + " " + Sample(bmp));
                return bmp;
            }

            bmp = Blank(img.Width, img.Height);
            trans = CopyPixels(img, bmp, Normal, frame);
            Log.Add("rgba hole keep n=" + _rgba.Count + " trans=" + trans + " " + Sample(bmp));
            return bmp;
        }

        _counts.TryGetValue(img.Id, out Counts? info);
        if (info == null || info.Tc <= 1 || conversion != Normal)
        {
            bmp = Blank(img.Width, img.Height);
            trans = CopyPixels(img, bmp, conversion, frame);
            Log.Add("fresh trans=" + trans + " " + Sample(bmp));
            return bmp;
        }

        var slot = new RgbaSlot
        {
            Id = img.Id,
            Frame = frame,
            Ref = info.Tc - 1,
            Width = img.Width,
            Height = img.Height,
            Data = Blank(img.Width, img.Height)
        };
        info.Tc = 0;
        trans = slot.Trans = CopyPixels(img, slot.Data, Normal, frame);
        _rgba.Add(slot);
        Log.Add("cache ref=" + slot.Ref + " trans=" + trans + " " + Sample(slot.Data));
        return slot.Data;
    }

    public void HoldRgbaHole(Image img, int frame, int refs, int trans)
    {
        _rgba.Add(new RgbaSlot
        {
            Id = img.Id,
            Frame = frame,
            Ref = refs,
            Trans = trans,
            Data = null,
            Width = 0,
            Height = 0
        });
        Log.Add("hole id=" + img.Id + " frame=" + frame + " ref=" + refs + " trans=" + trans);
    }

    public Image? GetImage(int lump, bool isflat)
    {
        if (lump == -1)
        {
            Log.Add("lump -1");
            return null;
        }

        if (lump >= _forLump.Count)
        {
            while (_forLump.Count <= lump)
                _forLump.Add(null);
            Log.Add("resize " + _forLump.Count);
        }

        Image? have = _forLump[lump];
        if (have != null)
        {
            Log.Add("hit " + lump + " id=" + have.Id);
            return have;
        }

        if (!Open.ContainsKey(lump))
        {
            Log.Add("closed " + lump);
            return null;
        }

        for (int i = 0; i < Probes.Length; i++)
        {
            (string name, bool flat) = Probes[i];
            if (flat && !isflat)
            {
                Log.Add("skip " + name);
                continue;
            }

            Log.Add("try " + name);
            Image? image = null;
            if (Try.TryGetValue(name, out Func<int, bool, Image?>? probe))
                image = probe(lump, isflat);
            if (image != null)
            {
                _forLump[lump] = image;
                Log.Add("keep " + name + " id=" + image.Id);
                return image;
            }
        }

        Log.Add("none");
        return null;
    }

    Pix ReadPal(Image img, int conversion, int frame)
    {
        Log.Add("pal id=" + img.Id + " conv=" + conversion + " frame=" + frame);
        int index = conversion == Normal ? FindPal(img.Id, frame) : -1;
        if (index >= 0)
        {
            PalSlot cache = _pal[index];
            if (cache.Ref > 1)
            {
                cache.Ref--;
                return View(cache.Data);
            }

            if (cache.Data.Length > 0)
            {
                byte[] data = cache.Data;
                _pal.RemoveAt(index);
                Log.Add("move");
                return new Pix { Data = data, Owned = true };
            }

            Log.Add("pal empty keep");
            return new Pix { Data = new byte[0], Owned = true };
        }

        _counts.TryGetValue(img.Id, out Counts? info);
        if (info == null || info.Pal <= 1 || conversion != Normal)
            return new Pix { Data = Create(img, conversion, frame), Owned = true };

        byte[] made = Create(img, Normal, frame);
        int refs = info.Pal - 1;
        info.Pal = 0;
        _pal.Add(new PalSlot { Id = img.Id, Frame = 0, Ref = refs, Data = made });
        Log.Add("pal frame unset asked " + frame + " store 0 ref=" + refs);
        return View(made);
    }

    static Pix View(byte[] data) => new() { Data = data, Owned = false };

    byte[] Create(Image img, int conversion, int frame)
    {
        Log.Add("create id=" + img.Id + " conv=" + conversion + " frame=" + frame);
        if (img.Paint != null)
            return img.Paint(conversion, frame);
        return new byte[img.Width * img.Height];
    }

    int CopyPixels(Image img, byte[] bmp, int conversion, int frame)
    {
        if (conversion == Luminance)
        {
            Log.Add("copy lum -> normal");
            conversion = Normal;
        }

        byte[] pix = Create(img, conversion, frame);
        Blit(bmp, img.Width, img.Height, pix, BaseColors);
        return 0;
    }

    int CopyTranslated(Image img, byte[] bmp, uint[] remap, int frame)
    {
        byte[] pix = Create(img, Normal, frame);
        Blit(bmp, img.Width, img.Height, pix, remap);
        return 0;
    }

    static void Blit(byte[] bmp, int width, int height, byte[] patch, uint[] palette)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int v = patch[y + x * height];
                uint pe = palette[v];
                int a = (int)(pe >> 24);
                if (a == 0)
                    continue;
                int pos = (y * width + x) * 4;
                bmp[pos] = (byte)pe;
                bmp[pos + 1] = (byte)(pe >> 8);
                bmp[pos + 2] = (byte)(pe >> 16);
                bmp[pos + 3] = (byte)a;
            }
        }
    }

    static byte[] Blank(int w, int h) => new byte[w * h * 4];

    int FindPal(int id, int frame)
    {
        for (int i = 0; i < _pal.Count; i++)
        {
            if (_pal[i].Id == id && _pal[i].Frame == frame)
                return i;
        }

        return -1;
    }

    int FindRgba(int id, int frame)
    {
        for (int i = 0; i < _rgba.Count; i++)
        {
            if (_rgba[i].Id == id && _rgba[i].Frame == frame)
                return i;
        }

        return -1;
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
}
