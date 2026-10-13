namespace HCDE.Client;

public interface IVmReplayGainDecoder : IDisposable
{
    bool Start(int subsong, bool loop);
    VmMusicStreamFormat GetFormat();
    bool FillFloat(Span<float> samples);
    bool FillInt16(Span<short> samples);
}

public static class VmReplayGainDecoder
{
    public static float? Calculate(Func<IVmReplayGainDecoder?> openDecoder)
    {
        ArgumentNullException.ThrowIfNull(openDecoder);
        VmReplayGainSamples samples;
        using (var decoder = openDecoder())
        {
            if (decoder is null || !decoder.Start(0, false)) return null;
            var format = decoder.GetFormat();
            if (format.BufferBytes == 0) return null;
            samples = VmReplayGainCollection.Collect(format, decoder.FillFloat, decoder.FillInt16);
        }
        return VmReplayGainCollection.AnalyzeCollected(samples);
    }
}
