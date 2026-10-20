namespace HCDE.Client;

/// <summary>Native <c>bitmap.cpp</c> ice palette and <c>iCopyColors</c> for RGB→BGRA.</summary>
public static class VmBitmapColorCopy
{
    public const int BlendBits = 12;

    public static ReadOnlySpan<byte> IcePaletteRed => IcePaletteStorage[0];
    public static ReadOnlySpan<byte> IcePaletteGreen => IcePaletteStorage[1];
    public static ReadOnlySpan<byte> IcePaletteBlue => IcePaletteStorage[2];

    private static readonly byte[][] IcePaletteStorage =
    {
        [10, 15, 20, 30, 40, 50, 59, 69, 79, 89, 99, 109, 118, 128, 138, 148],
        [8, 15, 16, 26, 36, 46, 57, 67, 77, 87, 97, 107, 118, 128, 138, 148],
        [18, 26, 36, 46, 57, 67, 78, 88, 99, 109, 120, 130, 141, 151, 162, 172],
    };

    public static void CopyRgbToBgra(
        Span<byte> output,
        ReadOnlySpan<byte> input,
        int count,
        int inputStep,
        VmBitmapCopyInfo? info = null)
    {
        var blend = info?.Blend ?? VmBitmapBlendMode.None;
        var outputOffset = 0;
        var inputOffset = 0;
        for (var index = 0; index < count; index++)
        {
            var r = input[inputOffset];
            var g = input[inputOffset + 1];
            var b = input[inputOffset + 2];
            var a = (byte)255;
            switch (blend)
            {
                case VmBitmapBlendMode.None:
                    WriteBgra(output, outputOffset, b, g, r, a);
                    break;
                case VmBitmapBlendMode.IceMap:
                    var gray = VmImageHelpers.Luminance(r, g, b) >> 4;
                    WriteBgra(output, outputOffset, IcePaletteStorage[2][gray], IcePaletteStorage[1][gray], IcePaletteStorage[0][gray], a);
                    break;
                case VmBitmapBlendMode.Modulate:
                    r = (byte)((r * info!.BlendColor[0]) >> BlendBits);
                    g = (byte)((g * info.BlendColor[1]) >> BlendBits);
                    b = (byte)((b * info.BlendColor[2]) >> BlendBits);
                    WriteBgra(output, outputOffset, b, g, r, a);
                    break;
                case VmBitmapBlendMode.Overlay:
                    r = (byte)((r * info!.BlendColor[3] + info.BlendColor[0]) >> BlendBits);
                    g = (byte)((g * info.BlendColor[3] + info.BlendColor[1]) >> BlendBits);
                    b = (byte)((b * info.BlendColor[3] + info.BlendColor[2]) >> BlendBits);
                    WriteBgra(output, outputOffset, b, g, r, a);
                    break;
                default:
                    if (blend >= VmBitmapBlendMode.Desaturate1 && blend <= VmBitmapBlendMode.Desaturate31)
                    {
                        var factor = (int)blend - (int)VmBitmapBlendMode.Desaturate1 + 1;
                        var luminance = VmImageHelpers.Luminance(r, g, b);
                        r = (byte)((r * (31 - factor) + luminance * factor) / 31);
                        g = (byte)((g * (31 - factor) + luminance * factor) / 31);
                        b = (byte)((b * (31 - factor) + luminance * factor) / 31);
                        WriteBgra(output, outputOffset, b, g, r, a);
                    }
                    else if (blend >= VmBitmapBlendMode.SpecialColormap1 && info?.SpecialColormap is not null)
                    {
                        var grayIndex = Math.Clamp(VmImageHelpers.Luminance(r, g, b), 0, 255);
                        var color = info.SpecialColormap[grayIndex];
                        WriteBgra(output, outputOffset, color.B, color.G, color.R, a);
                    }
                    else
                        throw new NotSupportedException($"Blend mode {blend} is not supported.");
                    break;
            }

            outputOffset += 4;
            inputOffset += inputStep;
        }
    }

    private static void WriteBgra(Span<byte> output, int offset, byte b, byte g, byte r, byte a)
    {
        output[offset] = b;
        output[offset + 1] = g;
        output[offset + 2] = r;
        output[offset + 3] = a;
    }
}

public enum VmBitmapBlendMode
{
    None = 0,
    IceMap = 1,
    Desaturate1 = 2,
    Desaturate31 = 32,
    SpecialColormap1 = 33,
    Modulate = -1,
    Overlay = -2,
}

public sealed class VmBitmapCopyInfo
{
    public VmBitmapBlendMode Blend { get; init; } = VmBitmapBlendMode.None;
    public int[] BlendColor { get; init; } = [4096, 4096, 4096, 4096];
    public VmPaletteEntry[]? SpecialColormap { get; init; }
}
