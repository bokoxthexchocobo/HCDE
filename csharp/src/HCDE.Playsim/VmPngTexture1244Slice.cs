using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// RGB and gray-alpha palettize, <c>CopyPixels</c>, and the savegame file texture from
/// <c>pngtexture.cpp</c> (<b>466–786</b>). Chunk <b>1244</b>. The gray and palette IDAT
/// decode stays chunk 106. <c>M_ReadIDAT</c> is that decode: this slice consumes the
/// expanded bytes. <c>RGBToPalette</c> indexes the 64-cube, which is not in this file.
/// Bitmap copies go through the chunk 1241 copier.
/// </summary>
public sealed class VmPngTexture1244
{
    public const int Luminance = 1;
    public const int SeekSet = 0;
    public const int SeekCur = 1;

    public readonly List<string> Log = new();
    public readonly Dictionary<int, byte[]?> Lumps = new();
    public readonly ushort[] NonPaletteTrans = new ushort[3];

    public int SourceLump;
    public int Width;
    public int Height;
    public int BitDepth = 8;
    public int ColorType;
    public int Interlace;
    public int PaletteSize;
    public bool HaveTrans;
    public byte[]? PaletteMap;
    public uint StartOfIdat;
    public Func<int, int, int, int>? Cube;
    public VmPngTexture1243? PaletteHost;
    public VmBitmapCopy1241.Surface? LastBitmap;
    public int LastTranspal;

    static readonly int[] Bpp = { 1, 0, 3, 1, 2, 0, 4 };
    readonly uint _idat = MakeId("IDAT");
    readonly uint _iend = MakeId("IEND");
    readonly uint _plte = MakeId("PLTE");
    readonly uint _trns = MakeId("tRNS");

    public byte[] Palettize(byte[]? decoded, int conversion)
    {
        bool lum = conversion == Luminance;
        int bpp = ColorType == 2 ? 3 : ColorType == 4 ? 2 : 4;
        byte[] temp = Take(decoded, Width, Height, bpp, "rgb");
        int n = NonNeg(Width) * NonNeg(Height);
        if (Width <= 0 || Height <= 0)
        {
            Log.Add($"palettize bad w={Width} h={Height}");
            return Array.Empty<byte>();
        }
        var pixels = new byte[n];
        int i = 0;
        int o = 0;
        switch (ColorType)
        {
            case 2:
                WalkRgb(temp, pixels, ref i, ref o, Width * 3, Height * Width * 3 - 3, lum, alpha: false);
                break;
            case 4:
                EnsurePalette();
                WalkGray(temp, pixels, ref i, ref o, Width * 2, Height * Width * 2 - 2, lum);
                break;
            case 6:
                WalkRgb(temp, pixels, ref i, ref o, Width * 4, Height * Width * 4 - 4, lum, alpha: true);
                break;
            default:
                Log.Add($"rgb skip type={ColorType}");
                break;
        }
        Log.Add($"palettize type={ColorType} lum={(lum ? 1 : 0)} n={pixels.Length} out={Hex(pixels)}");
        return pixels;
    }

    public int CopyPixels(byte[]? file, byte[]? decoded, int conversion, int frame)
    {
        _ = conversion;
        _ = frame;
        int transpal = 0;
        int pixwidth = PixelWidth(ColorType, Width);
        var pe = new uint[256];
        for (int i = 0; i < 256; i++)
            pe[i] = Pack(255, (byte)i, (byte)i, (byte)i);
        var reader = Open(file, "");
        reader.Seek(33, SeekSet);
        Walk(reader, pe, ref transpal, setSize: false);
        if (ColorType == 0 && HaveTrans && NonPaletteTrans[0] < 256)
        {
            pe[NonPaletteTrans[0]] = WithA(pe[NonPaletteTrans[0]], 0);
            transpal = 1;
            Log.Add($"gray a0 idx={NonPaletteTrans[0]}");
        }
        byte[] raw = pixwidth <= 0 || Height <= 0
            ? Array.Empty<byte>()
            : Take(decoded, pixwidth, Height, 1, "copy");
        LogIdat(reader, pixwidth, raw.Length);
        LastBitmap = null;
        if (Width <= 0 || Height <= 0 || pixwidth <= 0)
        {
            Log.Add($"copy skip type={ColorType} transpal={transpal}");
            LastTranspal = transpal;
            return transpal;
        }
        var copier = new VmBitmapCopy1241();
        var surface = copier.New(Width, Height, 0);
        switch (ColorType)
        {
            case 0:
            case 3:
                copier.CopyPal(surface, 0, 0, raw, 0, Width, Height, 1, Width, 0, pe, null);
                break;
            case 2:
                if (HaveTrans && (NonPaletteTrans[0] > 255 || NonPaletteTrans[1] > 255 || NonPaletteTrans[2] > 255))
                    Log.Add("trns wide");
                if (!HaveTrans || NonPaletteTrans[0] > 255 || NonPaletteTrans[1] > 255 || NonPaletteTrans[2] > 255)
                    copier.CopyRgb(surface, 0, 0, raw, 0, Width, Height, 3, pixwidth, 0, VmBitmapCopy1241.Rgb, null, 0, 0, 0);
                else
                    copier.CopyRgb(surface, 0, 0, raw, 0, Width, Height, 3, pixwidth, 0, VmBitmapCopy1241.RgbT, null,
                        (byte)NonPaletteTrans[0], (byte)NonPaletteTrans[1], (byte)NonPaletteTrans[2]);
                if (HaveTrans)
                    transpal = 1;
                break;
            case 4:
                copier.CopyRgb(surface, 0, 0, raw, 0, Width, Height, 2, pixwidth, 0, VmBitmapCopy1241.Ia, null, 0, 0, 0);
                transpal = -1;
                break;
            case 6:
                copier.CopyRgb(surface, 0, 0, raw, 0, Width, Height, 4, pixwidth, 0, VmBitmapCopy1241.Rgba, null, 0, 0, 0);
                transpal = -1;
                break;
            default:
                Log.Add($"copy skip type={ColorType}");
                break;
        }
        LastBitmap = surface;
        foreach (string line in copier.Log)
            Log.Add(line);
        Log.Add($"transpal={transpal}");
        LastTranspal = transpal;
        return transpal;
    }

    public FilePic? CreateFromFile(byte[]? file, byte[]? ihdr, uint ihdrLen)
    {
        if (ihdrLen == 0)
        {
            Log.Add("ihdr missing");
            return null;
        }
        var reader = new Reader(ihdr, Log, "ihdr ");
        int width = reader.ReadI32Be();
        int height = reader.ReadI32Be();
        byte depth = reader.ReadU8(out bool sd);
        byte kind = reader.ReadU8(out bool sk);
        byte compression = reader.ReadU8(out bool sc);
        byte filter = reader.ReadU8(out bool sf);
        byte interlace = reader.ReadU8(out bool si);
        if (sd || sk || sc || sf || si)
            Log.Add("ihdr short");
        if (compression != 0 || filter != 0 || interlace > 0 || depth != 8 || (kind != 2 && kind != 3))
        {
            Log.Add($"reject c={compression} f={filter} i={interlace} d={depth} t={kind}");
            return null;
        }
        Log.Add($"file w={width} h={height} t={kind} masked=0 translucent=0");
        return new FilePic
        {
            Width = width,
            Height = height,
            ColorType = kind,
            File = file ?? Array.Empty<byte>(),
            Masked = false,
            Translucent = false,
        };
    }

    public VmBitmapCopy1241.Surface? GetBgra(FilePic? pic, byte[]? decoded, uint[]? remap, int[]? trans)
    {
        _ = remap;
        _ = trans;
        if (pic == null)
        {
            Log.Add("bgra null");
            return null;
        }
        int pixwidth = pic.Width * (pic.ColorType == 2 ? 3 : 1);
        var pe = new uint[256];
        var reader = Open(pic.File, "bgra ");
        reader.Seek(33, SeekSet);
        int ignored = 0;
        int size = 0;
        Walk(reader, pe, ref ignored, setSize: true, sizeSink: n => size = n);
        pic.PaletteSize = size;
        uint start = (uint)reader.Pos - 8u;
        Log.Add($"bgra idat at={start}");
        reader.Seek(start, SeekSet);
        uint len = 0;
        uint id = 0;
        reader.ReadRaw(ref len);
        reader.ReadRaw(ref id);
        byte[] raw = pixwidth <= 0 || pic.Height <= 0
            ? Array.Empty<byte>()
            : Take(decoded, pixwidth, pic.Height, 1, "bgra");
        Log.Add($"bgra idat raw={len:x8} be={Bswap(len)} id={Name(id)} n={raw.Length}");
        if (pic.Width <= 0 || pic.Height <= 0)
        {
            Log.Add("bgra bad");
            return null;
        }
        var copier = new VmBitmapCopy1241();
        var surface = copier.New(pic.Width, pic.Height, 0);
        if (pic.ColorType == 3)
            copier.CopyPal(surface, 0, 0, raw, 0, pic.Width, pic.Height, 1, pic.Width, 0, pe, null);
        else
            copier.CopyRgb(surface, 0, 0, raw, 0, pic.Width, pic.Height, 3, pixwidth, 0, VmBitmapCopy1241.Rgb, null, 0, 0, 0);
        foreach (string line in copier.Log)
            Log.Add(line);
        LastBitmap = surface;
        return surface;
    }

    void EnsurePalette()
    {
        if (PaletteMap != null)
            return;
        if (PaletteHost == null)
        {
            Log.Add("setup missing");
            return;
        }
        int before = PaletteHost.Log.Count;
        PaletteHost.SetupPalette(Lumps.TryGetValue(SourceLump, out var file) ? file : null);
        PaletteMap = PaletteHost.PaletteMap;
        int map10 = PaletteMap != null && PaletteMap.Length > 10 ? PaletteMap[10] : -1;
        Log.Add($"setup called lines={PaletteHost.Log.Count - before} map10={map10}");
    }

    void WalkRgb(byte[] temp, byte[] pixels, ref int i, ref int o, int pitch, int back, bool lum, bool alpha)
    {
        for (int x = Width; x > 0; --x)
        {
            for (int y = Height; y > 0; --y)
            {
                byte r = At(temp, i);
                byte g = At(temp, i + 1);
                byte b = At(temp, i + 2);
                byte a = alpha ? At(temp, i + 3) : (byte)255;
                if (!alpha && HaveTrans && r == NonPaletteTrans[0] && g == NonPaletteTrans[1] && b == NonPaletteTrans[2])
                    pixels[o++] = 0;
                else
                    pixels[o++] = (byte)ToPal(lum, r, g, b, a);
                i += pitch;
            }
            i -= back;
        }
    }

    void WalkGray(byte[] temp, byte[] pixels, ref int i, ref int o, int pitch, int back, bool lum)
    {
        bool missing = false;
        for (int x = Width; x > 0; --x)
        {
            for (int y = Height; y > 0; --y)
            {
                byte gray = At(temp, i);
                byte a = At(temp, i + 1);
                int v;
                if (lum)
                    v = gray * a / 255;
                else if (a < 128)
                    v = 0;
                else if (PaletteMap != null && gray < PaletteMap.Length)
                    v = PaletteMap[gray];
                else
                {
                    if (!missing)
                        Log.Add("map missing");
                    missing = true;
                    v = 0;
                }
                pixels[o++] = (byte)v;
                i += pitch;
            }
            i -= back;
        }
    }

    int ToPal(bool lum, int r, int g, int b, int a)
    {
        if (lum)
            return (Luma(r, g, b) * a) / 255;
        if (a < 128)
            return 0;
        if (Cube == null)
        {
            Log.Add("cube missing");
            return 0;
        }
        return Cube(r >> 2, g >> 2, b >> 2) & 255;
    }

    void Walk(Reader reader, uint[] pe, ref int transpal, bool setSize, Action<int>? sizeSink = null)
    {
        uint len = 0;
        uint id = 0;
        int nlen = reader.ReadRaw(ref len);
        int nid = reader.ReadRaw(ref id);
        if (nlen < 4 || nid < 4)
            reader.Note($"header short len={nlen} id={nid}");
        int guard = 0;
        int seen = PaletteSize;
        while (id != _idat && id != _iend)
        {
            if (++guard > 100000)
            {
                reader.Note("walk cap");
                break;
            }
            uint size = Bswap(len);
            reader.Note($"chunk {Name(id)} len={size}");
            if (id == _plte)
            {
                int count = setSize ? (int)Math.Min(size / 3u, 256u) : PaletteSize;
                if (setSize)
                    seen = count;
                for (int i = 0; i < count; i++)
                {
                    byte r = reader.ReadU8(out bool sr);
                    byte g = reader.ReadU8(out bool sg);
                    byte b = reader.ReadU8(out bool sb);
                    if (sr || sg || sb)
                        reader.Note($"plte short i={i}");
                    if (i < pe.Length)
                    {
                        pe[i] = WithRgb(pe[i], r, g, b);
                        if (setSize)
                            pe[i] = WithA(pe[i], 255);
                    }
                }
                if (setSize)
                    reader.Note($"plte size={count}");
            }
            else if (id == _trns && !setSize && ColorType == 3)
            {
                uint left = size;
                int idx = 0;
                while (left > 0)
                {
                    byte a = reader.ReadU8(out bool sh);
                    if (sh)
                        break;
                    if (idx < pe.Length)
                    {
                        pe[idx] = WithA(pe[idx], a);
                        if (a != 0 && a != 255)
                            transpal = 1;
                    }
                    idx++;
                    left--;
                }
                if (size > 256)
                    reader.Note("trns cap");
                reader.Note($"trns partial={transpal}");
            }
            else
                reader.Seek(size, SeekCur);
            reader.Seek(4, SeekCur);
            int c1 = reader.ReadRaw(ref len);
            id = _iend;
            int c2 = reader.ReadRaw(ref id);
            if (c1 < 4 || c2 < 4)
                reader.Note($"cont short len={c1} id={c2}");
        }
        reader.Note($"stop {Name(id)}");
        sizeSink?.Invoke(seen);
    }

    void LogIdat(Reader reader, int pixwidth, int decoded)
    {
        reader.Seek(StartOfIdat, SeekSet);
        uint len = 0;
        uint id = 0;
        int nlen = reader.ReadRaw(ref len);
        int nid = reader.ReadRaw(ref id);
        if (nlen < 4 || nid < 4)
            reader.Note($"idat short len={nlen} id={nid}");
        Log.Add($"idat raw={len:x8} be={Bswap(len)} id={Name(id)} pitch={pixwidth} decoded={decoded}");
    }

    byte[] Take(byte[]? decoded, int stride, int rows, int bpp, string tag)
    {
        if (stride < 0 || rows < 0)
            return Array.Empty<byte>();
        long want64 = (long)stride * rows * bpp;
        if (want64 > 1_048_576 || want64 < 0)
        {
            Log.Add($"{tag} bad n={want64}");
            return Array.Empty<byte>();
        }
        int want = (int)want64;
        var temp = new byte[want];
        int got = 0;
        if (decoded != null && want > 0)
        {
            got = Math.Min(decoded.Length, want);
            Buffer.BlockCopy(decoded, 0, temp, 0, got);
        }
        if (got != want)
            Log.Add($"{tag} short {got}/{want}");
        return temp;
    }

    int PixelWidth(int colorType, int width)
    {
        if (colorType < 0 || colorType >= Bpp.Length)
        {
            Log.Add($"bpp oob type={colorType}");
            return 0;
        }
        return width * Bpp[colorType];
    }

    Reader Open(byte[]? file, string prefix)
    {
        if (file == null)
        {
            Log.Add(prefix + "lump missing");
            file = Array.Empty<byte>();
        }
        return new Reader(file, Log, prefix);
    }

    static byte At(byte[] temp, int i) => i >= 0 && i < temp.Length ? temp[i] : (byte)0;

    static int NonNeg(int v) => v < 0 ? 0 : v;

    static int Luma(int r, int g, int b) => (r * 77 + g * 143 + b * 37) >> 8;

    static uint Pack(byte a, byte r, byte g, byte b) =>
        (uint)b | ((uint)g << 8) | ((uint)r << 16) | ((uint)a << 24);

    static uint WithA(uint p, byte a) => (p & 0x00FFFFFFu) | ((uint)a << 24);

    static uint WithRgb(uint p, byte r, byte g, byte b) =>
        (uint)b | ((uint)g << 8) | ((uint)r << 16) | (p & 0xFF000000u);

    static uint Bswap(uint v) =>
        (v >> 24) | ((v >> 8) & 0xFF00u) | ((v << 8) & 0x00FF0000u) | (v << 24);

    static uint MakeId(string s) =>
        (uint)s[0] | ((uint)s[1] << 8) | ((uint)s[2] << 16) | ((uint)s[3] << 24);

    static string Name(uint id)
    {
        Span<byte> b = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(b, id);
        for (int i = 0; i < 4; i++)
        {
            if (b[i] < 32 || b[i] > 126)
                return id.ToString("x8");
        }
        return Encoding.ASCII.GetString(b);
    }

    static string Hex(byte[] data)
    {
        int m = Math.Min(data.Length, 32);
        return Convert.ToHexString(data.AsSpan(0, m)).ToLowerInvariant();
    }

    public sealed class FilePic
    {
        public int Width;
        public int Height;
        public int ColorType;
        public int PaletteSize;
        public byte[] File = Array.Empty<byte>();
        public bool Masked;
        public bool Translucent;
    }

    sealed class Reader
    {
        readonly byte[] _data;
        readonly List<string> _log;
        readonly string _prefix;
        public int Pos;

        public Reader(byte[]? data, List<string> log, string prefix)
        {
            _data = data ?? Array.Empty<byte>();
            _log = log;
            _prefix = prefix;
        }

        public int Length => _data.Length;

        public void Note(string message) => _log.Add(_prefix + message);

        public bool Seek(long offset, int origin)
        {
            long target = origin switch
            {
                SeekSet => offset,
                SeekCur => Pos + offset,
                _ => _data.Length + offset,
            };
            if (target < 0 || target > _data.Length)
            {
                Note($"seek fail to={target}");
                return false;
            }
            Pos = (int)target;
            return true;
        }

        public int ReadRaw(ref uint value)
        {
            Span<byte> b = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(b, value);
            int n = 0;
            while (n < 4 && Pos < _data.Length)
            {
                b[n] = _data[Pos];
                Pos++;
                n++;
            }
            value = BinaryPrimitives.ReadUInt32LittleEndian(b);
            return n;
        }

        public int ReadI32Be()
        {
            uint v = 0;
            ReadRaw(ref v);
            return unchecked((int)Bswap(v));
        }

        public byte ReadU8(out bool shortRead)
        {
            if (Pos >= _data.Length)
            {
                shortRead = true;
                return 0;
            }
            shortRead = false;
            return _data[Pos++];
        }
    }
}
