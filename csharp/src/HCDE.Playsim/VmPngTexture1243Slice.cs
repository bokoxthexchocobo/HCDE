using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Pre-IDAT chunk walk, palette setup, and alpha remap from <c>pngtexture.cpp</c>
/// (<b>177–423</b>). Chunk <b>1243</b>. The gray and palette IDAT decode that starts at
/// the next line stays chunk 106. <c>MakeRemap</c>, <c>BestColor</c>, and
/// <c>Luminance</c> live outside this file; setup and the alpha walk call them.
/// </summary>
public sealed class VmPngTexture1243
{
    public const int SeekSet = 0;
    public const int SeekCur = 1;
    public const int Luminance = 1;

    public readonly List<string> Log = new();
    public readonly Dictionary<int, byte[]?> Lumps = new();
    public readonly byte[] GrayMap;
    public readonly uint[] BaseColors;
    public readonly ushort[] NonPaletteTrans = new ushort[3];

    public int SourceLump;
    public int Width;
    public int Height;
    public int BitDepth;
    public int ColorType;
    public int Interlace;
    public int PaletteSize;
    public int LeftOffset;
    public int TopOffset;
    public bool HaveTrans;
    public bool Masked;
    public bool PaletteShared;
    public byte[]? PaletteMap;
    public uint StartOfIdat;
    public uint StartOfPalette;

    readonly uint _idat = MakeId("IDAT");
    readonly uint _iend = MakeId("IEND");
    readonly uint _grab = MakeId("grAb");
    readonly uint _plte = MakeId("PLTE");
    readonly uint _trns = MakeId("tRNS");

    public VmPngTexture1243()
    {
        GrayMap = new byte[256];
        BaseColors = new uint[256];
        for (int i = 0; i < 256; i++)
            GrayMap[i] = (byte)i;
    }

    public void Construct(byte[]? data, int lumpnum, int width, int height, int depth, int colorType, int interlace)
    {
        SourceLump = lumpnum;
        Width = width;
        Height = height;
        BitDepth = depth;
        ColorType = colorType;
        Interlace = interlace;
        HaveTrans = false;
        Masked = false;
        PaletteShared = false;
        PaletteMap = null;
        PaletteSize = 0;
        StartOfPalette = 0;
        StartOfIdat = 0;
        LeftOffset = 0;
        TopOffset = 0;
        NonPaletteTrans[0] = NonPaletteTrans[1] = NonPaletteTrans[2] = 0;

        var trans = new byte[256];
        Array.Fill(trans, (byte)255);
        var reader = new Reader(data, Log, "");
        Walk(reader, trans, null, setup: false, out _);
        StartOfIdat = (uint)reader.Pos - 8u;
        Log.Add($"idat at={StartOfIdat} tell={reader.Pos}");

        string gray = "none";
        switch (colorType)
        {
            case 4:
                Masked = true;
                goto case 0;
            case 0:
                if (colorType == 0 && HaveTrans && NonPaletteTrans[0] < 256)
                {
                    Masked = true;
                    PaletteSize = 256;
                    gray = "defer";
                }
                else
                {
                    PaletteMap = GrayMap;
                    PaletteShared = true;
                    gray = "shared";
                }
                break;
            case 3:
                for (int i = 0; i < PaletteSize; i++)
                {
                    if (trans[i] == 0)
                    {
                        Masked = true;
                        Log.Add($"mask idx={i}");
                    }
                }
                break;
            case 6:
                Masked = true;
                break;
            case 2:
                Masked = HaveTrans;
                break;
        }

        string npt = HaveTrans
            ? $"{NonPaletteTrans[0]},{NonPaletteTrans[1]},{NonPaletteTrans[2]}"
            : "unset";
        Log.Add(
            $"done mask={(Masked ? 1 : 0)} have={(HaveTrans ? 1 : 0)} pal={PaletteSize} idat={StartOfIdat} left={LeftOffset} top={TopOffset} npt={npt} gray={gray} ref={(PaletteShared ? 1 : 0)} depth={BitDepth} type={ColorType} ilace={Interlace} w={Width} h={Height} lump={SourceLump}");
    }

    public void SetupPalette(byte[]? data, int at = 0)
    {
        var reader = new Reader(data, Log, "setup ");
        if (at != 0)
            reader.Seek(at, SeekSet);
        int saved = reader.Pos;
        var trans = new byte[256];
        Array.Fill(trans, (byte)255);
        var colors = new uint[256];
        Walk(reader, trans, colors, setup: true, out bool sawPlte);
        StartOfIdat = (uint)reader.Pos - 8u;
        Log.Add($"setup idat at={StartOfIdat} tell={reader.Pos}");

        switch (ColorType)
        {
            case 0:
                if (HaveTrans && NonPaletteTrans[0] < 256)
                    InstallGrayCopy();
                break;
            case 3:
                if (!sawPlte)
                    Log.Add("setup plte absent");
                PaletteMap = new byte[Math.Max(PaletteSize, 0)];
                PaletteShared = false;
                MakeRemap(BaseColors, colors, PaletteMap, trans, PaletteSize);
                for (int i = 0; i < PaletteSize; i++)
                {
                    if (trans[i] == 0)
                    {
                        PaletteMap[i] = 0;
                        Log.Add($"setup force0 {i}");
                    }
                }
                if (PaletteSize > 0 && PaletteSize <= 32 && PaletteMap != null)
                    Log.Add("setup map=" + Hex(PaletteMap, PaletteSize));
                break;
        }

        reader.Seek(saved, SeekSet);
        Log.Add($"setup back={reader.Pos}");
    }

    public void ReadAlphaRemap(byte[]? data, byte[]? alpharemap, int at = 0)
    {
        if (alpharemap == null)
            Log.Add("alpha buf null");
        var reader = new Reader(data, Log, "alpha ");
        if (at != 0)
            reader.Seek(at, SeekSet);
        int saved = reader.Pos;
        reader.Seek(StartOfPalette, SeekSet);
        for (int i = 0; i < PaletteSize; i++)
        {
            byte r = reader.ReadU8(out bool sr);
            byte g = reader.ReadU8(out bool sg);
            byte b = reader.ReadU8(out bool sb);
            if (sr || sg || sb)
                Log.Add($"alpha short i={i}");
            int palmap;
            if (PaletteMap == null)
                palmap = i;
            else if (i >= PaletteMap.Length)
            {
                Log.Add($"alpha map oob i={i}");
                palmap = 0;
            }
            else
                palmap = PaletteMap[i];
            int lum = Luma(r, g, b);
            int outv = palmap == 0 ? 0 : lum;
            if (alpharemap != null && i < alpharemap.Length)
                alpharemap[i] = (byte)outv;
            else if (alpharemap != null)
                Log.Add($"alpha buf oob i={i}");
            Log.Add($"alpha i={i} map={palmap} out={outv}");
        }
        reader.Seek(saved, SeekSet);
        Log.Add($"alpha back={reader.Pos}");
    }

    public byte[] CreatePalettedPixels(int conversion, int frame)
    {
        _ = frame;
        byte[] file;
        if (!Lumps.TryGetValue(SourceLump, out var found) || found == null)
        {
            Log.Add("lump missing");
            file = Array.Empty<byte>();
        }
        else
            file = found;

        long n64 = (long)Width * Height;
        if (Width < 0 || Height < 0 || n64 > 1_048_576)
        {
            Log.Add($"pix bad n={n64}");
            n64 = 0;
        }
        int n = (int)n64;
        var pixels = new byte[n];
        if (StartOfIdat == 0)
        {
            Array.Fill(pixels, (byte)0x99);
            Log.Add($"pix n={n} fill=99");
            return pixels;
        }

        var reader = new Reader(file, Log, "pix ");
        reader.Seek(StartOfIdat, SeekSet);
        uint len = 0;
        uint id = 0;
        int nlen = reader.ReadRaw(ref len);
        int nid = reader.ReadRaw(ref id);
        if (nlen < 4 || nid < 4)
            reader.Note($"header short len={nlen} id={nid}");
        int alpha = conversion == Luminance ? 1 : 0;
        reader.Note($"len={len:x8} id={Name(id)} alpha={alpha} n={n} undecoded=1");
        return pixels;
    }

    void InstallGrayCopy()
    {
        var map = new byte[256];
        int copy = Math.Min(256, GrayMap.Length);
        if (copy > 0)
            Buffer.BlockCopy(GrayMap, 0, map, 0, copy);
        if (copy < 256)
            Log.Add($"setup gray short {copy}");
        int idx = NonPaletteTrans[0];
        if (idx >= 0 && idx < map.Length)
            map[idx] = 0;
        PaletteMap = map;
        PaletteShared = false;
        int shared = idx >= 0 && idx < GrayMap.Length ? GrayMap[idx] : -1;
        if (PaletteSize != 256)
            Log.Add($"setup gray alloc {PaletteSize}");
        Log.Add($"setup gray copy zero={idx} copy={map[idx]} shared={shared} ref=0");
    }

    void Walk(Reader reader, byte[] trans, uint[]? colors, bool setup, out bool sawPlte)
    {
        sawPlte = false;
        reader.Seek(33, SeekSet);
        uint len = 0;
        uint id = 0;
        int nlen = reader.ReadRaw(ref len);
        int nid = reader.ReadRaw(ref id);
        if (nlen < 4 || nid < 4)
            reader.Note($"header short len={nlen} id={nid}");
        int guard = 0;
        while (id != _idat && id != _iend)
        {
            if (++guard > 100000)
            {
                reader.Note("walk cap");
                break;
            }
            uint size = Bswap(len);
            reader.Note($"chunk {Name(id)} len={size}");
            if (id == _grab && !setup)
                ReadGrab(reader);
            else if (id == _plte && !setup)
            {
                PaletteSize = (int)Math.Min(size / 3u, 256u);
                StartOfPalette = (uint)reader.Pos;
                reader.Note($"plte size={PaletteSize} at={StartOfPalette}");
                reader.Seek(size, SeekCur);
            }
            else if (id == _plte && setup)
            {
                sawPlte = true;
                ReadPlte(reader, colors, size);
            }
            else if (id == _trns)
                ReadTrans(reader, trans, size, setup);
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
    }

    void ReadGrab(Reader reader)
    {
        int x = reader.ReadI32Be();
        int y = reader.ReadI32Be();
        int sx = x;
        int sy = y;
        if (x < -32768 || x > 32767)
        {
            reader.Note($"grab bad x={x}");
            sx = 0;
        }
        if (y < -32768 || y > 32767)
        {
            reader.Note($"grab bad y={y}");
            sy = 0;
        }
        LeftOffset = sx;
        TopOffset = sy;
        reader.Note($"grab x={sx} y={sy}");
    }

    void ReadPlte(Reader reader, uint[]? colors, uint size)
    {
        int want = PaletteSize * 3;
        var raw = new byte[Math.Max(want, 0)];
        int got = want > 0 ? reader.Read(raw, 0, want) : 0;
        if (got != want)
            reader.Note($"plte short {got}/{want}");
        int lenAsInt = unchecked((int)size);
        if (want != lenAsInt)
        {
            uint delta = unchecked((uint)size - (uint)want);
            reader.Seek(delta, SeekCur);
        }
        if (colors == null)
            return;
        for (int i = PaletteSize - 1; i >= 0; --i)
        {
            if (i >= colors.Length)
                continue;
            int o = i * 3;
            byte r = o < got ? raw[o] : (byte)0;
            byte g = o + 1 < got ? raw[o + 1] : (byte)0;
            byte b = o + 2 < got ? raw[o + 2] : (byte)0;
            colors[i] = ((uint)r << 16) | ((uint)g << 8) | b;
        }
    }

    void ReadTrans(Reader reader, byte[] trans, uint size, bool setup)
    {
        uint left = size;
        int stored = 0;
        var scratch = new byte[4096];
        while (left > 0)
        {
            int chunk = (int)Math.Min(left, (uint)scratch.Length);
            int got = reader.Read(scratch, 0, chunk);
            if (got <= 0)
                break;
            int room = trans.Length - stored;
            if (room > 0)
            {
                int n = Math.Min(room, got);
                Buffer.BlockCopy(scratch, 0, trans, stored, n);
                stored += n;
            }
            left -= (uint)got;
        }
        if (size > trans.Length)
            reader.Note("trns cap");
        if (setup)
            return;
        HaveTrans = true;
        NonPaletteTrans[0] = (ushort)(trans[0] * 256 + trans[1]);
        NonPaletteTrans[1] = (ushort)(trans[2] * 256 + trans[3]);
        NonPaletteTrans[2] = (ushort)(trans[4] * 256 + trans[5]);
        reader.Note($"trns npt={NonPaletteTrans[0]},{NonPaletteTrans[1]},{NonPaletteTrans[2]}");
    }

    void MakeRemap(uint[]? baseColors, uint[]? colors, byte[] remap, byte[]? useful, int numcolors)
    {
        if (baseColors == null || baseColors.Length < 256)
            Log.Add("setup base short");
        var work = new Work[255 + 256];
        for (int i = 1; i < 256; i++)
        {
            uint c = baseColors != null && i < baseColors.Length ? baseColors[i] : 0u;
            work[i - 1].Color = c & 0xFFFFFFu;
            work[i - 1].Foreign = 0;
            work[i - 1].PalEntry = (byte)i;
        }
        int j = 255;
        int k = 0;
        for (int i = 0; i < numcolors; i++)
        {
            bool keep = useful == null || i >= useful.Length || useful[i] != 0;
            if (useful != null && i >= useful.Length)
                Log.Add($"setup useful short {i}");
            if (keep)
            {
                if (j >= work.Length)
                {
                    Log.Add("setup remap cap");
                    break;
                }
                uint c = colors != null && i < colors.Length ? colors[i] : 0u;
                work[j].Color = c & 0xFFFFFFu;
                work[j].Foreign = 1;
                work[j].PalEntry = (byte)i;
                j++;
                k++;
            }
            else if (i < remap.Length)
                remap[i] = 0;
            else
                Log.Add($"setup remap oob {i}");
        }

        int count = j;
        int[] order = Enumerable.Range(0, count)
            .OrderBy(i => work[i].Color)
            .ThenByDescending(i => work[i].Foreign)
            .ToArray();
        var sorted = new Work[count];
        for (int i = 0; i < count; i++)
            sorted[i] = work[order[i]];

        int last = count - 1;
        for (int i = 0; i < last; i++)
        {
            if (sorted[i].Foreign == 0)
                continue;
            if (sorted[i + 1].Foreign == 0 && sorted[i].Color == sorted[i + 1].Color)
            {
                int src = sorted[i].PalEntry;
                byte dest = sorted[i + 1].PalEntry;
                if (src < remap.Length)
                    remap[src] = dest;
                Log.Add($"setup remap exact {src}->{dest}");
                sorted[i].Foreign = 2;
                i++;
                k--;
            }
        }

        if (k <= 0)
            return;
        for (int i = 0; i <= last; i++)
        {
            if (sorted[i].Foreign != 1)
                continue;
            uint c = sorted[i].Color;
            int rr = (int)((c >> 16) & 255);
            int gg = (int)((c >> 8) & 255);
            int bb = (int)(c & 255);
            int dest = BestColor(baseColors, rr, gg, bb);
            int src = sorted[i].PalEntry;
            if (src < remap.Length)
                remap[src] = (byte)dest;
            Log.Add($"setup remap near {src}->{dest}");
        }
    }

    static int BestColor(uint[]? pal, int r, int g, int b)
    {
        int best = 1;
        int bestdist = 257 * 257 + 257 * 257 + 257 * 257;
        int limit = Math.Min(255, pal?.Length ?? 0);
        for (int color = 1; color < limit; color++)
        {
            uint entry = pal![color];
            int x = r - (int)((entry >> 16) & 255);
            int y = g - (int)((entry >> 8) & 255);
            int z = b - (int)(entry & 255);
            int dist = x * x + y * y + z * z;
            if (dist < bestdist)
            {
                if (dist == 0)
                    return color;
                bestdist = dist;
                best = color;
            }
        }
        return best;
    }

    static int Luma(int r, int g, int b) => (r * 77 + g * 143 + b * 37) >> 8;

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

    static string Hex(byte[] data, int n)
    {
        int m = Math.Min(n, data.Length);
        return Convert.ToHexString(data.AsSpan(0, m)).ToLowerInvariant();
    }

    struct Work
    {
        public uint Color;
        public byte Foreign;
        public byte PalEntry;
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

        public int Read(byte[] dest, int destOff, int len)
        {
            if (len < 0)
                len = 0;
            int avail = _data.Length - Pos;
            if (avail < 0)
                avail = 0;
            if (len > avail)
                len = avail;
            if (len > 0)
                Buffer.BlockCopy(_data, Pos, dest, destOff, len);
            Pos += len;
            return len;
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
