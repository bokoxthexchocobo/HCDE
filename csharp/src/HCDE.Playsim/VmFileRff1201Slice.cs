namespace HCDE.Playsim;

/// <summary>
/// Blood RFF directory and <c>BloodCrypt</c> from <c>file_rff.cpp</c> (<b>1–162</b>).
/// Chunk <b>1201</b>. <c>NormalizeFileName</c> is a host callback. <c>GenerateHash</c> is counted.
/// Multi-byte fields are little-endian, matching <c>LittleLong</c> on this build.
/// </summary>
public static class VmFileRff1201Slice
{
    public const int LumpSize = 48;
    public const int MethodStored = 0;
    public const int MethodRffCrypt = 1339;
    public const int ResCompressed = 16;
    public const int NamespaceGlobal = 0;

    public static void BloodCrypt(byte[] data, int key, int length)
    {
        int p = key & 0xFF;
        for (int i = 0; i < length; i++)
            data[i] ^= (byte)(p + (i >> 1));
    }

    public sealed class Entry
    {
        public uint Position;
        public uint CompressedSize;
        public uint Length;
        public int Flags;
        public int Method;
        public int Namespace;
        public uint ResourceId;
        public string FileName = "";
    }

    public sealed class Resource
    {
        public string SourceName = "";
        public List<Entry> Entries = new();
        public int HashBuilds;
        public Func<string, string>? Normalize;

        public string NormalizeFileName(string name) => Normalize == null ? name : Normalize(name);

        public void GenerateHash() => HashBuilds++;
    }

    public static Resource? Check(string filename, byte[] file, Func<string, string>? normalize = null)
    {
        if (file.Length < 16)
            return null;
        if (file[0] != (byte)'R' || file[1] != (byte)'F' || file[2] != (byte)'F' || file[3] != 0x1A)
            return null;

        var resource = new Resource { SourceName = filename, Normalize = normalize };
        Open(resource, file);
        return resource;
    }

    public static void Open(Resource file, byte[] data)
    {
        uint numLumps = ReadU32(data, 12);
        uint dirOfs = ReadU32(data, 8);
        var lumps = new byte[numLumps * LumpSize];
        int available = data.Length - (int)dirOfs;
        if (available > 0)
        {
            int count = available < lumps.Length ? available : lumps.Length;
            Buffer.BlockCopy(data, (int)dirOfs, lumps, 0, count);
        }

        BloodCrypt(lumps, (int)dirOfs, lumps.Length);
        for (int i = 0; i < numLumps; i++)
        {
            int at = i * LumpSize;
            var entry = new Entry
            {
                Position = ReadU32(lumps, at + 16),
                Length = ReadU32(lumps, at + 20),
                Namespace = NamespaceGlobal,
                ResourceId = ReadU32(lumps, at + 44),
            };
            entry.CompressedSize = entry.Length;
            if ((lumps[at + 32] & 0x10) != 0)
            {
                entry.Flags = ResCompressed;
                entry.Method = MethodRffCrypt;
            }
            else
            {
                entry.Flags = 0;
                entry.Method = MethodStored;
            }

            var name = new byte[13];
            Buffer.BlockCopy(lumps, at + 36, name, 0, 8);
            name[8] = 0;
            int len = 0;
            while (len < 8 && name[len] != 0)
                len++;
            name[len] = (byte)'.';
            name[len + 1] = lumps[at + 33];
            name[len + 2] = lumps[at + 34];
            name[len + 3] = lumps[at + 35];
            name[len + 4] = 0;
            entry.FileName = file.NormalizeFileName(Bytes(name, len + 4));
            file.Entries.Add(entry);
        }

        file.GenerateHash();
    }

    static string Bytes(byte[] name, int count)
    {
        var chars = new char[count];
        for (int i = 0; i < count; i++)
            chars[i] = (char)name[i];
        return new string(chars);
    }

    static uint ReadU32(byte[] data, int offset)
    {
        return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
    }
}
