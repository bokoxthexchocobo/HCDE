namespace HCDE.Playsim;

/// <summary>
/// Model table, skin lookup, and format dispatch from <c>model.cpp</c> (<b>1–229</b>).
/// Chunk <b>1249</b>. Lump IO, texture lookup, <c>Load</c>, and the voxel reader are host seams.
/// The renderer slot count is 3. <c>silent</c> is accepted and does not change the messages.
/// </summary>
public sealed class VmModel1249
{
    public const int Slots = 3;
    public const int TryAny = 1;
    public const int ForceLookup = 128;

    public readonly List<string> Log = new();
    public readonly List<Model> Models = new();
    public readonly Dictionary<string, int> Lumps = new(StringComparer.Ordinal);
    public readonly Dictionary<int, byte[]> Bytes = new();
    public readonly Dictionary<int, string> Names = new();
    public readonly Dictionary<string, int> Textures = new(StringComparer.Ordinal);
    public bool LoadOk = true;
    public bool VoxelOk;
    public long? ForcedLength;

    public void Flush()
    {
        for (int i = Models.Count - 1; i >= 0; i--)
        {
            Models[i].Destroy();
            Log.Add($"flush i={i}");
        }
    }

    public int FindGfx(ref string name)
    {
        if (Lumps.TryGetValue(name, out int exact) && exact != -1)
        {
            Log.Add($"gfx exact {name} lump={exact}");
            return exact;
        }
        int dot = name.LastIndexOf('.');
        int slash = name.LastIndexOf('/');
        if (dot > slash)
            name = name[..dot];
        int best = -1;
        string[] ext = { ".png", ".jpg", ".tga", ".pcx" };
        foreach (string e in ext)
        {
            string candidate = name + e;
            int lump = Lumps.TryGetValue(candidate, out int found) ? found : -1;
            if (lump >= best)
                best = lump;
        }
        Log.Add($"gfx strip {name} lump={best}");
        return best;
    }

    public int LoadSkin(string? path, string? fn)
    {
        if (path == null || fn == null)
        {
            Log.Add("skin null");
            return -1;
        }
        string buffer = path + fn;
        int lump = FindGfx(ref buffer);
        string texname = lump < 0 ? fn : Names.GetValueOrDefault(lump, fn);
        int flags = TryAny | ForceLookup;
        int id = Textures.TryGetValue(texname, out int tex) ? tex : -1;
        Log.Add($"skin lump={lump} name={texname} flags={flags} id={id}");
        return id;
    }

    public static uint FrameHash(ReadOnlySpan<byte> data) => Crc(data);

    public int Find(string? path, string? file, bool silent = false)
    {
        _ = silent;
        if (file == null)
        {
            Log.Add("file null");
            return -1;
        }
        string full = path != null ? path + file : file;
        if (!Lumps.TryGetValue(full, out int lump) || lump < 0)
        {
            Log.Add($"FindModel: '{full}' not found");
            return -1;
        }
        for (int i = 0; i < Models.Count; i++)
        {
            if (string.Equals(Models[i].FileName, full, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        long len = ForcedLength ?? (Bytes.TryGetValue(lump, out byte[]? owned) ? owned.LongLength : 0);
        if (len >= 0x80000000L)
        {
            Log.Add($"LoadModel: File to large: '{full}'");
            return -1;
        }
        byte[] data = Bytes.TryGetValue(lump, out byte[]? got) ? got : Array.Empty<byte>();
        string? kind = Kind(full, data);
        if (kind != null)
        {
            if (!LoadOk)
            {
                Log.Add($"load fail {kind}");
                return -1;
            }
        }
        else if (VoxelOk)
        {
            kind = "voxel";
            Log.Add("voxel own=1");
        }
        else
        {
            Log.Add($"LoadModel: Unknown model format in '{full}'");
            return -1;
        }
        var model = new Model(kind)
        {
            FileName = full,
            Path = path ?? "",
            File = file,
        };
        Models.Add(model);
        Log.Add($"model kind={kind} idx={Models.Count - 1} path={model.Path} file={model.File}");
        return Models.Count - 1;
    }

    string? Kind(string full, byte[] data)
    {
        if (Ends(full, "_d.3d") && Lump(full.Replace("_d.3d", "_a.3d")) > 0)
            return "ue1";
        if (Ends(full, "_a.3d") && Lump(full.Replace("_a.3d", "_d.3d")) > 0)
            return "ue1";
        if (Ends(full, ".obj"))
            return "obj";
        if (Starts(data, "DMDM"u8))
            return "dmd";
        if (Starts(data, "IDP2"u8))
            return "md2";
        if (Starts(data, "IDP3"u8))
            return "md3";
        if (Starts(data, "INTERQUAKEMODEL\0"u8))
            return "iqm";
        return null;
    }

    int Lump(string name) => Lumps.TryGetValue(name, out int lump) ? lump : -1;

    static bool Ends(string name, string suffix)
    {
        int at = name.LastIndexOf(suffix, StringComparison.Ordinal);
        return at >= 0 && at == name.Length - suffix.Length;
    }

    static bool Starts(byte[] data, ReadOnlySpan<byte> magic) =>
        data.Length >= magic.Length && data.AsSpan(0, magic.Length).SequenceEqual(magic);

    static uint Crc(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFFu;
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

    public sealed class Model
    {
        public readonly object?[] Buffers = new object?[Slots];
        public string FileName = "";
        public string Path = "";
        public string File = "";
        public string Kind;

        public Model(string kind) => Kind = kind;

        public void Destroy()
        {
            for (int i = 0; i < Buffers.Length; i++)
                Buffers[i] = null;
        }
    }
}
