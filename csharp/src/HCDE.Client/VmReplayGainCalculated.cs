namespace HCDE.Client;

public static class VmReplayGainCalculated
{
    public static float Apply(string hash, float decibels, VmReplayGainCache cache,
        Action<float> setMusicVolume, Action saveGains)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(setMusicVolume);
        ArgumentNullException.ThrowIfNull(saveGains);
        cache.Store(hash, decibels);
        var amplitude = VmReplayGain.DecibelsToAmplitude(decibels);
        setMusicVolume(amplitude);
        saveGains();
        return amplitude;
    }

    public static float AnalyzeAndApply(string hash, VmMusicStreamFormat format,
        VmMusicFloatFill fillFloat, VmMusicInt16Fill fillInt16, VmReplayGainCache cache,
        Action<float> setMusicVolume, Action saveGains)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(setMusicVolume);
        ArgumentNullException.ThrowIfNull(saveGains);
        var gain = VmReplayGainCollection.Analyze(format, fillFloat, fillInt16);
        return Apply(hash, gain, cache, setMusicVolume, saveGains);
    }
}
