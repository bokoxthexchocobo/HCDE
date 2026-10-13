namespace HCDE.Client;

public static class VmMusicStreamSamples
{
    public static bool ApplyFloat(Span<float> samples, float musicVolume, bool written)
    {
        if (!written)
        {
            samples.Clear();
            return false;
        }
        if (musicVolume != 1)
            for (var i = 0; i < samples.Length; i++) samples[i] *= musicVolume;
        return true;
    }

    public static bool ConvertInt16(ReadOnlySpan<short> source, Span<float> destination,
        float musicVolume, bool written)
    {
        if (!written)
        {
            destination.Clear();
            return false;
        }
        if (source.Length < destination.Length)
            throw new ArgumentException("Source must contain every output sample.", nameof(source));
        for (var i = 0; i < destination.Length; i++)
            destination[i] = source[i] * musicVolume * (1f / 32768f);
        return true;
    }
}
