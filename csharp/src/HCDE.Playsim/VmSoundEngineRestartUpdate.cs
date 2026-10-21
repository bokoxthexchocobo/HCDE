namespace HCDE.Playsim;

/// <summary><c>SoundEngine::RestartChannel</c> policy from <c>s_sound.cpp</c> (649–800).</summary>
public static class VmSoundEngineRestartUpdate
{
    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;

    public static bool RequiresEvictedFlag(int chanFlags) => (chanFlags & ChanfEvicted) != 0;

    public static bool ShouldRestartLoop(bool wantsLoop, bool stillEvicted) => wantsLoop && stillEvicted;

    public static bool AbortRestartSingular(bool sfxSingular, bool alreadyPlaying) =>
        sfxSingular && alreadyPlaying;

    public static bool AbortRestartEmptyLump(int lumpnum, int sfxEmpty) => lumpnum == sfxEmpty;

    public static bool ChangeVolumeInRange(double volume) => volume >= 0;

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

    public static bool RestoreFlagsOnFailure(object? ochan, object? chan, int oldFlags) =>
        ochan is null;
}
