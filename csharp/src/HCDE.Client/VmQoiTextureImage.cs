using System.Buffers.Binary;

namespace HCDE.Client;

/// <summary>Native <c>qoitexture.cpp</c> header gate and paletted decode.</summary>
public static class VmQoiTextureImage
{
    public const int HeaderSize = 14;

    public static bool TryCreate(ReadOnlySpan<byte> lump, out VmQoiHeader header)
    {
        header = default;
        if (lump.Length < HeaderSize + 8)
            return false;
        if (!lump.StartsWith("qoif"u8))
            return false;
        header = VmQoiHeader.Read(lump);
        if (header.Width == 0 || header.Height == 0)
            return false;
        if (header.Channels is < 3 or > 4)
            return false;
        if (header.ColorSpace > 1)
            return false;
        return true;
    }

    public static byte[] CreatePalettedPixels(ReadOnlySpan<byte> lump, VmQoiHeader header, bool luminanceConversion = false)
    {
        var rgba = DecodeToBgra(lump, header);
        var pixels = new byte[header.Width * header.Height];
        var dest = 0;
        var destAdvance = header.Height;
        var destRewind = header.Width * header.Height - 1;
        var data = 0;
        for (var y = header.Height; y > 0; y--)
        {
            for (var x = header.Width; x > 0; x--)
            {
                var b = rgba[data++];
                var g = rgba[data++];
                var r = rgba[data++];
                var a = rgba[data++];
                pixels[dest] = a < 128 ? (byte)0 : VmImageHelpers.RgbToPalette(luminanceConversion, r, g, b);
                dest += destAdvance;
            }

            dest -= destRewind;
        }

        return pixels;
    }

    private static byte[] DecodeToBgra(ReadOnlySpan<byte> lump, VmQoiHeader header)
    {
        const byte QoiOpRgb = 0xfe;
        const byte QoiOpRgba = 0xff;
        const byte QoiMask2 = 0xc0;
        var pixels = new byte[header.Width * header.Height * 4];
        Span<VmQoiColor> index = stackalloc VmQoiColor[64];
        var pe = new VmQoiColor { R = 0, G = 0, B = 0, A = 255 };
        var position = HeaderSize;
        var run = 0;
        var chunksLen = lump.Length - 8;
        var pixel = 0;
        for (var h = 0; h < header.Height; h++)
        {
            for (var w = 0; w < header.Width; w++)
            {
                if (run > 0)
                    run--;
                else if (position < chunksLen)
                {
                    var b1 = lump[position++];
                    if (b1 == QoiOpRgb)
                    {
                        pe.R = lump[position++];
                        pe.G = lump[position++];
                        pe.B = lump[position++];
                    }
                    else if (b1 == QoiOpRgba)
                    {
                        pe.R = lump[position++];
                        pe.G = lump[position++];
                        pe.B = lump[position++];
                        pe.A = lump[position++];
                    }
                    else if ((b1 & QoiMask2) == 0x00)
                    {
                        pe = index[b1];
                    }
                    else if ((b1 & QoiMask2) == 0x40)
                    {
                        pe.R = (byte)(pe.R + ((b1 >> 4) & 0x03) - 2);
                        pe.G = (byte)(pe.G + ((b1 >> 2) & 0x03) - 2);
                        pe.B = (byte)(pe.B + (b1 & 0x03) - 2);
                    }
                    else if ((b1 & QoiMask2) == 0x80)
                    {
                        var b2 = lump[position++];
                        var vg = (b1 & 0x3f) - 32;
                        pe.R = (byte)(pe.R + vg - 8 + ((b2 >> 4) & 0x0f));
                        pe.G = (byte)(pe.G + vg);
                        pe.B = (byte)(pe.B + vg - 8 + (b2 & 0x0f));
                    }
                    else if ((b1 & QoiMask2) == 0xc0)
                        run = b1 & 0x3f;

                    index[pe.Hash() % 64] = pe;
                }

                pixels[pixel++] = pe.B;
                pixels[pixel++] = pe.G;
                pixels[pixel++] = pe.R;
                pixels[pixel++] = pe.A;
            }
        }

        return pixels;
    }
}

public readonly struct VmQoiHeader
{
    public int Width { get; init; }
    public int Height { get; init; }
    public byte Channels { get; init; }
    public byte ColorSpace { get; init; }

    public static VmQoiHeader Read(ReadOnlySpan<byte> lump) => new()
    {
        Width = BinaryPrimitives.ReadInt32BigEndian(lump[4..]),
        Height = BinaryPrimitives.ReadInt32BigEndian(lump[8..]),
        Channels = lump[12],
        ColorSpace = lump[13]
    };
}

internal struct VmQoiColor
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public int Hash() => R * 3 + G * 5 + B * 7 + A * 11;
}
