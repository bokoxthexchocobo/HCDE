using System.Buffers.Binary;
using System.Security.Cryptography;
namespace HCDE.Client;

/// <summary>Native <c>imgztexture.cpp</c> paletted decode.</summary>
public static class VmImgzTextureImage
{
    public const int HeaderSize = 24;

    public static bool TryCreate(ReadOnlySpan<byte> lump, out VmImgzHeader header, out bool isAlpha)
    {
        header = default;
        isAlpha = true;
        if (lump.Length < HeaderSize)
            return false;
        if (!lump.StartsWith("IMGZ"u8))
            return false;
        header = VmImgzHeader.Read(lump);
        isAlpha = !VmImgzPaletteCatalog.IsKnownPaletteFile(lump);
        return header.Width > 0 && header.Height > 0;
    }

    public static byte[] CreatePalettedPixels(ReadOnlySpan<byte> lump, VmImgzHeader header, bool isAlpha, bool luminanceConversion = false)
    {
        var remap = VmImageHelpers.GetRemap(luminanceConversion, isAlpha);
        var pixels = new byte[header.Width * header.Height];
        var data = lump[HeaderSize..];
        var destAdvance = header.Height;
        var destRewind = header.Width * header.Height - 1;
        var dest = 0;
        if (header.Compression == 0)
        {
            for (var y = header.Height; y > 0; y--)
            {
                for (var x = header.Width; x > 0; x--)
                {
                    pixels[dest] = remap[data[0]];
                    dest += destAdvance;
                    data = data[1..];
                }

                dest -= destRewind;
            }

            return pixels;
        }

        var runLength = 0;
        var setLength = 0;
        byte setValue = 0;
        for (var y = header.Height; y > 0; y--)
        {
            for (var x = header.Width; x > 0;)
            {
                if (runLength != 0)
                {
                    pixels[dest] = remap[data[0]];
                    dest += destAdvance;
                    data = data[1..];
                    x--;
                    runLength--;
                }
                else if (setLength != 0)
                {
                    pixels[dest] = setValue;
                    dest += destAdvance;
                    x--;
                    setLength--;
                }
                else
                {
                    var code = (sbyte)data[0];
                    data = data[1..];
                    if (code >= 0)
                        runLength = code + 1;
                    else if (code != -128)
                    {
                        setLength = (-code) + 1;
                        setValue = remap[data[0]];
                        data = data[1..];
                    }
                }
            }

            dest -= destRewind;
        }

        return pixels;
    }
}

public readonly struct VmImgzHeader
{
    public ushort Width { get; init; }
    public ushort Height { get; init; }
    public short LeftOffset { get; init; }
    public short TopOffset { get; init; }
    public byte Compression { get; init; }

    public static VmImgzHeader Read(ReadOnlySpan<byte> lump) => new()
    {
        Width = BinaryPrimitives.ReadUInt16LittleEndian(lump[4..]),
        Height = BinaryPrimitives.ReadUInt16LittleEndian(lump[6..]),
        LeftOffset = BinaryPrimitives.ReadInt16LittleEndian(lump[8..]),
        TopOffset = BinaryPrimitives.ReadInt16LittleEndian(lump[10..]),
        Compression = lump[12],
    };
}

/// <summary>Seam: native <c>checkIMGZPalette</c> MD5 table from <c>md5check.cpp</c>.</summary>
public static class VmImgzPaletteCatalog
{
    public static bool IsKnownPaletteFile(ReadOnlySpan<byte> lump)
    {
        var hash = Convert.ToHexString(SHA256.HashData(lump)).ToLowerInvariant();
        return KnownHashes.Contains(hash);
    }

    internal static readonly HashSet<string> KnownHashes = new(StringComparer.Ordinal);
}
