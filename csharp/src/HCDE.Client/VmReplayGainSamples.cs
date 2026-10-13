namespace HCDE.Client;

public sealed class VmReplayGainSamples
{
    private readonly List<float> left = [];
    private readonly List<float> right = [];
    public IReadOnlyList<float> Left { get; }
    public IReadOnlyList<float> Right { get; }
    public int Channels { get; }
    public int SampleRate { get; }

    public VmReplayGainSamples(int sampleRate, int channels)
    {
        if (sampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (channels is not (1 or 2)) throw new ArgumentOutOfRangeException(nameof(channels));
        SampleRate = sampleRate;
        Channels = channels;
        Left = left.AsReadOnly();
        Right = right.AsReadOnly();
    }

    public bool ExceedsAnalysisLimit => left.Count / (float)SampleRate > 8 * 60;

    public void AppendInt16(ReadOnlySpan<short> samples)
    {
        for (var i = 0; i < samples.Length / Channels; i++)
        {
            left.Add(samples[i * Channels]);
            if (Channels == 2) right.Add(samples[i * Channels + 1]);
        }
    }

    public void AppendFloat(ReadOnlySpan<float> samples)
    {
        for (var i = 0; i < samples.Length / Channels; i++)
        {
            left.Add(samples[i * Channels] * 32768f);
            if (Channels == 2) right.Add(samples[i * Channels + 1] * 32768f);
        }
    }
}
