namespace HCDE.Client;

public static class VmReplayGain
{
    public static float AmplitudeToDecibels(float amplitude) => 20f * MathF.Log10(amplitude);
    public static float DecibelsToAmplitude(float decibels) => MathF.Pow(10f, decibels / 20f);

    public static float SetManualGain(string hash, float decibels, Action<string, float> storeGain,
        Action saveGains, Action<float> setMusicVolume)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(storeGain);
        ArgumentNullException.ThrowIfNull(saveGains);
        ArgumentNullException.ThrowIfNull(setMusicVolume);
        if (decibels > 10) decibels = 10;
        storeGain(hash, decibels);
        saveGains();
        var amplitude = DecibelsToAmplitude(decibels);
        setMusicVolume(amplitude);
        return amplitude;
    }
}
