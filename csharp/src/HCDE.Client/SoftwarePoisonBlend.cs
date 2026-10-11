namespace HCDE.Client;

/// <summary>Native player poison view tint from V_AddPlayerBlend.</summary>
public static class SoftwarePoisonBlend
{
    public static void Apply(SoftwareFrame frame, int poisonCount, bool paletteFlash = false)
    {
        if (poisonCount is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(poisonCount));
        if (poisonCount == 0) return;
        var count = Math.Min(poisonCount, 64);
        var alpha = paletteFlash ? ((count + 7) >> 3) * 0.1f : count / 93.2571428571f;
        var red = paletteFlash ? 44f : 0.04f * 255;
        var green = paletteFlash ? 92f : 0.2571f * 255;
        var blue = paletteFlash ? 36f : 0;
        var inverse = 1 - alpha;
        for (var index = 0; index < frame.Pixels.Length; index += 3)
        {
            frame.Pixels[index] = (byte)(frame.Pixels[index] * inverse + red * alpha);
            frame.Pixels[index + 1] = (byte)(frame.Pixels[index + 1] * inverse + green * alpha);
            frame.Pixels[index + 2] = (byte)(frame.Pixels[index + 2] * inverse + blue * alpha);
        }
    }
}
