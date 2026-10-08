namespace HCDE.Client;

/// <summary>Converts represented mono PCM with linear interpolation.</summary>
public static class PcmResampler
{
    public static short[] Convert(ReadOnlySpan<short> samples, int sourceRate, int targetRate, float pitch)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetRate);
        if (!float.IsFinite(pitch) || pitch <= 0) throw new ArgumentOutOfRangeException(nameof(pitch));
        if (pitch == 1) return Convert(samples, sourceRate, targetRate);
        if (samples.IsEmpty) return [];
        var step = sourceRate * (double)pitch / targetRate;
        var count = Math.Ceiling(samples.Length / step);
        if (count > Array.MaxLength) throw new ArgumentOutOfRangeException(nameof(pitch), "Converted PCM exceeds supported buffer size.");
        var output = new short[(int)count];
        for (var index = 0; index < output.Length; index++)
        {
            var position = Math.Min(index * step, samples.Length - 1d);
            var lower = (int)position;
            var upper = Math.Min(lower + 1, samples.Length - 1);
            var value = samples[lower] + (samples[upper] - samples[lower]) * (position - lower);
            output[index] = (short)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), short.MinValue, short.MaxValue);
        }
        return output;
    }

    public static short[] Convert(ReadOnlySpan<short> samples, int sourceRate, int targetRate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetRate);
        if (samples.IsEmpty) return [];
        if (sourceRate == targetRate) return samples.ToArray();
        var count = ((long)samples.Length * targetRate + sourceRate - 1) / sourceRate;
        if (count > Array.MaxLength) throw new ArgumentOutOfRangeException(nameof(targetRate), "Converted PCM exceeds supported buffer size.");
        var output = new short[(int)count];
        for (var index = 0; index < output.Length; index++)
        {
            var position = (long)index * sourceRate;
            var lower = (int)(position / targetRate);
            var upper = Math.Min(lower + 1, samples.Length - 1);
            var fraction = (position % targetRate) / (double)targetRate;
            var value = samples[lower] + (samples[upper] - samples[lower]) * fraction;
            output[index] = (short)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), short.MinValue, short.MaxValue);
        }
        return output;
    }
}
