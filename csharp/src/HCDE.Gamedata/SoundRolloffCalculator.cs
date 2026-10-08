namespace HCDE.Gamedata;

/// <summary>Calculates the native distance curve independently of an audio backend.</summary>
public static class SoundRolloffCalculator
{
    public static float Calculate(SoundRolloff? rolloff, float distance, ReadOnlySpan<byte> customCurve = default)
    {
        if (!float.IsFinite(distance)) throw new ArgumentOutOfRangeException(nameof(distance));
        if (rolloff is null) return 0;
        if (!float.IsFinite(rolloff.MinDistance) || !float.IsFinite(rolloff.MaxDistance))
            throw new ArgumentException("Rolloff distances must be finite.", nameof(rolloff));
        if (distance <= rolloff.MinDistance) return 1;
        if (rolloff.Type == SoundRolloffType.Log)
        {
            // The native union stores the logarithmic factor in the maximum-distance field.
            var denominator = rolloff.MinDistance + rolloff.MaxDistance * (distance - rolloff.MinDistance);
            var gain = rolloff.MinDistance / denominator;
            if (!float.IsFinite(gain)) throw new ArgumentException("Logarithmic rolloff produces a nonfinite gain.", nameof(rolloff));
            return gain;
        }
        if (distance >= rolloff.MaxDistance) return 0;
        var volume = (rolloff.MaxDistance - distance) / (rolloff.MaxDistance - rolloff.MinDistance);
        if (rolloff.Type == SoundRolloffType.Linear) return volume;
        if (rolloff.Type == SoundRolloffType.Custom && !customCurve.IsEmpty)
        {
            var index = Math.Clamp((int)(customCurve.Length * (1 - volume)), 0, customCurve.Length - 1);
            return customCurve[index] / 127f;
        }
        return (MathF.Pow(10, volume) - 1) / 9;
    }
}
