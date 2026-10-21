namespace HCDE.Playsim;

/// <summary><c>SoundEngine::StartSound</c> policy from <c>s_sound.cpp</c> (398–639).</summary>
public static class VmSoundEngineStartSound
{
    public const int SourceNone = 0;
    public const int SourceUnattached = 2;
    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;
    public const int ChanfOverlap = 4;
    public const int ChanAuto = -1;

    public static bool ShouldRejectInitial(bool validSound, float volume, bool nosfx, bool soundEnabled, bool blockNewSounds) =>
        !validSound || volume <= 0 || nosfx || !soundEnabled || blockNewSounds;

    public static int NormalizeUnattachedType(int type, bool hasPoint) =>
        type == SourceUnattached && !hasPoint ? SourceNone : type;

    public static float ScaleSfxVolume(float volume, float sfxVolume) =>
        Math.Min(volume * sfxVolume, 1f);

    public static bool VolumePositiveAfterScale(float volume) => volume > 0;

    public static bool ShouldClearNearLimit(int sourceType, bool sourceIsListener) =>
        sourceType == SourceNone || sourceIsListener;

    public static bool ShouldMarkSingularEvict(bool sfxSingular, bool chanSingular, bool alreadyPlaying) =>
        (sfxSingular || chanSingular) && alreadyPlaying;

    public static bool ShouldMarkLimitEvict(int nearLimit, bool limitReached) =>
        nearLimit > 0 && limitReached;

    public static bool ShouldAbortEvictedNonLoop(int chanFlags) =>
        (chanFlags & (ChanfEvicted | ChanfLoop)) == ChanfEvicted;

    public static int BasePriority(int sourceType, bool sourceIsListener) =>
        sourceType == SourceNone || sourceIsListener ? 80 : 0;

    public static int ApplyAutoOverlapChannel(int channel, int chanFlags) =>
        channel == ChanAuto ? chanFlags | ChanfOverlap : chanFlags;

    public static bool ShouldStopChannelOnReuse(int chanFlags, int sourceType, int channel, bool channelUsed) =>
        (chanFlags & ChanfOverlap) == 0 && sourceType != SourceNone
        && (channel != ChanAuto || channelUsed);

    public static bool ShouldDeferUnpausedOneShot(int chanFlags, bool soundPaused) =>
        (chanFlags & ChanfLoop) == 0 && (chanFlags & 0x100) == 0 && soundPaused;

    public static bool Use3DPlayback(float attenuation, int sourceType) =>
        attenuation > 0 && sourceType != SourceNone;

    public static bool ShouldInheritNearLimit(int currentNearLimit, int linkedNearLimit) =>
        currentNearLimit < 0 && linkedNearLimit >= 0;

    public static bool ShouldInheritRolloff(float rolloffMinDistance) => rolloffMinDistance == 0;

    public static bool ShouldForceRolloff(float forcedMinDistance) => forcedMinDistance != 0;

    public static float ApplySfxAttenuation(float attenuation, float sfxAttenuation) =>
        attenuation * sfxAttenuation;

    public static bool MatchesUnattachedPoint(
        double chanX, double chanY, double chanZ,
        double ptX, double ptY, double ptZ) =>
        chanX == ptX && chanY == ptY && chanZ == ptZ;

    public static bool ShouldLoopEvictPlaceholder(bool chanIsNull, bool wantsLoop) =>
        chanIsNull && wantsLoop;

    public static int Merge3DChannelFlags(int chanFlags, bool use3D) =>
        use3D ? chanFlags | 0x10 | 0x20 : chanFlags | 0x40 | 0x20;

    public readonly record struct StartSoundPlan(
        bool Reject,
        int ChanFlags,
        int NearLimit,
        float Volume,
        int BasePriority,
        bool Use3D);

    public static StartSoundPlan PlanStartSound(
        bool validSound,
        float volume,
        bool nosfx,
        bool soundEnabled,
        bool blockNewSounds,
        int sourceType,
        bool hasPoint,
        float sfxVolume,
        bool sfxSingular,
        bool requestSingular,
        bool singularAlreadyPlaying,
        int nearLimit,
        bool limitReached,
        bool sourceIsListener,
        int channel,
        int incomingFlags,
        bool channelUsed,
        bool soundPaused,
        float attenuation)
    {
        if (ShouldRejectInitial(validSound, volume, nosfx, soundEnabled, blockNewSounds))
            return new StartSoundPlan(true, incomingFlags, nearLimit, volume, 0, false);

        sourceType = NormalizeUnattachedType(sourceType, hasPoint);
        volume = ScaleSfxVolume(volume, sfxVolume);
        if (!VolumePositiveAfterScale(volume))
            return new StartSoundPlan(true, incomingFlags, nearLimit, volume, 0, false);

        var chanFlags = incomingFlags;
        if (ShouldMarkSingularEvict(sfxSingular, requestSingular, singularAlreadyPlaying))
            chanFlags |= ChanfEvicted;

        if (ShouldClearNearLimit(sourceType, sourceIsListener))
            nearLimit = 0;

        if (ShouldMarkLimitEvict(nearLimit, limitReached))
            chanFlags |= ChanfEvicted;

        if (ShouldAbortEvictedNonLoop(chanFlags))
            return new StartSoundPlan(true, chanFlags, nearLimit, volume, 0, false);

        chanFlags = ApplyAutoOverlapChannel(channel, chanFlags);
        var priority = BasePriority(sourceType, sourceIsListener);
        var use3D = Use3DPlayback(attenuation, sourceType);
        chanFlags = Merge3DChannelFlags(chanFlags, use3D);

        if (ShouldDeferUnpausedOneShot(chanFlags, soundPaused))
            return new StartSoundPlan(true, chanFlags, nearLimit, volume, priority, use3D);

        return new StartSoundPlan(false, chanFlags, nearLimit, volume, priority, use3D);
    }
}
