namespace HCDE.Client;

/// <summary>Native <c>HCDEViewPainSmoothScale</c> and strength clamp from <c>r_view_pain_smooth.cpp</c>.</summary>
public static class VmViewPainSmooth
{
    public static float ClampStrength(float strength) => Math.Clamp(strength, 0f, 1f);

    public static float Scale(int rawDamageCount, float strength)
    {
        var clampedDc = Math.Clamp(rawDamageCount, 0, 100);
        var t = clampedDc * 0.01f;
        var linear = t;
        var smooth = t * t * (3.0f - 2.0f * t);
        var s = ClampStrength(strength);
        return linear + (smooth - linear) * s;
    }
}
