using System.Buffers.Binary;
using System.IO.Compression;

namespace HCDE.Playsim;

/// <summary>
/// <c>M_SaveBitmap</c> and <c>WriteIDAT</c> from <c>m_png.cpp</c> (<b>887–1104</b>).
/// Chunk <b>1247</b>. The filter heuristic is compiled out, so every row uses filter 0.
/// The IDAT bytes are produced by the host zlib wrapper. A test engine can stand in
/// for <c>deflate</c> so the full-buffer flush is observable below <c>PNG_WRITE_SIZE</c>.
/// </summary>
public sealed class VmPngSave1247
{
    public const int Pal = 0;
    public const int Rgb = 1;
    public const int Bgra = 2;
    public const int ZOk = 0;
    public const int ZStreamEnd = 1;
    public const int ZFinish = 4;
    public const int WriteSize = 32768;

    public readonly List<string> Log = new();
    public int Level = 5;
    public int ChunkBytes = WriteSize;
    public Func<Deflate>? Engine;

    public bool Save(OutFile? file, byte[]? from, int color, int width, int height, int pitch)
    {
        if (file == null)
        {
            Log.Add("file null");
            return false;
        }
        if (width < 0 || height < 0 || ChunkBytes < 1)
        {
            Log.Add($"dim bad w={width} h={height} buf={ChunkBytes}");
            return false;
        }

        Deflate engine = Engine != null ? Engine() : new HostDeflate();
        int err = engine.Init(Level);
        Log.Add($"init level={Level} err={err}");
        if (err != ZOk)
            return false;

        byte[] temprow = new byte[1 + width * 3];
        byte[] buffer = new byte[ChunkBytes];
        var io = new DeflateBuf { Output = buffer, AvailOut = buffer.Length };
        int y = height;
        int src = 0;
        while (true)
        {
            int before = y;
            y--;
            if (!(before > 0 && err == ZOk))
                break;
            if ((color == Pal || color == Rgb || color == Bgra) && from == null)
            {
                Log.Add("from null");
                return false;
            }
            if (color == Pal)
            {
                Buffer.BlockCopy(from!, src, temprow, 1, width);
                io.Input = temprow;
                io.InAt = 0;
                io.AvailIn = width + 1;
                Log.Add($"pack pal n={io.AvailIn} {Hex(temprow, io.AvailIn)}");
            }
            else if (color == Rgb)
            {
                Buffer.BlockCopy(from!, src, temprow, 1, width * 3);
                io.Input = temprow;
                io.InAt = 0;
                io.AvailIn = width * 3 + 1;
                Log.Add($"pack rgb n={io.AvailIn} {Hex(temprow, io.AvailIn)}");
            }
            else if (color == Bgra)
            {
                for (int x = 0; x < width; x++)
                {
                    temprow[x * 3 + 1] = from![src + x * 4 + 2];
                    temprow[x * 3 + 2] = from[src + x * 4 + 1];
                    temprow[x * 3 + 3] = from[src + x * 4];
                }
                io.Input = temprow;
                io.InAt = 0;
                io.AvailIn = width * 3 + 1;
                Log.Add($"pack bgra n={io.AvailIn} {Hex(temprow, io.AvailIn)}");
            }
            else
            {
                Log.Add($"pack skip type={color}");
            }
            if (from != null)
                src += pitch;

            int flush = y == 0 ? ZFinish : 0;
            err = engine.Process(ref io, flush);
            Log.Add($"deflate y={y} flush={flush} err={err} in={io.AvailIn} out={io.AvailOut}");
            if (err != ZOk)
                break;
            while (io.AvailOut == 0)
            {
                if (!WriteIdat(file, buffer, buffer.Length))
                {
                    Log.Add("idat abort");
                    return false;
                }
                io.OutAt = 0;
                io.AvailOut = buffer.Length;
                if (io.AvailIn != 0)
                {
                    err = engine.Process(ref io, flush);
                    Log.Add($"deflate y={y} flush={flush} err={err} in={io.AvailIn} out={io.AvailOut}");
                    if (err != ZOk)
                        break;
                }
            }
        }

        while (err == ZOk)
        {
            err = engine.Process(ref io, ZFinish);
            Log.Add($"deflate tail flush={ZFinish} err={err} in={io.AvailIn} out={io.AvailOut}");
            if (err != ZOk)
                break;
            if (io.AvailOut == 0)
            {
                if (!WriteIdat(file, buffer, buffer.Length))
                {
                    Log.Add("idat abort");
                    return false;
                }
                io.OutAt = 0;
                io.AvailOut = buffer.Length;
            }
        }

        engine.End();
        Log.Add("end");
        if (err != ZStreamEnd)
        {
            Log.Add($"stream bad err={err}");
            return false;
        }
        int sz = buffer.Length - io.AvailOut;
        return WriteIdat(file, buffer, sz);
    }

    bool WriteIdat(OutFile file, byte[] data, int len)
    {
        byte[] head = new byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(head, (uint)len);
        BinaryPrimitives.WriteUInt32LittleEndian(head.AsSpan(4), MakeId("IDAT"));
        uint crc = CrcAdd(Crc(head.AsSpan(4)), data.AsSpan(0, len));
        byte[] crcBe = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBe, crc);
        int n1 = file.Write(head);
        int n2 = n1 == 8 ? file.Write(data.AsSpan(0, len)) : 0;
        int n3 = n2 == len ? file.Write(crcBe) : 0;
        bool ok = n1 == 8 && n2 == len && n3 == 4;
        Log.Add($"idat len={len} ok={(ok ? 1 : 0)}");
        return ok;
    }

    static string Hex(byte[] data, int n)
    {
        if (n > 24)
            n = 24;
        return Convert.ToHexString(data.AsSpan(0, n)).ToLowerInvariant();
    }

    static uint MakeId(string s) =>
        (uint)s[0] | ((uint)s[1] << 8) | ((uint)s[2] << 16) | ((uint)s[3] << 24);

    static uint Crc(ReadOnlySpan<byte> data) => CrcAdd(0, data);

    static uint CrcAdd(uint crc, ReadOnlySpan<byte> data)
    {
        crc ^= 0xFFFFFFFFu;
        foreach (byte b in data)
            crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc ^ 0xFFFFFFFFu;
    }

    static readonly uint[] Table = BuildTable();

    static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint c = i;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? (c >> 1) ^ 0xEDB88320u : c >> 1;
            table[i] = c;
        }
        return table;
    }

    public struct DeflateBuf
    {
        public byte[]? Input;
        public int InAt;
        public int AvailIn;
        public byte[] Output;
        public int OutAt;
        public int AvailOut;
    }

    public abstract class Deflate
    {
        public abstract int Init(int level);
        public abstract int Process(ref DeflateBuf io, int flush);
        public abstract void End();
    }

    sealed class HostDeflate : Deflate
    {
        MemoryStream _store = new();
        ZLibStream? _z;
        int _taken;
        bool _finished;

        public override int Init(int level)
        {
            if (level < -1 || level > 9)
                return -2;
            CompressionLevel mode = level == 0
                ? CompressionLevel.NoCompression
                : level >= 9 ? CompressionLevel.SmallestSize : CompressionLevel.Optimal;
            _z = new ZLibStream(_store, mode, leaveOpen: true);
            return ZOk;
        }

        public override int Process(ref DeflateBuf io, int flush)
        {
            if (_z == null && !_finished)
                return -2;
            if (_z != null && io.AvailIn > 0 && io.Input != null)
            {
                _z.Write(io.Input, io.InAt, io.AvailIn);
                io.InAt += io.AvailIn;
                io.AvailIn = 0;
            }
            if (flush == ZFinish && !_finished && _z != null)
            {
                _z.Dispose();
                _z = null;
                _finished = true;
            }
            else if (_z != null)
            {
                _z.Flush();
            }
            int have = (int)_store.Length - _taken;
            int n = Math.Min(have, io.AvailOut);
            if (n > 0 && _store.TryGetBuffer(out ArraySegment<byte> seg))
            {
                Buffer.BlockCopy(seg.Array!, seg.Offset + _taken, io.Output, io.OutAt, n);
                _taken += n;
                io.OutAt += n;
                io.AvailOut -= n;
            }
            if (_finished && _taken >= _store.Length)
                return ZStreamEnd;
            return ZOk;
        }

        public override void End()
        {
            if (_z != null)
            {
                _z.Dispose();
                _z = null;
            }
            _finished = true;
        }
    }

    public sealed class OutFile
    {
        public readonly List<byte> Bytes = new();
        public int Limit = int.MaxValue;

        public int Write(ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
                return 0;
            if (Bytes.Count > Limit - data.Length)
                return 0;
            foreach (byte b in data)
                Bytes.Add(b);
            return data.Length;
        }
    }
}
