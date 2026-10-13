namespace HCDE.Client;

public static class VmReplayGainLookup
{
    public static bool ApplyCached(bool enabled, VmReplayGainCache cache, Action<float> setMusicVolume,
        Action updateFluidGain, Action updateModuleGain, Func<string?> prepareHash,
        Action readGains, Action<string> setCurrentHash, Func<bool>? openMusic = null)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(setMusicVolume);
        ArgumentNullException.ThrowIfNull(updateFluidGain);
        ArgumentNullException.ThrowIfNull(updateModuleGain);
        ArgumentNullException.ThrowIfNull(prepareHash);
        ArgumentNullException.ThrowIfNull(readGains);
        ArgumentNullException.ThrowIfNull(setCurrentHash);
        setMusicVolume(1);
        updateFluidGain();
        updateModuleGain();
        if (!enabled) return false;
        if (openMusic is not null && !openMusic()) return false;
        readGains();
        var hash = prepareHash();
        if (string.IsNullOrEmpty(hash)) return false;
        setCurrentHash(hash);
        if (!cache.TryGet(hash, out var gain)) return false;
        setMusicVolume(VmReplayGain.DecibelsToAmplitude(gain));
        return true;
    }
}
