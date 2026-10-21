namespace HCDE.Playsim;

/// <summary><c>SoundEngine::RestartChannel</c> and <c>ChangeSoundVolume</c> from <c>s_sound.cpp</c> (649–708, 1023–1044).</summary>
public static class VmSoundEngineRestartUpdate
{
    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;

    public static bool RequiresEvictedFlag(int chanFlags) => (chanFlags & ChanfEvicted) != 0;

    public static bool AbortRestartSingular(bool sfxSingular, bool alreadyPlaying) =>
        sfxSingular && alreadyPlaying;

    public static bool AbortRestartEmptyLump(int lumpnum, int sfxEmpty) => lumpnum == sfxEmpty;

    public static int BuildStartFlags(int chanFlags, int chanfLoop, int chanfArea, int chanfUi, int chanfNopause, int chanfAbstime)
    {
        var startflags = 0;
        if ((chanFlags & chanfLoop) != 0) startflags |= 1;
        if ((chanFlags & chanfArea) != 0) startflags |= 2;
        if ((chanFlags & (chanfUi | chanfNopause)) != 0) startflags |= 4;
        if ((chanFlags & chanfAbstime) != 0) startflags |= 8;
        return startflags;
    }

    public static bool RestartUses3D(int chanFlags, int chanfIs3D) => (chanFlags & chanfIs3D) != 0;

    public static bool AbortRestartNearLimit(int nearLimit, bool limitHit) => nearLimit > 0 && limitHit;

    public static int ClearEvictedFlags(int chanFlags, int evicted, int abstime) =>
        chanFlags & ~(evicted | abstime);

    public static float ClampChannelVolume(double dvolume)
    {
        if (dvolume < 0)
            return 0f;
        if (dvolume > 1)
            return 1f;
        return (float)dvolume;
    }

    public static bool ShouldChangeChannelVolume(
        int chanSourceType,
        int sourcetype,
        object? chanSource,
        object? source,
        int chanEntChannel,
        int channel) =>
        chanSourceType == sourcetype
        && chanSource == source
        && (chanEntChannel == channel || channel == -1);
}
