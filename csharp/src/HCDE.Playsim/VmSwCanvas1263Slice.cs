namespace HCDE.Playsim;

/// <summary>
/// Software canvas pixels from <c>swcanvastexture.cpp</c> (<b>1–188</b>). Chunk <b>1263</b>.
/// The map keeps the software texture after destruction. The unrendered paletted fill is the
/// first half zero and the second half 255, using integer sizes. The BGRA fill writes 255
/// from byte offset <c>width * height / 2</c> for <c>4 * width * height / 2</c> bytes.
/// True color copies the canvas through its pitch, then palettizes each column with
/// <c>RPART</c>, <c>GPART</c>, and <c>BPART</c> into <c>RGB32k</c>. The style argument on
/// the paletted lock is not read. <c>SetUpdated</c> is always passed false.
/// </summary>
public sealed class VmSwCanvasHost1263
{
    public readonly List<string> Log = new();
    public readonly Dictionary<int, VmSwCanvas1263> Map = new();
    public readonly byte[] Rgb32k = new byte[32 * 32 * 32];
    public readonly byte[] Remap = new byte[256];
    public int CurrentUpdate;
    public int NextCanvasId = 1;
    public int NextTextureId = 1;
}

public sealed class VmSwCanvas1263
{
    public readonly VmSwCanvasHost1263 Host;
    public readonly int Id;
    public readonly int CamId;
    public readonly int Width;
    public readonly int Height;
    public Canvas? PalCanvas;
    public Canvas? BgraCanvas;
    public byte[]? Pal;
    public byte[]? Bgra;
    public bool NeedsUpdate = true;
    public bool FirstUpdate = true;
    public bool LastUpdateType;
    readonly byte[]?[] _cached = new byte[]?[3];
    readonly int[] _cacheGen = { -1, -1, -1 };

    public sealed class Canvas
    {
        public int Id;
        public int Width;
        public int Height;
        public int Pitch;
        public bool Bgra;
        public byte[] Pixels = [];
    }

    VmSwCanvas1263(VmSwCanvasHost1263 host, int camId, int width, int height)
    {
        Host = host;
        Id = host.NextTextureId++;
        CamId = camId;
        Width = width;
        Height = height;
    }

    public static VmSwCanvas1263 Create(VmSwCanvasHost1263 host, int camId, int width, int height)
    {
        var vm = new VmSwCanvas1263(host, camId, width, height);
        host.Log.Add(host.Map.ContainsKey(camId) ? $"relink {camId} {vm.Id}" : $"link {camId} {vm.Id}");
        host.Map[camId] = vm;
        return vm;
    }

    public static VmSwCanvas1263? Find(VmSwCanvasHost1263 host, int camId)
    {
        if (!host.Map.TryGetValue(camId, out var vm))
        {
            host.Log.Add($"miss {camId}");
            return null;
        }

        host.Log.Add($"hit {camId} {vm.Id}");
        return vm;
    }

    public void Destroy()
    {
        Drop(ref PalCanvas);
        Drop(ref BgraCanvas);
        Host.Log.Add($"dtor {Id}");
    }

    public byte[] GetPixelsLocked(int style)
    {
        Host.Log.Add($"lock pal {style}");
        NeedUpdate();
        if (PalCanvas == null)
            MakeTexture();
        return Pal ?? throw new NullReferenceException();
    }

    public byte[] GetPixelsBgraLocked()
    {
        Host.Log.Add("lock bgra");
        NeedUpdate();
        if (BgraCanvas == null)
            MakeTextureBgra();
        return Bgra ?? throw new NullReferenceException();
    }

    public byte[] GetPixels(int style)
    {
        if (_cacheGen[style] == Host.CurrentUpdate)
        {
            Host.Log.Add($"cache hit {style}");
            return _cached[style] ?? throw new NullReferenceException();
        }

        Host.Log.Add($"cache miss {style}");
        byte[] data = style != 2 ? GetPixelsLocked(style) : GetPixelsBgraLocked();
        _cached[style] = data;
        _cacheGen[style] = Host.CurrentUpdate;
        return data;
    }

    public void Unload()
    {
        Drop(ref PalCanvas);
        Drop(ref BgraCanvas);
        Pal = null;
        Bgra = null;
        for (int i = 0; i < _cached.Length; i++)
        {
            _cached[i] = null;
            _cacheGen[i] = -1;
        }

        Host.Log.Add("unload");
    }

    public void UpdatePixels(bool truecolor)
    {
        if (truecolor)
        {
            Host.Log.Add("update tc");
            Canvas src = BgraCanvas ?? throw new NullReferenceException();
            byte[] bgra = Bgra ?? throw new NullReferenceException();
            Flip(bgra, src);
            Host.Log.Add("flip");
            byte[] pal = GetPixels(0);
            byte[] read = GetPixels(2);
            Shade(pal, read);
            Host.Log.Add("remap " + Hex(pal));
        }
        else
        {
            Host.Log.Add("update pal");
            Canvas src = PalCanvas ?? throw new NullReferenceException();
            byte[] pal = Pal ?? throw new NullReferenceException();
            FlipRemap(pal, src);
            Host.Log.Add("remap " + Hex(pal));
        }

        NeedsUpdate = false;
        FirstUpdate = false;
        LastUpdateType = false;
        Host.Log.Add($"updated {(NeedsUpdate ? 1 : 0)} {(FirstUpdate ? 1 : 0)} {(LastUpdateType ? 1 : 0)}");
    }

    void MakeTexture()
    {
        PalCanvas = NewCanvas(false);
        int count = Width * Height;
        Pal = new byte[count];
        int half = count / 2;
        for (int i = half; i < half + half; i++)
            Pal[i] = 255;
        Host.Log.Add("fill p " + Hex(Pal));
    }

    void MakeTextureBgra()
    {
        BgraCanvas = NewCanvas(true);
        Bgra = new byte[4 * Width * Height];
        int start = Width * Height / 2;
        int length = 4 * Width * Height / 2;
        for (int i = 0; i < length; i++)
            Bgra[start + i] = 255;
        Host.Log.Add("fill b " + Hex(Bgra));
    }

    Canvas NewCanvas(bool bgra)
    {
        int pitch = Width;
        var canvas = new Canvas
        {
            Id = Host.NextCanvasId++,
            Width = Width,
            Height = Height,
            Pitch = pitch,
            Bgra = bgra,
            Pixels = new byte[pitch * Height * (bgra ? 4 : 1)],
        };
        Host.Log.Add($"canvas {(bgra ? "bgra" : "pal")} {canvas.Id} {Width} {Height} {pitch}");
        return canvas;
    }

    void Flip(byte[] dst, Canvas src)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                int from = (i + j * src.Pitch) * 4;
                int to = (i * Height + j) * 4;
                dst[to] = src.Pixels[from];
                dst[to + 1] = src.Pixels[from + 1];
                dst[to + 2] = src.Pixels[from + 2];
                dst[to + 3] = src.Pixels[from + 3];
            }
        }
    }

    void FlipRemap(byte[] dst, Canvas src)
    {
        byte[] remap = Host.Remap;
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
                dst[i * Height + j] = remap[src.Pixels[i + j * src.Pitch]];
        }
    }

    void Shade(byte[] pal, byte[] bgra)
    {
        byte[] table = Host.Rgb32k;
        int palAt = 0;
        int bgraAt = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int o = (bgraAt + y) * 4;
                uint color = (uint)(bgra[o] | (bgra[o + 1] << 8) | (bgra[o + 2] << 16) | (bgra[o + 3] << 24));
                int r = (int)((color >> 16) & 0xff);
                int g = (int)((color >> 8) & 0xff);
                int b = (int)(color & 0xff);
                pal[palAt + y] = table[((r >> 3) * 32 + (g >> 3)) * 32 + (b >> 3)];
            }

            palAt += Height;
            bgraAt += Height;
        }
    }

    void NeedUpdate()
    {
        NeedsUpdate = true;
        Host.Log.Add("need");
    }

    void Drop(ref Canvas? canvas)
    {
        if (canvas == null)
            return;
        Host.Log.Add($"drop {(canvas.Bgra ? "bgra" : "pal")} {canvas.Id}");
        canvas = null;
    }

    static string Hex(byte[] bytes)
    {
        var chars = new char[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            byte value = bytes[i];
            chars[i * 2] = HexDigit(value >> 4);
            chars[i * 2 + 1] = HexDigit(value & 15);
        }

        return new string(chars);
    }

    static char HexDigit(int nibble) => (char)(nibble < 10 ? '0' + nibble : 'a' + (nibble - 10));
}
