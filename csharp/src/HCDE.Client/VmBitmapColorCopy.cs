namespace HCDE.Client;

/// <summary>Native <c>bitmap.cpp</c> ice palette and <c>iCopyColors</c> for RGB→BGRA.</summary>
public static class VmBitmapColorCopy
{
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
        VmBitmapBlendMode blend,
        byte transparentR = 0,
        byte transparentG = 0,
        byte transparentB = 0)
    {
        var outputOffset = 0;
        var inputOffset = 0;
        for (var index = 0; index < count; index++)
        {
            var r = input[inputOffset];
            var g = input[inputOffset + 1];
            var b = input[inputOffset + 2];
            var a = (byte)255;
            if (blend == VmBitmapBlendMode.None)
            {
                output[outputOffset] = b;
                output[outputOffset + 1] = g;
                output[outputOffset + 2] = r;
                output[outputOffset + 3] = a;
            }
            else if (blend == VmBitmapBlendMode.IceMap)
            {
                var gray = VmImageHelpers.Luminance(r, g, b) >> 4;
                output[outputOffset] = IcePaletteStorage[2][gray];
                output[outputOffset + 1] = IcePaletteStorage[1][gray];
                output[outputOffset + 2] = IcePaletteStorage[0][gray];
                output[outputOffset + 3] = a;
            }
            else
                throw new NotSupportedException($"Blend mode {blend} is not ported (native special colormap/desaturate seam).");

            outputOffset += 4;
            inputOffset += inputStep;
        }
    }
}

public enum VmBitmapBlendMode
{
    None = 0,
    IceMap = 1,
}
