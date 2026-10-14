using HCDE.Playsim;

namespace HCDE.Client;

/// <summary>Supported V_AddPlayerBlend sources, accumulated with native V_AddBlend weights.</summary>
public static class SoftwarePlayerBlend
{
    public static void Apply(SoftwareFrame frame, PlayerPawn player, double bloodFadeScalar = 1,
        bool smoothPain = false, float smoothStrength = 0.6f, bool poisonPaletteFlash = false,
        double powerupFadeScalar = 1, double pickupFadeScalar = 1)
    {
        if (!double.IsFinite(powerupFadeScalar) || powerupFadeScalar < 0)
            throw new ArgumentOutOfRangeException(nameof(powerupFadeScalar));
        var pickupAlpha = GetPickupAlpha(player.BonusCount, pickupFadeScalar);
        var color = player.GetPainFlash();
        var pain = SoftwarePainBlend.GetAlpha(player.DamageCount, color, bloodFadeScalar, smoothPain, smoothStrength);
        var poison = SoftwarePoisonBlend.GetTint(player.PoisonCount, poisonPaletteFlash);
        var blend = new PlayerBlendAccumulator();
        var strength = player.GetStrengthBlend();
        blend.Add((strength >> 16) & 255, (strength >> 8) & 255, strength & 255,
            (float)Math.Clamp((strength >> 24) * powerupFadeScalar / 255, 0, 1));
        var pickupColor = player.Level?.PickupColor ?? 0xd7ba45u;
        blend.Add((pickupColor >> 16) & 255, (pickupColor >> 8) & 255, pickupColor & 255, pickupAlpha);
        blend.Add((color >> 16) & 255, (color >> 8) & 255, color & 255, pain / 255f);
        blend.Add(poison.R, poison.G, poison.B, poison.Alpha);
        blend.Apply(frame);
    }

    internal static float GetPickupAlpha(int bonusCount, double fadeScalar)
    {
        if (!double.IsFinite(fadeScalar) || fadeScalar < 0)
            throw new ArgumentOutOfRangeException(nameof(fadeScalar));
        // Only the distinction above 128 matters; saturate before converting huge values to int.
        var count = (int)Math.Min(129, bonusCount * 8.0 * fadeScalar);
        return count > 128 ? 0.5f : count / 255f;
    }
}

internal struct PlayerBlendAccumulator
{
    private float _r, _g, _b, _alpha;

    internal void Add(float r, float g, float b, float alpha)
    {
        if (alpha <= 0) return;
        var total = _alpha + (1 - _alpha) * alpha;
        var oldWeight = _alpha / total;
        _r = _r * oldWeight + r * (1 - oldWeight);
        _g = _g * oldWeight + g * (1 - oldWeight);
        _b = _b * oldWeight + b * (1 - oldWeight);
        _alpha = total;
    }

    internal readonly void Apply(SoftwareFrame frame)
    {
        if (_alpha <= 0) return;
        var inverse = 1 - _alpha;
        for (var i = 0; i < frame.Pixels.Length; i += 3)
        {
            frame.Pixels[i] = (byte)(frame.Pixels[i] * inverse + _r * _alpha);
            frame.Pixels[i + 1] = (byte)(frame.Pixels[i + 1] * inverse + _g * _alpha);
            frame.Pixels[i + 2] = (byte)(frame.Pixels[i + 2] * inverse + _b * _alpha);
        }
    }
}
