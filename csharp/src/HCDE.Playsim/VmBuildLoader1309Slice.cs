namespace HCDE.Playsim;

/// <summary>
/// Build ART tile count, palette translation, and tile registration from
/// <c>buildloader.cpp</c> (1-290). Chunk 1309.
/// </summary>
public sealed class VmBuildLoader1309
{
    public const int TranslationStandard = 3;
    public const int TranslationMask = 0xFFFF;

    readonly Host _host;

    public VmBuildLoader1309(Host host)
    {
        _host = host;
    }

    public static int CountTiles(ReadOnlySpan<byte> tiles)
    {
        if (tiles.Length < 16)
            return 0;
        if (ReadI32(tiles, 0) != 1)
            return 0;
        int start = ReadI32(tiles, 8);
        int end = ReadI32(tiles, 12);
        return end >= start ? end - start + 1 : 0;
    }

    public int BuildPalette(int lump)
    {
        if (_host.Length(lump) < 768)
            return 0;
        byte[] data = _host.Read(lump);
        bool blood = false;
        for (int c = 0; c < 765; c++)
        {
            if (data[c] >= 64)
            {
                blood = true;
                break;
            }
        }

        var palette = new uint[256];
        var remap = new byte[256];
        for (int c = 0; c < 255; c++)
        {
            int r;
            int g;
            int b;
            if (!blood)
            {
                int pr = data[3 * c];
                int pg = data[3 * c + 1];
                int pb = data[3 * c + 2];
                r = (pr << 2) | (pr >> 4);
                g = (pg << 2) | (pg >> 4);
                b = (pb << 2) | (pb >> 4);
            }
            else
            {
                r = data[3 * c] << 2;
                g = data[3 * c + 1] << 2;
                b = data[3 * c + 2] << 2;
            }

            palette[c] = (uint)((255 << 24) | ((r & 255) << 16) | ((g & 255) << 8) | (b & 255));
            remap[c] = _host.Pick(r, g, b);
        }

        palette[255] = 0;
        remap[255] = 0;
        int id = _host.StoreTranslation(TranslationStandard, palette, remap);
        return id & TranslationMask;
    }

    public void AddTiles(string prefix, ReadOnlySpan<byte> tiles, Remap remap)
    {
        if (tiles.Length < 16)
            throw new InvalidOperationException("art header");
        int start = ReadI32(tiles, 8);
        int end = ReadI32(tiles, 12);
        if (end < start)
            return;
        int count = end - start + 1;
        int xAt = 16;
        int yAt = xAt + count * 2;
        int anmAt = yAt + count * 2;
        int dataAt = anmAt + count * 4;
        if (dataAt > tiles.Length)
            throw new InvalidOperationException("art tables");
        int cursor = dataAt;
        for (int i = start; i <= end; i++)
        {
            int pic = i - start;
            int width = ReadU16(tiles, xAt + pic * 2);
            int height = ReadU16(tiles, yAt + pic * 2);
            uint anm = ReadU32(tiles, anmAt + pic * 4);
            if (width <= 0 || height <= 0)
                continue;
            int xOff = (sbyte)((anm >> 8) & 255) + width / 2;
            int yOff = (sbyte)((anm >> 16) & 255) + height / 2;
            int size = width * height;
            var pixels = tiles.Slice(cursor, size).ToArray();
            cursor += size;
            string name = $"{prefix}BTIL{i:D4}";
            int id = _host.AddTexture(prefix, i, name, width, height, xOff, yOff, pixels, remap);
            if ((anm & 63) != 0 && (anm & 192) != 0)
            {
                int type = (anm & 192) switch
                {
                    64 => 2,
                    128 => 0,
                    192 => 1,
                    _ => 0,
                };
                int speed = (int)((anm >> 24) & 15);
                speed = Math.Max(1, ((1 << speed) * 1000) / 120);
                _host.AddSimpleAnim(id, (int)(anm & 63), type, speed);
            }

            int rot = (int)((anm >> 28) & 7);
            if (rot == 1)
                _host.SetRotations(id, FiveView(id), 0x00FC);
            else if (rot == 2)
                _host.SetRotations(id, EightView(id), 0);
        }
    }

    public int Init()
    {
        int total = 0;
        int lumps = _host.Count;
        for (int i = 0; i < lumps; i++)
        {
            string name = _host.FullName(i);
            if (_host.CheckFullName(name) != i)
                continue;
            if (ExtractFileBase(name, true).ToLowerInvariant() != "palette.dat" || _host.Length(i) < 768)
                continue;
            string path = ExtractFilePath(name);
            if (path.Length > 0 && path[^1] != '/')
                path += "/";
            int translation = BuildPalette(i);
            Remap remap = _host.GetTranslation(TranslationStandard, translation);
            int container = _host.Container(i);
            for (int art = 0; art < 1000; art++)
            {
                string artPath = $"{path}tiles{art:D3}.art";
                int lump = _host.CheckFullName(artPath, container);
                if (lump < 0)
                    break;
                int length = _host.Length(lump);
                if (length < 0)
                    throw new InvalidOperationException("art length");
                byte[] data = _host.Read(lump);
                int num = CountTiles(data);
                if (num > 0)
                {
                    AddTiles(path, data, remap);
                    total += num;
                }
            }
        }

        return total;
    }

    public static string ExtractFilePath(string path)
    {
        if (path.Length == 0)
            return "";
        int src = path.Length - 1;
        while (src != 0 && path[src - 1] != '/')
            src--;
        return path[..src];
    }

    public static string ExtractFileBase(string path, bool includeExtension)
    {
        if (path.Length == 0)
            return "";
        int src = path.Length - 1;
        while (src != 0 && path[src - 1] != '/')
            src--;
        string file = path[src..];
        if (!includeExtension)
        {
            int dot = file.LastIndexOf('.');
            if (dot >= 0)
                return file[..dot];
        }

        return file;
    }

    public static int[] FiveView(int id)
    {
        var tex = new int[16];
        tex[0] = id;
        tex[1] = id;
        for (int j = 1; j < 4; j++)
        {
            tex[j * 2] = id + j;
            tex[j * 2 + 1] = id + j;
            tex[16 - j * 2] = id + j;
            tex[17 - j * 2] = id + j;
        }

        tex[8] = id;
        tex[9] = id;
        return tex;
    }

    public static int[] EightView(int id)
    {
        var tex = new int[16];
        tex[0] = id;
        tex[1] = id;
        for (int j = 1; j < 8; j++)
        {
            tex[16 - j * 2] = id + j;
            tex[17 - j * 2] = id + j;
        }

        return tex;
    }

    static int ReadI32(ReadOnlySpan<byte> data, int offset)
    {
        return data[offset]
            | (data[offset + 1] << 8)
            | (data[offset + 2] << 16)
            | (data[offset + 3] << 24);
    }

    static int ReadU16(ReadOnlySpan<byte> data, int offset)
    {
        return data[offset] | (data[offset + 1] << 8);
    }

    static uint ReadU32(ReadOnlySpan<byte> data, int offset)
    {
        return (uint)ReadI32(data, offset);
    }

    public sealed class Remap
    {
        public Remap(int slot, int index)
        {
            Slot = slot;
            Index = index;
        }

        public int Slot { get; }
        public int Index { get; }
    }

    public sealed class Lump
    {
        public string Name { get; init; } = "";
        public int Container { get; init; }
        public byte[] Data { get; init; } = Array.Empty<byte>();
    }

    public sealed class Host
    {
        public List<Lump> Lumps { get; } = new();
        public List<string> Names { get; } = new();
        public List<byte[]> Pixels { get; } = new();
        public List<string> Anims { get; } = new();
        public List<(int id, int[] tex, int flip)> Rotations { get; } = new();
        public List<(int slot, uint[] palette, byte[] remap)> Stored { get; } = new();
        public int NextId { get; set; } = 1;

        public int Count => Lumps.Count;

        public string FullName(int index) => Lumps[index].Name;

        public int Length(int index) => Lumps[index].Data.Length;

        public byte[] Read(int index) => Lumps[index].Data;

        public int Container(int index) => Lumps[index].Container;

        public int CheckFullName(string name)
        {
            for (int i = Lumps.Count - 1; i >= 0; i--)
            {
                if (Lumps[i].Name == name)
                    return i;
            }

            return -1;
        }

        public int CheckFullName(string name, int container)
        {
            for (int i = Lumps.Count - 1; i >= 0; i--)
            {
                if (Lumps[i].Name == name && Lumps[i].Container == container)
                    return i;
            }

            return -1;
        }

        public byte Pick(int r, int g, int b) => (byte)((r + g + b) & 255);

        public int StoreTranslation(int slot, uint[] palette, byte[] remap)
        {
            Stored.Add((slot, palette, remap));
            return (slot << 16) | (Stored.Count - 1);
        }

        public Remap GetTranslation(int slot, int index) => new(slot, index);

        public int AddTexture(
            string prefix,
            int tile,
            string name,
            int width,
            int height,
            int xOff,
            int yOff,
            byte[] pixels,
            Remap remap)
        {
            int id = NextId++;
            Names.Add($"{id}:{prefix}:{tile}:{name}:{width}x{height}:{xOff},{yOff}:{remap.Slot}:{remap.Index}");
            Pixels.Add(pixels);
            return id;
        }

        public void AddSimpleAnim(int id, int frames, int type, int speed)
        {
            Anims.Add($"{id}:{frames}:{type}:{speed}");
        }

        public void SetRotations(int id, int[] textures, int flip)
        {
            Rotations.Add((id, textures, flip));
        }
    }
}
