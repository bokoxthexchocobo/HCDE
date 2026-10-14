namespace HCDE.Client;

/// <summary>Source-over inventory powerup tint, before pain and poison blends.</summary>
public static class SoftwarePowerupBlend
{
    public static void Apply(SoftwareFrame frame, uint color, double fadeScalar = 1)
    {
        if (!double.IsFinite(fadeScalar) || fadeScalar < 0)
            throw new ArgumentOutOfRangeException(nameof(fadeScalar));
        var alpha = (float)Math.Clamp((color >> 24) * fadeScalar / 255, 0, 1);
        if (alpha == 0) return;
        var inverse = 1 - alpha;
        var red = (int)((color >> 16) & 255);
        var green = (int)((color >> 8) & 255);
        var blue = (int)(color & 255);
        for (var i = 0; i < frame.Pixels.Length; i += 3)
        {
            frame.Pixels[i] = (byte)(frame.Pixels[i] * inverse + red * alpha);
            frame.Pixels[i + 1] = (byte)(frame.Pixels[i + 1] * inverse + green * alpha);
            frame.Pixels[i + 2] = (byte)(frame.Pixels[i + 2] * inverse + blue * alpha);
        }
    }
}
