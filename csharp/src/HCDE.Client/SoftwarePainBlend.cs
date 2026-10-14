namespace HCDE.Client;

/// <summary>Player pain color blend using the native DamageToAlpha curve.</summary>
public static class SoftwarePainBlend
{
    private static ReadOnlySpan<byte> DamageToAlpha =>
    [0,8,16,23,30,36,42,47,53,58,62,67,71,75,79,83,87,90,94,97,100,103,107,109,112,115,118,120,123,125,
     128,130,133,135,137,139,141,143,145,147,149,151,153,155,157,159,160,162,164,165,167,169,170,172,173,175,
     176,178,179,181,182,183,185,186,187,189,190,191,192,194,195,196,197,198,200,201,202,203,204,205,206,207,
     209,210,211,212,213,214,215,216,217,218,219,220,221,221,222,223,224,225,226,227,228,229,229,230,231,
     232,233,234,235,235,236,237];

    internal static int GetAlpha(int damageCount, uint painColor, double fadeScalar,
        bool smoothPain = false, float smoothStrength = 0.6f)
    {
        if (damageCount is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(damageCount));
        if (!double.IsFinite(fadeScalar) || fadeScalar < 0) throw new ArgumentOutOfRangeException(nameof(fadeScalar));
        if (!float.IsFinite(smoothStrength)) throw new ArgumentOutOfRangeException(nameof(smoothStrength));
        var intensity = damageCount * (int)(painColor >> 24) / 255;
        var count = (int)DamageToAlpha[intensity];
        if (smoothPain && damageCount > 0)
        {
            // Native smoothing uses the raw counter, after the color-alpha curve lookup.
            var linear = damageCount * 0.01f;
            var smooth = linear * linear * (3.0f - 2.0f * linear);
            var scaled = linear + (smooth - linear) * Math.Clamp(smoothStrength, 0, 1);
            count = (int)(count * (scaled / linear));
        }
        var alpha = (int)Math.Min(175, count * fadeScalar);
        return alpha;
    }

    public static void Apply(SoftwareFrame frame, int damageCount, uint painColor = 0xffff0000u, double fadeScalar = 1,
        bool smoothPain = false, float smoothStrength = 0.6f)
    {
        var alpha = GetAlpha(damageCount, painColor, fadeScalar, smoothPain, smoothStrength);
        if (alpha == 0) return;
        var inverse = 255 - alpha;
        var red = (int)((painColor >> 16) & 255);
        var green = (int)((painColor >> 8) & 255);
        var blue = (int)(painColor & 255);
        for (var index = 0; index < frame.Pixels.Length; index += 3)
        {
            frame.Pixels[index] = (byte)((frame.Pixels[index] * inverse + red * alpha) / 255);
            frame.Pixels[index + 1] = (byte)((frame.Pixels[index + 1] * inverse + green * alpha) / 255);
            frame.Pixels[index + 2] = (byte)((frame.Pixels[index + 2] * inverse + blue * alpha) / 255);
        }
    }
}
