namespace HCDE.Playsim;

/// <summary>
/// IDAT chunk tail, <c>MakeChunk</c>, <c>StuffPalette</c>, <c>CalcSum</c>, and
/// <c>SelectFilter</c> from <c>m_png.cpp</c> lines 655-886.
/// The inflate loop through line 654 stays on the earlier PNG chunks.
/// <c>M_SaveBitmap</c> at line 887 stays chunk 1247.
/// </summary>
public sealed class VmPngTail1368Slice
{
    public const uint IdatType = 0x54414449;

    public uint ChunkLen { get; set; }
    public bool LastIdat { get; set; }
    public int BitDepth { get; set; } = 8;
    public int Interlace { get; set; }
    public int Pitch { get; set; }
    public int Prev { get; set; } = -1;
    public int Width { get; set; }
    public int BytesPerRowIn { get; set; }
    public int ColorType { get; set; }
    public bool UseFilterHeuristic { get; set; }

    public Func<byte[], int>? Read { get; set; }
    public Action? InflateEnd { get; set; }
    public Action<int, int, int, int, bool>? Unpack { get; set; }
    public Func<byte[], int, int, uint>? Crc { get; set; }
    public List<string> Trace { get; } = [];

    public void ContinueIdat()
    {
        if (ChunkLen != 0 || LastIdat)
            return;
        if (Read == null)
            throw new InvalidOperationException("Read");
        var buf = new byte[12];
        int count = Read(buf);
        Trace.Add("Read");
        if (count != 12)
        {
            LastIdat = true;
            return;
        }

        uint type = ReadU32(buf, 8);
        if (type != IdatType)
        {
            LastIdat = true;
            return;
        }

        ChunkLen = Swap(ReadU32(buf, 4));
    }

    public bool FinishIdat()
    {
        if (InflateEnd == null)
            throw new InvalidOperationException("inflateEnd");
        InflateEnd();
        Trace.Add("inflateEnd");
        if (BitDepth < 8)
        {
            if (Interlace is < 0 or >= 31)
                throw new InvalidOperationException("interlace");
            int passpitch = Pitch << Interlace;
            int curr = Pitch * Interlace;
            if (passpitch == 0 && curr <= Prev)
                throw new InvalidOperationException("passpitch");
            while (curr <= Prev)
            {
                if (Unpack == null)
                    throw new InvalidOperationException("UnpackPixels");
                Unpack(Width, BytesPerRowIn, BitDepth, curr, ColorType == 0);
                Trace.Add("UnpackPixels");
                curr += passpitch;
            }
        }

        return true;
    }

    public void MakeChunk(byte[] storage, int data, uint type, int len)
    {
        if (storage == null)
            throw new InvalidOperationException("where");
        if (len < 0 || data < 8 || data > storage.Length - 4 || data + len > storage.Length - 4)
            throw new IndexOutOfRangeException("len");
        WriteU32(storage, data - 8, Swap((uint)len));
        WriteU32(storage, data - 4, type);
        if (Crc == null)
            throw new InvalidOperationException("CalcCRC32");
        uint crc = Crc(storage, data - 4, len + 4);
        Trace.Add("CalcCRC32");
        WriteU32(storage, data + len, Swap(crc));
    }

    public static void StuffPalette(byte[] from, byte[] to)
    {
        if (from == null)
            throw new InvalidOperationException("from");
        if (to == null)
            throw new InvalidOperationException("to");
        int src = 0;
        int dst = 0;
        for (int i = 256; i > 0; --i)
        {
            if (src + 2 >= from.Length)
                throw new IndexOutOfRangeException("from");
            if (dst + 2 >= to.Length)
                throw new IndexOutOfRangeException("to");
            to[dst] = from[src];
            to[dst + 1] = from[src + 1];
            to[dst + 2] = from[src + 2];
            src += 4;
            dst += 3;
        }
    }

    public static uint CalcSum(byte[] row, int len)
    {
        if (row == null)
            throw new InvalidOperationException("row");
        if (len < 0)
            throw new InvalidOperationException("len");
        uint sum = 0;
        int index = 0;
        while (len-- != 0)
        {
            if (index >= row.Length)
                throw new IndexOutOfRangeException("row");
            sum = unchecked(sum + (uint)(int)unchecked((sbyte)row[index]));
            index++;
        }

        return sum;
    }

    public int SelectFilter(byte[][]? row, byte[]? prior, int width)
    {
        _ = row;
        _ = prior;
        _ = width;
        _ = UseFilterHeuristic;
        return 0;
    }

    static uint ReadU32(byte[] buf, int at) =>
        (uint)(buf[at] | (buf[at + 1] << 8) | (buf[at + 2] << 16) | (buf[at + 3] << 24));

    static void WriteU32(byte[] buf, int at, uint value)
    {
        buf[at] = (byte)value;
        buf[at + 1] = (byte)(value >> 8);
        buf[at + 2] = (byte)(value >> 16);
        buf[at + 3] = (byte)(value >> 24);
    }

    static uint Swap(uint value) =>
        (value >> 24) | ((value >> 8) & 0xFF00) | ((value << 8) & 0xFF0000) | (value << 24);
}
