namespace HCDE.Client;

/// <summary>
/// Sums queued PCM into one buffer and clamps to 16-bit. ZMusic is probed and not called.
/// </summary>
public sealed class AudioMixer
{
    private readonly List<short[]> _channels = new();

    public int SampleRate { get; init; } = 11025;
    public bool Muted { get; set; }

    public void Play(ReadOnlySpan<short> pcm)
    {
        if (pcm.IsEmpty)
            return;
        _channels.Add(pcm.ToArray());
    }

    public short[] Mix(int sampleCount)
    {
        if (sampleCount < 0)
            throw new ArgumentOutOfRangeException(nameof(sampleCount));
        var output = new short[sampleCount];
        if (!Muted)
        {
            foreach (var channel in _channels)
            {
                var count = Math.Min(sampleCount, channel.Length);
                for (var i = 0; i < count; i++)
                {
                    var sum = output[i] + channel[i];
                    output[i] = (short)Math.Clamp(sum, short.MinValue, short.MaxValue);
                }
            }
        }

        _channels.Clear();
        return output;
    }
}
