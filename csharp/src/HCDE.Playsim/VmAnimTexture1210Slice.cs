namespace HCDE.Playsim;

/// <summary>
/// Frame size, YUV/RGB/palette copies, and the two-frame swap from <c>animtexture.cpp</c> (<b>1–241</b>).
/// Chunk <b>1210</b>. The RGB copy rereads the first pixel. Game textures come from the host lookup.
/// </summary>
public sealed class VmVpxImage1210
{
    public const int I420 = 0x100 | 2;
    public const int I422 = 0x100 | 5;
    public const int I444 = 0x100 | 6;
    public const int I440 = 0x100 | 7;

    public int Fmt;
    public byte[] Y = Array.Empty<byte>();
    public byte[] U = Array.Empty<byte>();
    public byte[] V = Array.Empty<byte>();
    public int YStride;
    public int UStride;
    public int VStride;
}

/// <summary>Non-owning BGRA view. Pitch is width times 4. Remap and translucency are not applied.</summary>
public sealed class VmBgraBitmap1210
{
    public VmBgraBitmap1210(byte[] data, int pitch, int width, int height)
    {
        Data = data;
        Pitch = pitch;
        Width = width;
        Height = height;
    }

    public byte[] Data { get; }
    public int Pitch { get; }
    public int Width { get; }
    public int Height { get; }
}

/// <summary>
/// Game-texture seam for <c>SetSize</c> and <c>CleanHardwareData</c>.
/// <c>EQUAL_EPSILON</c> is <c>1/65536</c>.
/// </summary>
public sealed class VmAnimGameTexture1210
{
    public const float EqualEpsilon = 1f / 65536f;

    public VmAnimGameTexture1210(VmAnimTexture1210Slice texture, int id)
    {
        Base = texture;
        Id = id;
    }

    public VmAnimTexture1210Slice Base { get; }
    public int Id { get; }
    public int TexelWidth { get; private set; }
    public int TexelHeight { get; private set; }
    public float DisplayWidth { get; private set; }
    public float DisplayHeight { get; private set; }
    public float ScaleX { get; private set; }
    public float ScaleY { get; private set; }
    public int CleanCalls { get; private set; }
    public bool LastCleanFull { get; private set; }
    public int BaseHardwareCleans { get; private set; }
    public int DescriptorDeletes { get; private set; }
    public List<bool> Materials { get; } = new();

    public void SetSize(int x, int y)
    {
        TexelWidth = x;
        TexelHeight = y;
        SetDisplaySize(x, y);
        Base.SetSize(x, y);
    }

    public void SetDisplaySize(float w, float h)
    {
        DisplayWidth = w;
        DisplayHeight = h;
        ScaleX = TexelWidth / w;
        ScaleY = TexelHeight / h;
        if ((int)(ScaleX * w) != TexelWidth)
            ScaleX += EqualEpsilon;
        if ((int)(ScaleY * h) != TexelHeight)
            ScaleY += EqualEpsilon;
    }

    public void CleanHardwareData(bool full = true)
    {
        LastCleanFull = full;
        CleanCalls++;
        if (full)
            BaseHardwareCleans++;
        for (int i = 0; i < Materials.Count; i++)
        {
            if (Materials[i])
                DescriptorDeletes++;
        }
    }
}

public sealed class VmAnimTexture1210Slice
{
    public const int Paletted = 0;
    public const int Rgb = 1;
    public const int Yuv = 2;
    public const int Vpx = 3;
    public const int TextureUseOverride = 10;

    byte[] Image = Array.Empty<byte>();

    public int Format { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[] Pixels => Image;

    public void SetSize(int w, int h)
    {
        Width = w;
        Height = h;
    }

    public void SetFrameSize(int format, int width, int height)
    {
        Format = format;
        SetSize(width, height);
        Image = new byte[width * height * 4];
    }

    public void SetFrame(byte[]? palette, object? data)
    {
        if (data == null)
            return;
        if (Format == Yuv)
            CopyYuv((byte[])data);
        else if (Format == Vpx)
            CopyVpx((VmVpxImage1210)data);
        else if (Format == Rgb)
            CopyRgb((byte[])data);
        else if (Format == Paletted)
            CopyPaletted(palette, (byte[])data);
    }

    public VmBgraBitmap1210 GetBgraBitmap(byte[]? remap, int[]? trans)
    {
        _ = remap;
        _ = trans;
        return new VmBgraBitmap1210(Image, Width * 4, Width, Height);
    }

    void CopyYuv(byte[] src)
    {
        int pixels = Width * Height;
        for (int i = 0; i < pixels; i++)
        {
            int s = i * 4;
            YuvToRgb(src[s], src[s + 1], src[s + 2], Image, i * 4);
        }
    }

    void CopyVpx(VmVpxImage1210 img)
    {
        int mode;
        if (img.Fmt == VmVpxImage1210.I420)
            mode = 1;
        else if (img.Fmt == VmVpxImage1210.I444)
            mode = 2;
        else if (img.Fmt == VmVpxImage1210.I422)
            mode = 3;
        else if (img.Fmt == VmVpxImage1210.I440)
            mode = 4;
        else
            return;
        CopyPlanes(img, mode);
    }

    void CopyPlanes(VmVpxImage1210 img, int mode)
    {
        int at = 0;
        uint height = (uint)Height;
        uint width = (uint)Width;
        for (uint y = 0; y < height; y++)
        {
            for (uint x = 0; x < width; x++)
            {
                int cx = (int)x;
                int cy = (int)y;
                if (mode == 1)
                {
                    cy >>= 1;
                    cx >>= 1;
                }
                else if (mode == 3)
                {
                    cx >>= 1;
                }
                else if (mode == 4)
                {
                    cy >>= 1;
                }

                YuvToRgb(
                    img.Y[img.YStride * (int)y + (int)x],
                    img.U[img.UStride * cy + cx],
                    img.V[img.VStride * cy + cx],
                    Image,
                    at);
                at += 4;
            }
        }
    }

    void CopyRgb(byte[] src)
    {
        int pixels = Width * Height;
        for (int i = 0; i < pixels; i++)
        {
            int d = i * 4;
            Image[d] = src[2];
            Image[d + 1] = src[1];
            Image[d + 2] = src[0];
            Image[d + 3] = 255;
        }
    }

    void CopyPaletted(byte[]? palette, byte[] src)
    {
        if (palette == null)
            throw new InvalidOperationException("assert(Palette)");
        int pixels = Width * Height;
        for (int i = 0; i < pixels; i++)
        {
            int index = src[i];
            int d = i * 4;
            Image[d] = palette[index * 3 + 2];
            Image[d + 1] = palette[index * 3 + 1];
            Image[d + 2] = palette[index * 3];
            Image[d + 3] = 255;
        }
    }

    static void YuvToRgb(byte yi, byte ui, byte vi, byte[] rgb, int at)
    {
        float y = yi * (1f / 255f);
        float u = ui * (1f / 255f) - 0.5f;
        float v = vi * (1f / 255f) - 0.5f;
        y = 1.1643f * (y - 0.0625f);
        float r = y + 1.5958f * v;
        float g = y - 0.39173f * u - 0.81290f * v;
        float b = y + 2.017f * u;
        rgb[at + 2] = (byte)(Clamp(r, 0f, 1f) * 255);
        rgb[at + 1] = (byte)(Clamp(g, 0f, 1f) * 255);
        rgb[at] = (byte)(Clamp(b, 0f, 1f) * 255);
        rgb[at + 3] = 255;
    }

    static float Clamp(float val, float min, float max)
    {
        return MathF.Max(MathF.Min(val, max), min);
    }
}

/// <summary>
/// Two override-texture slots. <c>active</c> starts at 1 and toggles before the write.
/// </summary>
public sealed class VmAnimTextures1210Slice : IDisposable
{
    readonly VmAnimGameTexture1210?[] Frames = new VmAnimGameTexture1210?[2];

    public static Func<string, int, VmAnimGameTexture1210?>? FindGameTexture { get; set; }

    public int Active { get; private set; }

    public VmAnimTextures1210Slice()
    {
        Active = 1;
        var find = FindGameTexture;
        if (find == null)
            throw new NullReferenceException();
        Frames[0] = find("AnimTextureFrame1", VmAnimTexture1210Slice.TextureUseOverride);
        Frames[1] = find("AnimTextureFrame2", VmAnimTexture1210Slice.TextureUseOverride);
    }

    public void Dispose()
    {
        Clean();
    }

    public void Clean()
    {
        if (Frames[0] != null)
            Frames[0]!.CleanHardwareData(true);
        if (Frames[1] != null)
            Frames[1]!.CleanHardwareData(true);
        Frames[0] = null;
        Frames[1] = null;
    }

    public void SetSize(int format, int width, int height)
    {
        Frame(0).SetFrameSize(format, width, height);
        Frame(1).SetFrameSize(format, width, height);
        Game(0).SetSize(width, height);
        Game(1).SetSize(width, height);
        Game(0).CleanHardwareData();
        Game(1).CleanHardwareData();
    }

    public void SetFrame(byte[]? palette, object? data)
    {
        Active ^= 1;
        Frame(Active).SetFrame(palette, data);
        Game(Active).CleanHardwareData();
    }

    public VmAnimGameTexture1210? GetFrame()
    {
        return Frames[Active];
    }

    VmAnimTexture1210Slice Frame(int slot)
    {
        return Game(slot).Base;
    }

    VmAnimGameTexture1210 Game(int slot)
    {
        return Frames[slot] ?? throw new NullReferenceException();
    }
}
