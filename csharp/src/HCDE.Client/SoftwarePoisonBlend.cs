namespace HCDE.Client;

/// <summary>Native player poison view tint from V_AddPlayerBlend.</summary>
public static class SoftwarePoisonBlend
{
    internal static (float R, float G, float B, float Alpha) GetTint(int poisonCount, bool paletteFlash)
    {
        if (poisonCount is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(poisonCount));
        var count = Math.Min(poisonCount, 64);
        return paletteFlash ? (44f, 92f, 36f, ((count + 7) >> 3) * 0.1f)
            : (0.04f * 255, 0.2571f * 255, 0, count / 93.2571428571f);
    }

    public static void Apply(SoftwareFrame frame, int poisonCount, bool paletteFlash = false)
    {
        var (red, green, blue, alpha) = GetTint(poisonCount, paletteFlash);
        if (alpha == 0) return;
        var inverse = 1 - alpha;
        for (var index = 0; index < frame.Pixels.Length; index += 3)
        {
            frame.Pixels[index] = (byte)(frame.Pixels[index] * inverse + red * alpha);
            frame.Pixels[index + 1] = (byte)(frame.Pixels[index + 1] * inverse + green * alpha);
            frame.Pixels[index + 2] = (byte)(frame.Pixels[index + 2] * inverse + blue * alpha);
        }
    }
}
