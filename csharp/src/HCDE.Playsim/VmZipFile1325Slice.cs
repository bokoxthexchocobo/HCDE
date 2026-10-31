using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Central directory, compression methods, and the local-header skip from
/// <c>file_zip.cpp</c> (1-388). Chunk 1325.
/// </summary>
public sealed class VmZipFile1325
{
    public const int BufReadComment = 0x400;
    public const int MethodStored = 0;
    public const int MethodShrink = 1;
    public const int MethodImplode = 6;
    public const int MethodDeflate = 8;
    public const int MethodBzip2 = 12;
    public const int MethodLzma = 14;
    public const int MethodXz = 95;
    public const int MethodImplode0 = 1000;
    public const int MethodImplode2 = 1002;
    public const int MethodImplode4 = 1004;
    public const int MethodImplode6 = 1006;
    public const int ResFullPath = 2;
    public const int ResCompressed = 16;
    public const int ResNeedFileStart = 32;
    public const int Encrypted = 1;
    public const int LevelError = 1;
    public const int LevelWarning = 2;
    public const int CentralHeader = 46;
    public const int LocalHeader = 30;

    public static int DestroyCount { get; private set; }

    static readonly Encoding Latin1 = Encoding.GetEncoding(28591);

    public readonly record struct Message(int Level, string Text);

    public sealed class Reader
    {
        public Reader(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; }
        public int Position { get; private set; }
        public long Length => Data.LongLength;

        public int Seek(long pos, int origin)
        {
            if (origin != 0 || pos < 0 || pos > Data.Length)
            {
                return -1;
            }

            Position = (int)pos;
            return 0;
        }

        public int Read(byte[] dest, int count)
        {
            int n = Math.Max(0, Math.Min(count, Data.Length - Position));
            Buffer.BlockCopy(Data, Position, dest, 0, n);
            Position += n;
            return n;
        }
    }

    public sealed class Entry
    {
        public string FileName = "";
        public uint Length;
        public uint CompressedSize;
        public ushort Flags;
        public ushort Method;
        public uint Crc32;
        public ulong Position;
    }

    public sealed class Compressed
    {
        public ulong Size;
        public ulong CompressedSize;
        public int Method;
        public uint Crc32;
        public byte[]? Buffer;
    }

    public sealed class Archive
    {
        public Archive(string fileName, Reader reader)
        {
            FileName = fileName;
            Reader = reader;
        }

        public string FileName { get; }
        public Reader Reader { get; }
        public List<Entry> Entries { get; } = new();
        public List<Message> Messages { get; } = new();
        public int NumLumps { get; private set; }
        public bool HashGenerated { get; private set; }
        public bool PostProcessed { get; private set; }
        public object? PostedFilter { get; private set; }
        public bool Destroyed { get; private set; }

        public bool Open(object? filter)
        {
            bool zip64 = false;
            uint central = FindCentralDir(Reader, ref zip64);
            if (central == 0)
            {
                Say(LevelError, $"{FileName}: ZIP file corrupt!\n");
                return false;
            }

            ulong dirSize;
            ulong dirOffset;
            uint lumps;
            if (!zip64)
            {
                var info = new byte[22];
                if (Reader.Seek(central, 0) != 0 || Reader.Read(info, 22) != 22)
                {
                    Say(LevelError, $"{FileName}: ZIP file corrupt!\n");
                    return false;
                }

                ushort disk = ReadU16(info, 4);
                ushort first = ReadU16(info, 6);
                ushort num = ReadU16(info, 8);
                ushort numAll = ReadU16(info, 10);
                if (num != numAll || first != 0 || disk != 0)
                {
                    Say(LevelError, $"{FileName}: Multipart Zip files are not supported.\n");
                    return false;
                }

                lumps = num;
                dirSize = ReadU32(info, 12);
                dirOffset = ReadU32(info, 16);
            }
            else
            {
                var info = new byte[58];
                if (Reader.Seek(central, 0) != 0 || Reader.Read(info, 58) != 58)
                {
                    Say(LevelError, $"{FileName}: ZIP file corrupt!\n");
                    return false;
                }

                uint disk = ReadU32(info, 16);
                uint first = ReadU32(info, 20);
                ulong num = ReadU64(info, 24);
                ulong numAll = ReadU64(info, 32);
                if (num != numAll || first != 0 || disk != 0)
                {
                    Say(LevelError, $"{FileName}: Multipart Zip files are not supported.\n");
                    return false;
                }

                lumps = (uint)num;
                dirSize = ReadU64(info, 40);
                dirOffset = ReadU64(info, 48);
            }

            if (dirSize > int.MaxValue || Reader.Seek((long)dirOffset, 0) != 0)
            {
                Say(LevelError, $"{FileName}: Central directory corrupted.");
                return false;
            }

            var directory = new byte[(int)dirSize];
            Reader.Read(directory, directory.Length);
            if (lumps > 1)
            {
                int cursor = 0;
                for (uint i = 0; i < lumps; i++)
                {
                    if (!Step(directory, ref cursor))
                    {
                        Say(LevelError, $"{FileName}: Central directory corrupted.");
                        return false;
                    }
                }
            }

            NumLumps = (int)lumps;
            int skipped = 0;
            int p = 0;
            for (uint i = 0; i < lumps; i++)
            {
                int start = p;
                if (!TryHeader(directory, start, out Header zip))
                {
                    Say(LevelError, $"{FileName}: Central directory corrupted.");
                    return false;
                }

                int nameAt = start + CentralHeader;
                int end = nameAt + zip.NameLen + zip.ExtraLen + zip.CommentLen;
                if (end > directory.Length)
                {
                    Say(LevelError, $"{FileName}: Central directory corrupted.");
                    return false;
                }

                byte[] raw = directory.AsSpan(nameAt, zip.NameLen).ToArray();
                string name = Latin1.GetString(raw);
                p = end;
                if (name.Length == 0 || (name[^1] == '/' && zip.Unc32 == 0))
                {
                    skipped++;
                    continue;
                }

                if (zip.Method != MethodStored &&
                    zip.Method != MethodDeflate &&
                    zip.Method != MethodLzma &&
                    zip.Method != MethodBzip2 &&
                    zip.Method != MethodImplode &&
                    zip.Method != MethodShrink &&
                    zip.Method != MethodXz)
                {
                    Say(LevelError, $"{FileName}: '{name}' uses an unsupported compression algorithm (#{zip.Method}).\n");
                    skipped++;
                    continue;
                }

                if ((zip.Flags & Encrypted) != 0)
                {
                    Say(LevelError, $"{FileName}: '{name}' is encrypted. Encryption is not supported.\n");
                    skipped++;
                    continue;
                }

                uint unc = zip.Unc32;
                uint comp = zip.Comp32;
                ulong local = zip.Local32;
                if (zip.ExtraLen > 0)
                {
                    int rawExt = nameAt + zip.NameLen;
                    uint extra = (uint)zip.ExtraLen;
                    while (extra > 0)
                    {
                        if (rawExt + 4 > directory.Length)
                        {
                            break;
                        }

                        ushort type = ReadU16(directory, rawExt);
                        ushort block = ReadU16(directory, rawExt + 2);
                        if ((uint)block + 4 > extra)
                        {
                            break;
                        }

                        int blockAt = rawExt;
                        rawExt += block + 4;
                        extra -= (uint)block + 4;
                        if (type == 1 && block >= 0x18)
                        {
                            ulong zunc = ReadU64(directory, blockAt + 4);
                            ulong zcomp = ReadU64(directory, blockAt + 12);
                            ulong zlocal = ReadU64(directory, blockAt + 20);
                            if (zunc > 0x7fffffff || zcomp > 0x7fffffff)
                            {
                                Say(LevelWarning, $"{FileName}: '{name}' is too large.\n");
                                skipped++;
                                continue;
                            }

                            unc = (uint)zunc;
                            comp = (uint)zcomp;
                            local = zlocal;
                        }
                    }
                }

                var entry = new Entry
                {
                    FileName = Normalize(raw),
                    Length = unc,
                    Flags = (ushort)(ResFullPath | ResNeedFileStart),
                    Method = zip.Method,
                    Crc32 = zip.Crc,
                    CompressedSize = comp,
                    Position = local,
                };
                if (entry.Method != MethodStored)
                {
                    entry.Flags |= ResCompressed;
                }

                if (entry.Method == MethodImplode)
                {
                    int bits = zip.Flags & 6;
                    if (bits == 2)
                    {
                        entry.Method = MethodImplode2;
                    }
                    else if (bits == 4)
                    {
                        entry.Method = MethodImplode4;
                    }
                    else if (bits == 6)
                    {
                        entry.Method = MethodImplode6;
                    }
                    else
                    {
                        entry.Method = MethodImplode0;
                    }
                }

                Entries.Add(entry);
            }

            NumLumps -= skipped;
            GenerateHash();
            PostProcessArchive(filter);
            return true;
        }

        public Compressed GetRawData(uint index)
        {
            var cbuf = new Compressed();
            if (index >= (uint)NumLumps || Entries[(int)index].Length == 0)
            {
                cbuf.Method = MethodStored;
                return cbuf;
            }

            Entry e = Entries[(int)index];
            cbuf.Size = e.Length;
            cbuf.CompressedSize = e.CompressedSize;
            cbuf.Method = e.Method;
            cbuf.Crc32 = e.Crc32;
            cbuf.Buffer = new byte[e.CompressedSize];
            if ((e.Flags & ResNeedFileStart) != 0)
            {
                SetEntryAddress(index);
            }

            Reader.Seek((long)e.Position, 0);
            if (cbuf.Buffer.Length > 0)
            {
                Reader.Read(cbuf.Buffer, cbuf.Buffer.Length);
            }

            return cbuf;
        }

        public void SetEntryAddress(uint index)
        {
            Entry e = Entries[(int)index];
            Reader.Seek((long)e.Position, 0);
            var local = new byte[LocalHeader];
            Reader.Read(local, LocalHeader);
            int skip = ReadU16(local, 26) + ReadU16(local, 28);
            e.Position += (ulong)(LocalHeader + skip);
            e.Flags = (ushort)(e.Flags & ~ResNeedFileStart);
        }

        public Reader Destroy()
        {
            Destroyed = true;
            DestroyCount++;
            return Reader;
        }

        void GenerateHash()
        {
            HashGenerated = true;
        }

        void PostProcessArchive(object? filter)
        {
            PostProcessed = true;
            PostedFilter = filter;
        }

        void Say(int level, string text)
        {
            Messages.Add(new Message(level, text));
        }
    }

    public static Archive? CheckZip(string filename, Reader file, object? filter)
    {
        if (file.Length >= LocalHeader)
        {
            file.Seek(0, 0);
            var head = new byte[4];
            file.Read(head, 4);
            file.Seek(0, 0);
            if (head[0] == (byte)'P' && head[1] == (byte)'K' && head[2] == 3 && head[3] == 4)
            {
                var rf = new Archive(filename, file);
                if (rf.Open(filter))
                {
                    return rf;
                }

                file = rf.Destroy();
                _ = file;
            }
        }

        return null;
    }

    public static uint FindCentralDir(Reader fin, ref bool zip64)
    {
        uint fileSize = (uint)fin.Length;
        uint uMaxBack = Math.Min(0xffffu, fileSize);
        uint uBackRead = 4;
        uint uPosFound = 0;
        var buf = new byte[BufReadComment + 4];
        while (uBackRead < uMaxBack)
        {
            if (uBackRead + BufReadComment > uMaxBack)
            {
                uBackRead = uMaxBack;
            }
            else
            {
                uBackRead += BufReadComment;
            }

            uint uReadPos = fileSize - uBackRead;
            uint uReadSize = Math.Min((uint)(BufReadComment + 4), fileSize - uReadPos);
            if (fin.Seek(uReadPos, 0) != 0)
            {
                break;
            }

            if (fin.Read(buf, (int)uReadSize) != (int)uReadSize)
            {
                break;
            }

            int i = (int)uReadSize - 3;
            while (i-- > 0)
            {
                if (buf[i] == (byte)'P' && buf[i + 1] == (byte)'K' && buf[i + 2] == 5 && buf[i + 3] == 6 && !zip64 && uPosFound == 0)
                {
                    zip64 = false;
                    uPosFound = uReadPos + (uint)i;
                }

                if (buf[i] == (byte)'P' && buf[i + 1] == (byte)'K' && buf[i + 2] == 6 && buf[i + 3] == 6)
                {
                    zip64 = true;
                    uPosFound = uReadPos + (uint)i;
                    return uPosFound;
                }
            }
        }

        return uPosFound;
    }

    public static string Normalize(byte[] raw)
    {
        if (raw.Length == 0)
        {
            return "";
        }

        byte[] bytes = raw;
        if (!IsUtf8(raw))
        {
            bytes = (byte[])raw.Clone();
            for (int n = 0; n < bytes.Length; n++)
            {
                if ((bytes[n] & 0x80) != 0)
                {
                    bytes[n] = (byte)'@';
                }
            }
        }

        string text = Encoding.UTF8.GetString(bytes);
        var sb = new StringBuilder(text.Length);
        foreach (char ch in text)
        {
            char c = ch is >= 'A' and <= 'Z' ? (char)(ch + 32) : ch;
            if (c == '\\')
            {
                c = '/';
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    readonly struct Header
    {
        public ushort Flags { get; init; }
        public ushort Method { get; init; }
        public uint Crc { get; init; }
        public uint Comp32 { get; init; }
        public uint Unc32 { get; init; }
        public int NameLen { get; init; }
        public int ExtraLen { get; init; }
        public int CommentLen { get; init; }
        public uint Local32 { get; init; }
    }

    static bool Step(byte[] directory, ref int cursor)
    {
        if (!TryHeader(directory, cursor, out Header zip))
        {
            return false;
        }

        int end = cursor + CentralHeader + zip.NameLen + zip.ExtraLen + zip.CommentLen;
        if (end > directory.Length)
        {
            return false;
        }

        cursor = end;
        return true;
    }

    static bool TryHeader(byte[] directory, int start, out Header zip)
    {
        zip = default;
        if ((uint)start > (uint)directory.Length || start + CentralHeader > directory.Length)
        {
            return false;
        }

        zip = new Header
        {
            Flags = ReadU16(directory, start + 8),
            Method = ReadU16(directory, start + 10),
            Crc = ReadU32(directory, start + 16),
            Comp32 = ReadU32(directory, start + 20),
            Unc32 = ReadU32(directory, start + 24),
            NameLen = ReadU16(directory, start + 28),
            ExtraLen = ReadU16(directory, start + 30),
            CommentLen = ReadU16(directory, start + 32),
            Local32 = ReadU32(directory, start + 42),
        };
        return true;
    }

    static bool IsUtf8(byte[] raw)
    {
        try
        {
            Encoding.UTF8.GetString(raw);
            var strict = new UTF8Encoding(false, true);
            strict.GetString(raw);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    static ushort ReadU16(byte[] b, int o) => (ushort)(b[o] | (b[o + 1] << 8));

    static uint ReadU32(byte[] b, int o) => (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24));

    static ulong ReadU64(byte[] b, int o) => ReadU32(b, o) | ((ulong)ReadU32(b, o + 4) << 32);
}
