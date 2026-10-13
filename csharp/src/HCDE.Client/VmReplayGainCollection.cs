namespace HCDE.Client;

public static class VmReplayGainCollection
{
    public static float Analyze(VmMusicStreamFormat format, VmMusicFloatFill fillFloat, VmMusicInt16Fill fillInt16)
        => AnalyzeCollected(Collect(format, fillFloat, fillInt16));

    public static VmReplayGainSamples Collect(VmMusicStreamFormat format, VmMusicFloatFill fillFloat,
        VmMusicInt16Fill fillInt16)
    {
        ArgumentNullException.ThrowIfNull(fillFloat);
        ArgumentNullException.ThrowIfNull(fillInt16);
        var samples = new VmReplayGainSamples(format.SampleRate, format.Channels);
        var sampleBytes = format.IsFloat ? 4 : 2;
        if (format.BufferBytes <= 0 || format.BufferBytes % (sampleBytes * format.Channels) != 0)
            throw new ArgumentOutOfRangeException(nameof(format), "Analysis requires a positive, frame-aligned buffer.");
        var sampleCount = format.BufferBytes / sampleBytes;
        if (format.IsFloat)
        {
            var buffer = new float[sampleCount];
            while (fillFloat(buffer))
            {
                samples.AppendFloat(buffer);
                if (samples.ExceedsAnalysisLimit) break;
            }
        }
        else
        {
            var buffer = new short[sampleCount];
            while (fillInt16(buffer))
            {
                samples.AppendInt16(buffer);
                if (samples.ExceedsAnalysisLimit) break;
            }
        }
        return samples;
    }

    public static float AnalyzeCollected(VmReplayGainSamples samples)
    {
        ArgumentNullException.ThrowIfNull(samples);
        var analyzer = new VmReplayGainAnalyzer(samples.SampleRate);
        analyzer.Analyze(samples.Left.ToArray(), samples.Right.ToArray(), samples.Channels);
        return analyzer.GetTitleGain();
    }
}
