namespace HCDE.Client;

public static class VmMusicSongStart
{
    public static bool Start(VmMusicVolume volume, float songLevel, bool useReplayGain,
        int subsong, bool loop, Action<float> applyRelativeLevel, Action stopSong,
        Action updateFluidGain, Action updateModuleGain, Func<int, bool, bool> startSong,
        Action notifyMusicVolume)
    {
        ArgumentNullException.ThrowIfNull(volume);
        ArgumentNullException.ThrowIfNull(applyRelativeLevel);
        ArgumentNullException.ThrowIfNull(stopSong);
        ArgumentNullException.ThrowIfNull(updateFluidGain);
        ArgumentNullException.ThrowIfNull(updateModuleGain);
        ArgumentNullException.ThrowIfNull(startSong);
        ArgumentNullException.ThrowIfNull(notifyMusicVolume);
        if (volume.StartSongLevel(songLevel, useReplayGain)) applyRelativeLevel(volume.RelativeLevel);
        stopSong();
        updateFluidGain();
        updateModuleGain();
        if (!startSong(subsong, loop)) return false;
        notifyMusicVolume();
        return true;
    }
}
