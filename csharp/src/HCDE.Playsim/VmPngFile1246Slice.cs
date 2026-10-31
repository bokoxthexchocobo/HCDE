using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// PNG signature, IHDR, gAMA, PLTE, chunk append, text, and verify from
/// <c>m_png.cpp</c> (<b>117–460</b>). Chunk <b>1246</b>. <c>M_SaveBitmap</c> is later
/// in the file, so the header writer records that call and does not deflate.
/// <c>MakeChunk</c> and <c>StuffPalette</c> are the helpers this band calls.
/// </summary>
public sealed class VmPngFile1246
{
    public const int Pal = 0;
    public const int Rgb = 1;
    public const int SeekSet = 0;
    public const int SeekCur = 1;

    public readonly List<string> Log = new();
    public float PngGamma;
    public bool SaveResult = true;
    public int SaveCalls;
    public int LastSaveWidth;
    public int LastSaveHeight;
    public int LastSavePitch;
    public int LastSaveType;
    public int LastSaveBytes;

    public bool Create(OutFile? file, uint[]? palette, int colorType, int width, int height, int pitch, float gamma, byte[]? pixels)
    {
        if (file == null)
        {
            Log.Add("file null");
            return false;
        }
        var work = new byte[829];
        WriteId(work, 0, 137, (byte)'P', (byte)'N', (byte)'G');
        WriteId(work, 4, 13, 10, 26, 10);
        WriteBe(work, 16, width);
        WriteBe(work, 20, height);
        work[24] = 8;
        bool pal = colorType == Pal;
        work[25] = (byte)(pal ? 3 : 2);
        MakeChunk(work, 16, MakeId("IHDR"), 13);
        int g = (int)(45454.5f * (PngGamma == 0f ? gamma : PngGamma));
        WriteBe(work, 41, g);
        MakeChunk(work, 41, MakeId("gAMA"), 4);
        Log.Add($"gama {g}");
        int len;
        if (pal)
        {
            Stuff(palette, work, 57);
            MakeChunk(work, 57, MakeId("PLTE"), 768);
            len = 829;
        }
        else
            len = 49;
        if (file.Write(work, 0, len) != len)
        {
            Log.Add($"write fail {len}");
            return false;
        }
        Log.Add($"png pal={(pal ? 1 : 0)} bytes={len} type={work[25]}");
        SaveCalls++;
        LastSaveWidth = width;
        LastSaveHeight = height;
        LastSavePitch = pitch;
        LastSaveType = colorType;
        LastSaveBytes = pixels?.Length ?? 0;
        Log.Add($"save w={width} h={height} pitch={pitch} type={colorType} n={LastSaveBytes} ok={(SaveResult ? 1 : 0)}");
        return SaveResult;
    }

    public bool CreateDummy(OutFile? file)
    {
        if (file == null)
        {
            Log.Add("file null");
            return false;
        }
        byte[] dummy =
        {
            137, (byte)'P', (byte)'N', (byte)'G', 13, 10, 26, 10,
            0, 0, 0, 13, (byte)'I', (byte)'H', (byte)'D', (byte)'R',
            0, 0, 0, 1, 0, 0, 0, 1, 8, 0, 0, 0, 0, 0x3a, 0x7e, 0x9b, 0x55,
            0, 0, 0, 10, (byte)'I', (byte)'D', (byte)'A', (byte)'T',
            104, 222, 99, 96, 0, 0, 0, 2, 0, 1, 0x9f, 0x65, 0x0e, 0x18,
        };
        bool ok = file.Write(dummy, 0, dummy.Length) == dummy.Length;
        Log.Add(ok ? "dummy ok" : "dummy fail");
        return ok;
    }

    public bool Finish(OutFile? file)
    {
        if (file == null)
        {
            Log.Add("file null");
            return false;
        }
        byte[] iend = { 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 };
        bool ok = file.Write(iend, 0, 12) == 12;
        Log.Add(ok ? "iend ok" : "iend fail");
        return ok;
    }

    public bool Append(OutFile? file, string id, byte[]? data)
    {
        if (file == null)
        {
            Log.Add("file null");
            return false;
        }
        uint len = (uint)(data?.Length ?? 0);
        var head = new byte[8];
        WriteBe(head, 0, unchecked((int)len));
        WriteId(head, 4, id);
        if (file.Write(head, 0, 8) != 8)
        {
            Log.Add("chunk head fail");
            return false;
        }
        if (len != 0 && file.Write(data!, 0, (int)len) != (int)len)
        {
            Log.Add("chunk data fail");
            return false;
        }
        uint crc = Crc(head.AsSpan(4, 4));
        if (len != 0)
            crc = CrcAdd(crc, data!);
        var tail = new byte[4];
        WriteBe(tail, 0, unchecked((int)crc));
        bool ok = file.Write(tail, 0, 4) == 4;
        Log.Add($"chunk {id} len={len} ok={(ok ? 1 : 0)}");
        return ok;
    }

    public bool AppendText(OutFile? file, string? keyword, string? text)
    {
        if (file == null)
        {
            Log.Add("file null");
            return false;
        }
        keyword ??= "";
        text ??= "";
        int textLen = text.Length;
        int keyLen = Math.Min(keyword.Length, 79);
        var head = new byte[88];
        WriteBe(head, 0, textLen + keyLen + 1);
        WriteId(head, 4, "tEXt");
        Encoding.ASCII.GetBytes(keyword, 0, keyLen, head, 8);
        head[8 + keyLen] = 0;
        if (file.Write(head, 0, keyLen + 9) != keyLen + 9)
        {
            Log.Add("text head fail");
            return false;
        }
        var raw = Encoding.ASCII.GetBytes(text);
        if (file.Write(raw, 0, textLen) != textLen)
        {
            Log.Add("text data fail");
            return false;
        }
        uint crc = Crc(head.AsSpan(4, keyLen + 5));
        if (textLen != 0)
            crc = CrcAdd(crc, raw);
        var tail = new byte[4];
        WriteBe(tail, 0, unchecked((int)crc));
        bool ok = file.Write(tail, 0, 4) == 4;
        Log.Add($"text key={keyLen} text={textLen} ok={(ok ? 1 : 0)}");
        return ok;
    }

    public uint Find(Handle? png, string id) 
    {
        if (png == null)
        {
            Log.Add("find null");
            return 0;
        }
        png.ChunkPt = 0;
        return Next(png, id);
    }

    public uint Next(Handle? png, string id)
    {
        if (png == null)
        {
            Log.Add("next null");
            return 0;
        }
        uint want = MakeId(id);
        while (png.ChunkPt < png.Chunks.Count)
        {
            Chunk chunk = png.Chunks[png.ChunkPt];
            if (chunk.Id == want)
            {
                png.Seek(chunk.Offset, SeekSet);
                png.ChunkPt++;
                Log.Add($"found {id} off={chunk.Offset} size={chunk.Size} pt={png.ChunkPt}");
                return chunk.Size;
            }
            png.ChunkPt++;
        }
        Log.Add($"miss {id} pt={png.ChunkPt}");
        return 0;
    }

    public string? GetText(Handle? png, string? keyword)
    {
        if (png == null || keyword == null)
        {
            Log.Add("gettext null");
            return null;
        }
        foreach (string raw in png.Texts)
        {
            if (!KeyEq(keyword, raw))
                continue;
            int keyLen = Math.Min(80, keyword.Length + 1);
            string text = raw.Length > keyLen ? raw[keyLen..] : "";
            Log.Add($"gettext {keyword} n={text.Length}");
            return text;
        }
        Log.Add($"gettext miss {keyword}");
        return null;
    }

    public bool GetText(Handle? png, string? keyword, byte[]? buffer)
    {
        if (png == null || keyword == null || buffer == null)
        {
            Log.Add("gettext null");
            return false;
        }
        foreach (string raw in png.Texts)
        {
            if (!KeyEq(keyword, raw))
                continue;
            int keyLen = Math.Min(80, keyword.Length + 1);
            string text = raw.Length > keyLen ? raw[keyLen..] : "";
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = i < text.Length ? (byte)text[i] : (byte)0;
            Log.Add($"gettext buf {keyword} n={buffer.Length} text={text.Length}");
            return true;
        }
        Log.Add($"gettext miss {keyword}");
        return false;
    }

    public Handle? Verify(byte[]? file)
    {
        var reader = new Handle { Bytes = file ?? Array.Empty<byte>() };
        if (file == null)
            Log.Add("verify missing");
        var data = new byte[8];
        if (reader.Read(data, 8) != 8)
        {
            Log.Add("verify sig short");
            return null;
        }
        if (!SigOk(data))
        {
            Log.Add("verify sig bad");
            return null;
        }
        if (reader.Read(data, 8) != 8)
        {
            Log.Add("verify ihdr short");
            return null;
        }
        uint id = ReadU32(data, 4);
        if (id != MakeId("IHDR"))
        {
            Log.Add($"verify ihdr id={Name(id)}");
            return null;
        }
        var chunk = new Chunk
        {
            Id = id,
            Offset = 16,
            Size = Bswap(ReadU32(data, 0)),
        };
        reader.Chunks.Add(chunk);
        reader.Seek(16, SeekSet);
        bool saw = false;
        int guard = 0;
        while (reader.Seek((long)chunk.Size + 4, SeekCur) == 0)
        {
            if (++guard > 100000)
            {
                Log.Add("verify cap");
                break;
            }
            if (reader.Read(data, 8) != 8)
            {
                Log.Add("verify end");
                break;
            }
            id = ReadU32(data, 4);
            if (id == MakeId("IEND"))
            {
                if (ReadU32(data, 0) == 0 && saw)
                {
                    Log.Add($"verify ok chunks={reader.Chunks.Count} texts={reader.Texts.Count}");
                    return reader;
                }
                Log.Add("verify iend bad");
                break;
            }
            if (id == MakeId("IDAT"))
                saw = true;
            chunk = new Chunk
            {
                Id = id,
                Offset = (uint)reader.Pos,
                Size = Bswap(ReadU32(data, 0)),
            };
            reader.Chunks.Add(chunk);
            Log.Add($"walk {Name(id)} off={chunk.Offset} size={chunk.Size}");
            if (id == MakeId("tEXt"))
            {
                if (chunk.Size > 1_048_576)
                {
                    Log.Add("text cap");
                    break;
                }
                var str = new byte[chunk.Size];
                if (reader.Read(str, (int)chunk.Size) != (int)chunk.Size)
                {
                    Log.Add("text short");
                    break;
                }
                reader.Texts.Add(Encoding.ASCII.GetString(str));
                chunk = new Chunk { Id = chunk.Id, Offset = chunk.Offset, Size = 0 };
            }
        }
        Log.Add("verify fail");
        return null;
    }

    public void Free(Handle? png)
    {
        if (png == null)
        {
            Log.Add("free null");
            return;
        }
        Log.Add($"free chunks={png.Chunks.Count} texts={png.Texts.Count}");
        png.Chunks.Clear();
        png.Texts.Clear();
        png.Bytes = Array.Empty<byte>();
    }

    void Stuff(uint[]? palette, byte[] dest, int at)
    {
        if (palette == null)
        {
            Log.Add("plte null");
            return;
        }
        if (palette.Length < 256)
            Log.Add($"plte short {palette.Length}");
        for (int i = 0; i < 256; i++)
        {
            uint pe = i < palette.Length ? palette[i] : 0;
            dest[at] = (byte)(pe >> 16);
            dest[at + 1] = (byte)(pe >> 8);
            dest[at + 2] = (byte)pe;
            at += 3;
        }
    }

    void MakeChunk(byte[] work, int at, uint type, int len)
    {
        WriteBe(work, at - 8, len);
        BinaryPrimitives.WriteUInt32LittleEndian(work.AsSpan(at - 4, 4), type);
        uint crc = Crc(work.AsSpan(at - 4, len + 4));
        WriteBe(work, at + len, unchecked((int)crc));
    }

    static bool SigOk(byte[] data) =>
        data[0] == 137 && data[1] == (byte)'P' && data[2] == (byte)'N' && data[3] == (byte)'G'
        && data[4] == 13 && data[5] == 10 && data[6] == 26 && data[7] == 10;

    static bool KeyEq(string keyword, string raw)
    {
        for (int i = 0; i < 80; i++)
        {
            char a = i < keyword.Length ? keyword[i] : '\0';
            char b = i < raw.Length ? raw[i] : '\0';
            if (a != b)
                return false;
            if (a == '\0')
                return true;
        }
        return true;
    }

    static void WriteId(byte[] buf, int at, byte a, byte b, byte c, byte d)
    {
        buf[at] = a;
        buf[at + 1] = b;
        buf[at + 2] = c;
        buf[at + 3] = d;
    }

    static void WriteId(byte[] buf, int at, string id) =>
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(at, 4), MakeId(id));

    static void WriteBe(byte[] buf, int at, int v) =>
        BinaryPrimitives.WriteUInt32BigEndian(buf.AsSpan(at, 4), unchecked((uint)v));

    static uint ReadU32(byte[] buf, int at) => BinaryPrimitives.ReadUInt32LittleEndian(buf.AsSpan(at, 4));

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

    public sealed class OutFile
    {
        public readonly List<byte> Bytes = new();
        public int Limit = int.MaxValue;

        public int Write(byte[] data, int at, int count)
        {
            if (count == 0)
                return 0;
            if (Bytes.Count > Limit - count)
                return 0;
            for (int i = 0; i < count; i++)
                Bytes.Add(data[at + i]);
            return count;
        }
    }

    public sealed class Chunk
    {
        public uint Id;
        public uint Offset;
        public uint Size;
    }

    public sealed class Handle
    {
        public byte[] Bytes = Array.Empty<byte>();
        public int Pos;
        public int ChunkPt;
        public readonly List<Chunk> Chunks = new();
        public readonly List<string> Texts = new();

        public int Seek(long offset, int origin)
        {
            long target = origin switch
            {
                SeekSet => offset,
                SeekCur => Pos + offset,
                _ => Bytes.Length + offset,
            };
            if (target < 0 || target > Bytes.Length)
                return -1;
            Pos = (int)target;
            return 0;
        }

        public int Read(byte[] dest, int count)
        {
            int n = 0;
            while (n < count && Pos < Bytes.Length)
            {
                dest[n] = Bytes[Pos];
                Pos++;
                n++;
            }
            return n;
        }
    }
}
