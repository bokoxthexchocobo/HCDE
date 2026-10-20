using System.Buffers.Binary;

namespace HCDE.Client;

/// <summary>Native <c>ddstexture.cpp</c> validation and uncompressed RGB paletted decode.</summary>
public static class VmDdsTextureImage
{
    private const uint MagicDds = 0x20534444;
    private const uint DdsdCaps = 0x00000001;
    private const uint DdsdHeight = 0x00000002;
    private const uint DdsdWidth = 0x00000004;
    private const uint DdsdPitch = 0x00000008;
    private const uint DdsdPixelFormat = 0x00001000;
    private const uint DdpfRgb = 0x00000040;
    private const uint DdpfAlphaPixels = 0x00000001;
    private const uint DdpfFourCc = 0x00000004;
    private const uint DdsdLinearSize = 0x00080000;
    private const uint FourCcDxt1 = 0x31545844;

    public static bool TryCreate(ReadOnlySpan<byte> lump, out VmDdsInfo info)
    {
        info = default;
        if (!TryReadHeader(lump, out var desc))
            return false;

        if ((desc.PixelFormatFlags & VmDdsPixelFormatFlags.FourCc) != 0)
        {
            if (desc.FourCc is not (FourCcDxt1 or 0x32545844 or 0x33545844 or 0x34545844 or 0x35545844))
                return false;
            if ((desc.Flags & DdsdLinearSize) == 0)
                return false;
        }
        else if ((desc.PixelFormatFlags & VmDdsPixelFormatFlags.Rgb) != 0)
        {
            var bytesPerPixel = desc.RgbBitCount >> 3;
            if (bytesPerPixel is < 1 or > 4)
                return false;
            if ((desc.Flags & DdsdPitch) != 0 && desc.Pitch <= 0)
                return false;
        }
        else
            return false;

        info = desc;
        return true;
    }

    public static byte[] CreatePalettedPixels(ReadOnlySpan<byte> lump, VmDdsInfo info, bool luminanceConversion = false)
    {
        var pixelMode = luminanceConversion ? 1 : 0;
        var pixels = new byte[info.Width * info.Height];
        if (info.FourCc != 0)
        {
            if (info.FourCc != FourCcDxt1)
                throw new NotSupportedException("Only DXT1 blocks are ported in this chunk.");
            DecompressDxt1(lump, info, pixels, pixelMode);
            return pixels;
        }

        ReadRgb(lump, info, pixels, pixelMode);
        return pixels;
    }

    private static bool TryReadHeader(ReadOnlySpan<byte> lump, out VmDdsInfo info)
    {
        if (lump.Length < 4 + 124)
        {
            info = default;
            return false;
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(lump) != MagicDds)
        {
            info = default;
            return false;
        }
        var desc = lump[4..];
        var size = BinaryPrimitives.ReadUInt32LittleEndian(desc);
        if (size != 124 && size != MagicDds)
        {
            info = default;
            return false;
        }
        var flags = BinaryPrimitives.ReadUInt32LittleEndian(desc[4..]);
        var required = DdsdCaps | DdsdPixelFormat | DdsdWidth | DdsdHeight;
        if ((flags & required) != required)
        {
            info = default;
            return false;
        }
        var width = BinaryPrimitives.ReadUInt32LittleEndian(desc[8..]);
        var height = BinaryPrimitives.ReadUInt32LittleEndian(desc[12..]);
        if (width == 0 || height == 0)
        {
            info = default;
            return false;
        }
        var pf = desc[76..];
        var pfSize = BinaryPrimitives.ReadUInt32LittleEndian(pf);
        if (pfSize != 32)
        {
            info = default;
            return false;
        }
        var pfFlags = BinaryPrimitives.ReadUInt32LittleEndian(pf[4..]);
        var rgbBitCount = BinaryPrimitives.ReadUInt32LittleEndian(pf[8..]);
        var fourCc = 0u;
        if ((pfFlags & DdpfFourCc) != 0)
            fourCc = rgbBitCount;
        info = new VmDdsInfo
        {
            Width = width,
            Height = height,
            Flags = flags,
            Pitch = BinaryPrimitives.ReadInt32LittleEndian(desc[16..]),
            LinearSize = BinaryPrimitives.ReadUInt32LittleEndian(desc[16..]),
            PixelFormatFlags = (VmDdsPixelFormatFlags)pfFlags,
            RgbBitCount = rgbBitCount,
            FourCc = fourCc,
            RMask = BinaryPrimitives.ReadUInt32LittleEndian(pf[12..]),
            GMask = BinaryPrimitives.ReadUInt32LittleEndian(pf[16..]),
            BMask = BinaryPrimitives.ReadUInt32LittleEndian(pf[20..]),
            AMask = BinaryPrimitives.ReadUInt32LittleEndian(pf[24..])
        };
        return true;
    }

    private static void ReadRgb(ReadOnlySpan<byte> lump, VmDdsInfo info, Span<byte> buffer, int pixelMode)
    {
        var format = (int)(info.RgbBitCount >> 3);
        CalcBitShift(info.RMask, out var rShiftL, out var rShiftR);
        CalcBitShift(info.GMask, out var gShiftL, out var gShiftR);
        CalcBitShift(info.BMask, out var bShiftL, out var bShiftR);
        CalcBitShift(info.AMask, out var aShiftL, out var aShiftR);
        var amask = info.AMask == 0 ? 0 : 0x80000000u >> aShiftL;
        var pitch = (info.Flags & DdsdPitch) != 0 ? info.Pitch : (int)((info.Width * format + 3) & ~3);
        var data = lump[(4 + 124)..];
        for (var y = (int)info.Height; y > 0; y--)
        {
            var line = data[..pitch];
            data = data[pitch..];
            var buffp = 0;
            var pixelp = (int)(y - 1);
            for (var x = (int)info.Width; x > 0; x--)
            {
                uint c = format switch
                {
                    4 => BinaryPrimitives.ReadUInt32LittleEndian(line[buffp..]),
                    2 => BinaryPrimitives.ReadUInt16LittleEndian(line[buffp..]),
                    3 => line[buffp] | (uint)(line[buffp + 1] << 8) | (uint)(line[buffp + 2] << 16),
                    _ => line[buffp]
                };
                buffp += format;
                if (amask == 0 || (c & amask) != 0)
                {
                    var r = Expand(c & info.RMask, rShiftL, rShiftR);
                    var g = Expand(c & info.GMask, gShiftL, gShiftR);
                    var b = Expand(c & info.BMask, bShiftL, bShiftR);
                    var a = Expand(c & info.AMask, aShiftL, aShiftR);
                    buffer[pixelp] = VmImageHelpers.RgbToPalette(pixelMode == 1, r, g, b, a);
                }
                else
                    buffer[pixelp] = 0;
                pixelp += (int)info.Height;
            }
        }
    }

    private static void DecompressDxt1(ReadOnlySpan<byte> lump, VmDdsInfo info, Span<byte> buffer, int pixelMode)
    {
        var blockLineLen = (int)(((info.Width + 3) >> 2) << 3);
        var data = lump[(4 + 124)..];
        Span<int> color = stackalloc int[12];
        Span<byte> palcol = stackalloc byte[4];
        for (var oy = 0; oy < info.Height; oy += 4)
        {
            var blockLine = data[..blockLineLen];
            data = data[blockLineLen..];
            var block = 0;
            for (var ox = 0; ox < info.Width; ox += 4)
            {
                var color16 = new ushort[]
                {
                    BinaryPrimitives.ReadUInt16LittleEndian(blockLine[(block + 0)..]),
                    BinaryPrimitives.ReadUInt16LittleEndian(blockLine[(block + 2)..])
                };
                for (var i = 1; i >= 0; i--)
                {
                    color[i * 3] = ((color16[i] & 0xF800) >> 8) | (color16[i] >> 13);
                    color[i * 3 + 1] = ((color16[i] & 0x07E0) >> 3) | ((color16[i] & 0x0600) >> 9);
                    color[i * 3 + 2] = ((color16[i] & 0x001F) << 3) | ((color16[i] & 0x001C) >> 2);
                }

                if (color16[0] > color16[1])
                {
                    color[6] = (color[0] + color[0] + color[3] + 1) / 3;
                    color[7] = (color[1] + color[1] + color[4] + 1) / 3;
                    color[8] = (color[2] + color[2] + color[5] + 1) / 3;
                    color[9] = (color[3] + color[6] + color[6] + 1) / 3;
                    color[10] = (color[4] + color[7] + color[7] + 1) / 3;
                    color[11] = (color[5] + color[8] + color[8] + 1) / 3;
                }
                else
                {
                    color[6] = (color[0] + color[3]) / 2;
                    color[7] = (color[1] + color[4]) / 2;
                    color[8] = (color[2] + color[5]) / 2;
                    color[9] = color[10] = color[11] = 0;
                }

                for (var i = 3; i >= 0; i--)
                    palcol[i] = VmImageHelpers.RgbToPalette(pixelMode == 1, color[i * 3], color[i * 3 + 1], color[i * 3 + 2]);

                for (var y = 0; y < 4; y++)
                {
                    if (oy + y >= info.Height)
                        break;
                    var yslice = blockLine[block + 4 + y];
                    for (var x = 0; x < 4; x++)
                    {
                        if (ox + x >= info.Width)
                            break;
                        var ci = (yslice >> (x + x)) & 3;
                        buffer[oy + y + (ox + x) * (int)info.Height] = palcol[ci];
                    }
                }

                block += 8;
            }
        }
    }

    private static int Expand(uint masked, byte shiftL, byte shiftR)
    {
        var c = masked << shiftL;
        c |= c >> shiftR;
        return (int)(c >> 24);
    }

    public static void CalcBitShift(uint mask, out byte shiftL, out byte shiftR)
    {
        if (mask == 0)
        {
            shiftL = shiftR = 0;
            return;
        }

        byte shift = 0;
        while ((mask & 0x80000000) == 0)
        {
            mask <<= 1;
            shift++;
        }

        shiftL = shift;
        shift = 0;
        while ((mask & 0x80000000) != 0)
        {
            mask <<= 1;
            shift++;
        }

        shiftR = shift;
    }
}

public readonly struct VmDdsInfo
{
    public uint Width { get; init; }
    public uint Height { get; init; }
    public uint Flags { get; init; }
    public int Pitch { get; init; }
    public uint LinearSize { get; init; }
    public VmDdsPixelFormatFlags PixelFormatFlags { get; init; }
    public uint RgbBitCount { get; init; }
    public uint FourCc { get; init; }
    public uint RMask { get; init; }
    public uint GMask { get; init; }
    public uint BMask { get; init; }
    public uint AMask { get; init; }
}

[Flags]
public enum VmDdsPixelFormatFlags : uint
{
    AlphaPixels = 0x00000001,
    FourCc = 0x00000004,
    Rgb = 0x00000040
}
