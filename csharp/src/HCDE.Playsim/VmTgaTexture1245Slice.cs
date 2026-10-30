using System.Buffers.Binary;

namespace HCDE.Playsim;

/// <summary>
/// TGA <c>CopyPixels</c> from <c>tgatexture.cpp</c> (<b>368–518</b>). Chunk <b>1245</b>.
/// The RLE reader at 133–161 is already credited; this slice repeats it because
/// <c>CopyPixels</c> calls it and that method is private on the earlier port.
/// Bitmap expansion goes through the chunk 1241 copier.
/// </summary>
public sealed class VmTgaTexture1245
{
    public const int SeekSet = 0;
    public const int SeekCur = 1;

    public readonly List<string> Log = new();
    public int Width;
    public int Height;
    public byte Fill;
    public VmBitmapCopy1241.Surface? LastBitmap;
    public int LastTrans;

    public int CopyPixels(byte[]? file, int conversion, int frame)
    {
        _ = conversion;
        _ = frame;
        LastBitmap = null;
        LastTrans = 0;
        var reader = new Reader(file ?? Array.Empty<byte>(), Log);
        if (file == null)
            Log.Add("lump missing");
        var hdr = new byte[18];
        int got = reader.ReadInto(hdr, 0, 18);
        if (got < 18)
            Log.Add($"hdr short {got}/18");
        byte idLen = hdr[0];
        byte hasCm = hdr[1];
        byte imgType = hdr[2];
        short cmFirst = ReadI16(hdr, 3);
        short cmLength = ReadI16(hdr, 5);
        byte cmSize = hdr[7];
        short hdrW = ReadI16(hdr, 12);
        short hdrH = ReadI16(hdr, 14);
        byte bpp = hdr[16];
        byte desc = hdr[17];
        reader.Seek(idLen, SeekCur);
        Log.Add($"hdr type={imgType} bpp={bpp} cm={hasCm} first={cmFirst} len={cmLength} size={cmSize} desc={desc} id={idLen} hw={hdrW} hh={hdrH}");

        int trans = 0;
        var pe = new uint[256];
        if (hasCm != 0)
        {
            Log.Add("pe zero");
            int first = cmFirst;
            int last = first + cmLength;
            for (int i = first; i < last && i < 256; i++)
            {
                byte r, g, b, a;
                switch (cmSize)
                {
                    case 15:
                    case 16:
                        ushort w = reader.ReadU16(out bool sw);
                        if (sw)
                            Log.Add($"cm short i={i}");
                        r = (byte)((w & 0x001F) << 3);
                        g = (byte)((w & 0x03E0) >> 2);
                        b = (byte)((w & 0x7C00) >> 7);
                        a = 255;
                        break;
                    case 24:
                        b = reader.ReadU8(out bool sb);
                        g = reader.ReadU8(out bool sg);
                        r = reader.ReadU8(out bool sr);
                        if (sb || sg || sr)
                            Log.Add($"cm short i={i}");
                        a = 255;
                        break;
                    case 32:
                        b = reader.ReadU8(out bool sb32);
                        g = reader.ReadU8(out bool sg32);
                        r = reader.ReadU8(out bool sr32);
                        a = reader.ReadU8(out bool sa32);
                        if (sb32 || sg32 || sr32 || sa32)
                            Log.Add($"cm short i={i}");
                        if ((desc & 15) != 8)
                            a = 255;
                        else if (a != 0 && a != 255)
                            trans = 1;
                        break;
                    default:
                        r = g = b = a = 0;
                        Log.Add($"cm default i={i}");
                        break;
                }
                if (i < 0)
                {
                    Log.Add($"cm skip i={i}");
                    continue;
                }
                pe[i] = Pack(a, r, g, b);
            }
        }
        else
            Log.Add("pe uninit");

        int step = bpp >> 3;
        long want64 = (long)Width * Height * step;
        if (Width <= 0 || Height <= 0 || step <= 0 || want64 <= 0 || want64 > 1_048_576)
        {
            Log.Add($"copy skip dim w={Width} h={Height} step={step} n={want64} trans={trans}");
            LastTrans = trans;
            return trans;
        }
        int size = (int)want64;
        var pixels = new byte[size + 4];
        if (imgType < 4)
        {
            int n = reader.ReadInto(pixels, 0, size);
            Log.Add($"raw {n}/{size}");
        }
        else
            ReadCompressed(reader, pixels, step);

        int pitch = Width * step;
        int origin = 0;
        if ((desc & 32) == 0)
        {
            origin = (Height - 1) * pitch;
            pitch = -pitch;
        }
        Log.Add($"flip origin={origin} pitch={pitch} step={step}");
        if (!SpanOk(pixels, size, origin, pitch, step))
        {
            Log.Add($"span oob origin={origin} pitch={pitch} trans={trans}");
            LastTrans = trans;
            return trans;
        }

        var copier = new VmBitmapCopy1241();
        var surface = copier.New(Width, Height, Fill);
        int kind = imgType & 7;
        switch (kind)
        {
            case 1:
                copier.CopyPal(surface, 0, 0, pixels, origin, Width, Height, step, pitch, 0, pe, null);
                break;
            case 2:
                switch (bpp)
                {
                    case 15:
                    case 16:
                        copier.CopyRgb(surface, 0, 0, pixels, origin, Width, Height, step, pitch, 0, VmBitmapCopy1241.Rgb555, null, 0, 0, 0);
                        break;
                    case 24:
                        copier.CopyRgb(surface, 0, 0, pixels, origin, Width, Height, step, pitch, 0, VmBitmapCopy1241.Bgr, null, 0, 0, 0);
                        break;
                    case 32:
                        if ((desc & 15) != 8)
                            copier.CopyRgb(surface, 0, 0, pixels, origin, Width, Height, step, pitch, 0, VmBitmapCopy1241.Bgr, null, 0, 0, 0);
                        else
                        {
                            copier.CopyRgb(surface, 0, 0, pixels, origin, Width, Height, step, pitch, 0, VmBitmapCopy1241.Bgra, null, 0, 0, 0);
                            trans = -1;
                        }
                        break;
                    default:
                        Log.Add($"rgb skip bpp={bpp}");
                        break;
                }
                break;
            case 3:
                switch (bpp)
                {
                    case 8:
                        for (int i = 0; i < 256; i++)
                            pe[i] = Pack(255, (byte)i, (byte)i, (byte)i);
                        copier.CopyPal(surface, 0, 0, pixels, origin, Width, Height, step, pitch, 0, pe, null);
                        break;
                    case 16:
                        copier.CopyRgb(surface, 0, 0, pixels, origin, Width, Height, step, pitch, 0, VmBitmapCopy1241.I16, null, 0, 0, 0);
                        break;
                    default:
                        Log.Add($"gray skip bpp={bpp}");
                        break;
                }
                break;
            default:
                Log.Add($"copy skip type={imgType}");
                break;
        }
        LastBitmap = surface;
        foreach (string line in copier.Log)
            Log.Add(line);
        Log.Add($"trans={trans}");
        LastTrans = trans;
        return trans;
    }

    void ReadCompressed(Reader lump, byte[] buffer, int bpp)
    {
        int size = Width * Height;
        int at = 0;
        var data = new byte[4];
        int guard = 0;
        while (size > 0)
        {
            if (++guard > 1_000_000)
            {
                Log.Add("rle cap");
                break;
            }
            byte b = lump.ReadU8(out bool sh);
            if (sh)
            {
                Log.Add("rle short");
                break;
            }
            if ((b & 128) != 0)
            {
                b = (byte)(b & 127);
                int nread = lump.ReadInto(data, 0, bpp);
                if (nread < bpp)
                    Log.Add($"rle pix short {nread}/{bpp}");
                int n = Math.Min(size, b + 1);
                for (int i = n; i > 0; i--)
                {
                    if (at < buffer.Length)
                        buffer[at] = data[0];
                    if (bpp >= 2 && at + 1 < buffer.Length)
                        buffer[at + 1] = data[1];
                    if (bpp >= 3 && at + 2 < buffer.Length)
                        buffer[at + 2] = data[2];
                    if (bpp == 4 && at + 3 < buffer.Length)
                        buffer[at + 3] = data[3];
                    at += bpp;
                }
            }
            else
            {
                int n = Math.Min(size, b + 1);
                int want = n * bpp;
                int got = at >= 0 && at < buffer.Length ? lump.ReadInto(buffer, at, Math.Min(want, buffer.Length - at)) : 0;
                if (got < want)
                    Log.Add($"rle raw short {got}/{want}");
                at += (b + 1) * bpp;
            }
            size -= b + 1;
        }
        Log.Add($"rle left={size} at={at}");
    }

    bool SpanOk(byte[] pixels, int size, int origin, int pitch, int step)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int at = origin + y * pitch + x * step;
                if (at < 0 || at >= size)
                    return false;
            }
        }
        return pixels.Length >= size;
    }

    static uint Pack(byte a, byte r, byte g, byte b) =>
        (uint)b | ((uint)g << 8) | ((uint)r << 16) | ((uint)a << 24);

    static short ReadI16(byte[] buf, int at) =>
        (short)(buf[at] | (buf[at + 1] << 8));

    sealed class Reader
    {
        readonly byte[] _data;
        readonly List<string> _log;
        public int Pos;

        public Reader(byte[] data, List<string> log)
        {
            _data = data;
            _log = log;
        }

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
                _log.Add($"seek fail to={target}");
                return false;
            }
            Pos = (int)target;
            return true;
        }

        public int ReadInto(byte[] dest, int at, int count)
        {
            int n = 0;
            while (n < count && Pos < _data.Length && at + n < dest.Length)
            {
                dest[at + n] = _data[Pos];
                Pos++;
                n++;
            }
            return n;
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

        public ushort ReadU16(out bool shortRead)
        {
            byte lo = ReadU8(out bool s0);
            byte hi = ReadU8(out bool s1);
            shortRead = s0 || s1;
            return (ushort)(lo | (hi << 8));
        }
    }
}
