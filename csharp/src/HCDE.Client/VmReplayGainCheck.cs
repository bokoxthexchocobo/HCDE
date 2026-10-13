namespace HCDE.Client;

public static class VmReplayGainCheck
{
    public static bool Check(bool enabled, bool calculateMissing, VmReplayGainCache cache,
        Action<float> setMusicVolume, Action updateFluidGain, Action updateModuleGain,
        Func<bool> openMusic, Action readGains, Func<string?> prepareHash,
        Action<string> setCurrentHash, Func<float?> calculateGain, Action saveGains)
    {
        ArgumentNullException.ThrowIfNull(openMusic);
        ArgumentNullException.ThrowIfNull(prepareHash);
        ArgumentNullException.ThrowIfNull(calculateGain);
        ArgumentNullException.ThrowIfNull(saveGains);
        string? hash = null;
        var cached = VmReplayGainLookup.ApplyCached(enabled, cache, setMusicVolume,
            updateFluidGain, updateModuleGain, () => hash = prepareHash(), readGains, setCurrentHash, openMusic);
        if (cached) return true;
        if (!calculateMissing || string.IsNullOrEmpty(hash)) return false;
        var gain = calculateGain();
        if (gain is null) return false;
        VmReplayGainCalculated.Apply(hash, gain.Value, cache, setMusicVolume, saveGains);
        return true;
    }
}
