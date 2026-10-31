namespace HCDE.Playsim;

/// <summary>
/// DDS fourcc and flag constants, the header layout, and the FDDSTexture
/// declaration from ddstexture.cpp 1-186. Chunk 1390.
/// CheckDDS at line 187 and the method bodies stay outside this band.
/// Fourcc values use little-endian MAKE_ID.
/// </summary>
public static class VmDdsTexture1390Slice
{
    public const int PixelFormatBytes = 32;
    public const int CapsBytes = 16;
    public const int SurfaceBytes = 124;
    public const int HeaderBytes = 128;
    public const string BaseClass = "FImageSource";
    public const string FriendClass = "FTexture";

    public static uint MakeId(char a, char b, char c, char d)
    {
        return (uint)a | ((uint)b << 8) | ((uint)c << 16) | ((uint)d << 24);
    }

    public static readonly uint IdDds = MakeId('D', 'D', 'S', ' ');
    public static readonly uint IdDxt1 = MakeId('D', 'X', 'T', '1');
    public static readonly uint IdDxt2 = MakeId('D', 'X', 'T', '2');
    public static readonly uint IdDxt3 = MakeId('D', 'X', 'T', '3');
    public static readonly uint IdDxt4 = MakeId('D', 'X', 'T', '4');
    public static readonly uint IdDxt5 = MakeId('D', 'X', 'T', '5');

    public const uint DdsdCaps = 0x00000001, DdsdHeight = 0x00000002, DdsdWidth = 0x00000004, DdsdPitch = 0x00000008;
    public const uint DdsdPixelFormat = 0x00001000, DdsdMipMapCount = 0x00020000, DdsdLinearSize = 0x00080000, DdsdDepth = 0x00800000;
    public const uint DdpfAlphaPixels = 0x00000001, DdpfFourCc = 0x00000004, DdpfRgb = 0x00000040;
    public const uint DdscapsComplex = 0x00000008, DdscapsTexture = 0x00001000, DdscapsMipMap = 0x00400000;
    public const uint Ddscaps2Cubemap = 0x00000200, Ddscaps2CubemapPositiveX = 0x00000400, Ddscaps2CubemapNegativeX = 0x00000800;
    public const uint Ddscaps2CubemapPositiveY = 0x00001000, Ddscaps2CubemapNegativeY = 0x00002000, Ddscaps2CubemapPositiveZ = 0x00004000;
    public const uint Ddscaps2CubemapNegatizeZ = 0x00008000, Ddscaps2Volume = 0x00200000;

    public static uint Flag(string name) => name switch
    {
        "DDSD_CAPS" => DdsdCaps,
        "DDSD_HEIGHT" => DdsdHeight,
        "DDSD_WIDTH" => DdsdWidth,
        "DDSD_PITCH" => DdsdPitch,
        "DDSD_PIXELFORMAT" => DdsdPixelFormat,
        "DDSD_MIPMAPCOUNT" => DdsdMipMapCount,
        "DDSD_LINEARSIZE" => DdsdLinearSize,
        "DDSD_DEPTH" => DdsdDepth,
        "DDPF_ALPHAPIXELS" => DdpfAlphaPixels,
        "DDPF_FOURCC" => DdpfFourCc,
        "DDPF_RGB" => DdpfRgb,
        "DDSCAPS_COMPLEX" => DdscapsComplex,
        "DDSCAPS_TEXTURE" => DdscapsTexture,
        "DDSCAPS_MIPMAP" => DdscapsMipMap,
        "DDSCAPS2_CUBEMAP" => Ddscaps2Cubemap,
        "DDSCAPS2_CUBEMAP_POSITIVEX" => Ddscaps2CubemapPositiveX,
        "DDSCAPS2_CUBEMAP_NEGATIVEX" => Ddscaps2CubemapNegativeX,
        "DDSCAPS2_CUBEMAP_POSITIVEY" => Ddscaps2CubemapPositiveY,
        "DDSCAPS2_CUBEMAP_NEGATIVEY" => Ddscaps2CubemapNegativeY,
        "DDSCAPS2_CUBEMAP_POSITIVEZ" => Ddscaps2CubemapPositiveZ,
        "DDSCAPS2_CUBEMAP_NEGATIZEZ" => Ddscaps2CubemapNegatizeZ,
        "DDSCAPS2_VOLUME" => Ddscaps2Volume,
        _ => throw new KeyNotFoundException(name),
    };

    public static int PixelMode(string name) => name switch
    {
        "PIX_Palette" => 0,
        "PIX_Alphatex" => 1,
        "PIX_ARGB" => 2,
        _ => throw new InvalidOperationException(name),
    };

    public static readonly string[] FieldNames =
    {
        "Format", "RMask", "GMask", "BMask", "AMask",
        "RShiftL", "GShiftL", "BShiftL", "AShiftL",
        "RShiftR", "GShiftR", "BShiftR", "AShiftR",
        "Pitch", "LinearSize",
    };

    public readonly record struct MethodSpec(string Name, string Signature, bool DefaultFrame);

    public static readonly MethodSpec[] Methods =
    {
        new("FDDSTexture", "public FDDSTexture(FileReader &lump, int lumpnum, void *surfdesc)", false),
        new("CreatePalettedPixels", "public override PalettedPixels CreatePalettedPixels(int conversion, int frame = 0)", true),
        new("CalcBitShift", "protected static void CalcBitShift(uint32_t mask, uint8_t *lshift, uint8_t *rshift)", false),
        new("ReadRGB", "protected void ReadRGB(FileReader &lump, uint8_t *buffer, int pixelmode)", false),
        new("DecompressDXT1", "protected void DecompressDXT1(FileReader &lump, uint8_t *buffer, int pixelmode)", false),
        new("DecompressDXT3", "protected void DecompressDXT3(FileReader &lump, bool premultiplied, uint8_t *buffer, int pixelmode)", false),
        new("DecompressDXT5", "protected void DecompressDXT5(FileReader &lump, bool premultiplied, uint8_t *buffer, int pixelmode)", false),
        new("CopyPixels", "protected override int CopyPixels(FBitmap *bmp, int conversion, int frame = 0)", true),
    };

    public static MethodSpec Method(string name)
    {
        foreach (var spec in Methods)
        {
            if (spec.Name == name)
                return spec;
        }
        throw new InvalidOperationException(name);
    }

    public static int DefaultFrame(string name)
    {
        var spec = Method(name);
        if (!spec.DefaultFrame)
            throw new InvalidOperationException(name);
        return 0;
    }

    public static int DescOffset(string field) => field switch
    {
        "Size" => 0, "Flags" => 4, "Height" => 8, "Width" => 12,
        "Pitch" or "LinearSize" => 16, "Depth" => 20, "MipMapCount" => 24,
        "Reserved1" => 28, "PixelFormat" => 72, "Caps" => 104, "Reserved2" => 120,
        _ => throw new InvalidOperationException(field),
    };

    public static int PixelOffset(string field) => field switch
    {
        "Size" => 0, "Flags" => 4, "FourCC" => 8, "RGBBitCount" => 12,
        "RBitMask" => 16, "GBitMask" => 20, "BBitMask" => 24, "RGBAlphaBitMask" => 28,
        _ => throw new InvalidOperationException(field),
    };

    public static int CapsOffset(string field) => field switch
    {
        "Caps1" => 0, "Caps2" => 4, "Reserved" => 8,
        _ => throw new InvalidOperationException(field),
    };

    public sealed class PixelFormat
    {
        public uint Size, Flags, FourCc, RgbBitCount, R, G, B, A;
    }

    public sealed class Caps
    {
        public uint Caps1, Caps2;
        public uint[] Reserved = new uint[2];
    }

    public sealed class Surface
    {
        public uint Size, Flags, Height, Width, Depth, MipMapCount, Reserved2;
        public uint[] Reserved1 = new uint[11];
        public PixelFormat Format = new();
        public Caps Caps = new();
        uint _span;
        public int Pitch { get => unchecked((int)_span); set => _span = unchecked((uint)value); }
        public uint LinearSize { get => _span; set => _span = value; }
    }

    public sealed class Header
    {
        public uint Magic;
        public Surface Desc = new();
    }

    /// <summary>Field slots declared on FDDSTexture. The constructor body is not in this band.</summary>
    public sealed class Members
    {
        public uint Format, RMask, GMask, BMask, AMask;
        public byte RShiftL, GShiftL, BShiftL, AShiftL, RShiftR, GShiftR, BShiftR, AShiftR;
        public int Pitch;
        public uint LinearSize;
    }

    public static byte[] WriteHeader(Header header)
    {
        if (header.Desc == null)
            throw new ArgumentNullException(nameof(header));
        var bytes = new byte[HeaderBytes];
        Put(bytes, 0, header.Magic);
        WriteSurface(bytes, 4, header.Desc);
        return bytes;
    }

    public static Header ReadHeader(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length != HeaderBytes)
            throw new ArgumentException("DDS header is 128 bytes");
        var header = new Header { Magic = Take(data, 0) };
        ReadSurface(data, 4, header.Desc);
        return header;
    }

    static void WriteSurface(byte[] dest, int at, Surface s)
    {
        if (s.Reserved1.Length != 11)
            throw new ArgumentException("Reserved1");
        if (s.Caps.Reserved.Length != 2)
            throw new ArgumentException("Caps.Reserved");
        Put(dest, at + DescOffset("Size"), s.Size);
        Put(dest, at + DescOffset("Flags"), s.Flags);
        Put(dest, at + DescOffset("Height"), s.Height);
        Put(dest, at + DescOffset("Width"), s.Width);
        Put(dest, at + DescOffset("LinearSize"), s.LinearSize);
        Put(dest, at + DescOffset("Depth"), s.Depth);
        Put(dest, at + DescOffset("MipMapCount"), s.MipMapCount);
        for (int i = 0; i < 11; i++)
            Put(dest, at + DescOffset("Reserved1") + (i * 4), s.Reserved1[i]);
        WritePixelFormat(dest, at + DescOffset("PixelFormat"), s.Format);
        WriteCaps(dest, at + DescOffset("Caps"), s.Caps);
        Put(dest, at + DescOffset("Reserved2"), s.Reserved2);
    }

    static void ReadSurface(byte[] src, int at, Surface s)
    {
        s.Size = Take(src, at + DescOffset("Size"));
        s.Flags = Take(src, at + DescOffset("Flags"));
        s.Height = Take(src, at + DescOffset("Height"));
        s.Width = Take(src, at + DescOffset("Width"));
        s.LinearSize = Take(src, at + DescOffset("LinearSize"));
        s.Depth = Take(src, at + DescOffset("Depth"));
        s.MipMapCount = Take(src, at + DescOffset("MipMapCount"));
        for (int i = 0; i < 11; i++)
            s.Reserved1[i] = Take(src, at + DescOffset("Reserved1") + (i * 4));
        ReadPixelFormat(src, at + DescOffset("PixelFormat"), s.Format);
        ReadCaps(src, at + DescOffset("Caps"), s.Caps);
        s.Reserved2 = Take(src, at + DescOffset("Reserved2"));
    }

    static void WritePixelFormat(byte[] dest, int at, PixelFormat p)
    {
        Put(dest, at + PixelOffset("Size"), p.Size);
        Put(dest, at + PixelOffset("Flags"), p.Flags);
        Put(dest, at + PixelOffset("FourCC"), p.FourCc);
        Put(dest, at + PixelOffset("RGBBitCount"), p.RgbBitCount);
        Put(dest, at + PixelOffset("RBitMask"), p.R);
        Put(dest, at + PixelOffset("GBitMask"), p.G);
        Put(dest, at + PixelOffset("BBitMask"), p.B);
        Put(dest, at + PixelOffset("RGBAlphaBitMask"), p.A);
    }

    static void ReadPixelFormat(byte[] src, int at, PixelFormat p)
    {
        p.Size = Take(src, at + PixelOffset("Size"));
        p.Flags = Take(src, at + PixelOffset("Flags"));
        p.FourCc = Take(src, at + PixelOffset("FourCC"));
        p.RgbBitCount = Take(src, at + PixelOffset("RGBBitCount"));
        p.R = Take(src, at + PixelOffset("RBitMask"));
        p.G = Take(src, at + PixelOffset("GBitMask"));
        p.B = Take(src, at + PixelOffset("BBitMask"));
        p.A = Take(src, at + PixelOffset("RGBAlphaBitMask"));
    }

    static void WriteCaps(byte[] dest, int at, Caps c)
    {
        Put(dest, at + CapsOffset("Caps1"), c.Caps1);
        Put(dest, at + CapsOffset("Caps2"), c.Caps2);
        Put(dest, at + CapsOffset("Reserved"), c.Reserved[0]);
        Put(dest, at + CapsOffset("Reserved") + 4, c.Reserved[1]);
    }

    static void ReadCaps(byte[] src, int at, Caps c)
    {
        c.Caps1 = Take(src, at + CapsOffset("Caps1"));
        c.Caps2 = Take(src, at + CapsOffset("Caps2"));
        c.Reserved[0] = Take(src, at + CapsOffset("Reserved"));
        c.Reserved[1] = Take(src, at + CapsOffset("Reserved") + 4);
    }

    static void Put(byte[] dest, int at, uint value)
    {
        dest[at] = (byte)value;
        dest[at + 1] = (byte)(value >> 8);
        dest[at + 2] = (byte)(value >> 16);
        dest[at + 3] = (byte)(value >> 24);
    }

    static uint Take(byte[] src, int at) =>
        src[at] | ((uint)src[at + 1] << 8) | ((uint)src[at + 2] << 16) | ((uint)src[at + 3] << 24);
}
