namespace HCDE.Playsim;

/// <summary><c>SoundEngine::StartSound</c> from <c>s_sound.cpp</c> (398–639).</summary>
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

    public static bool ShouldDeferUnpausedOneShot(int chanFlags, bool soundPaused) =>
        (chanFlags & ChanfLoop) == 0 && (chanFlags & 0x100) == 0 && soundPaused;

    public static bool Use3DPlayback(float attenuation, int sourceType) =>
        attenuation > 0 && sourceType != SourceNone;

    public static int Merge3DChannelFlags(int chanFlags, bool use3D) =>
        use3D ? chanFlags | 0x10 | 0x20 : chanFlags | 0x40 | 0x20;

    /// <summary>One iteration after <c>ResolveSound</c> (`s_sound.cpp` 444–460).</summary>
    public readonly record struct SfxLinkResolveFrame(
        bool ValidSoundId,
        bool SfxChanged,
        int NearLimit,
        float LimitRange,
        float DefPitch,
        float DefPitchMax,
        float RolloffMinDistance);

    public static float PickRolloffMinDistance(
        bool forcedRolloffPresent,
        float forcedMin,
        float sfxMin,
        float globalDefaultMin)
    {
        if (forcedRolloffPresent && forcedMin != 0)
            return forcedMin;
        if (sfxMin != 0)
            return sfxMin;
        return globalDefaultMin;
    }

    public readonly record struct StartSoundOutcome(
        bool Reject,
        int ChanFlags,
        int NearLimit,
        float Volume,
        int BasePriority,
        bool Use3D);

    public static StartSoundOutcome PlanStartSound(
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
        bool soundPaused,
        float attenuation,
        IReadOnlyList<SfxLinkResolveFrame> sfxLinkFrames,
        float sfxAttenuation,
        ref float limitRange,
        ref float defPitch,
        ref float defPitchMax,
        ref float rolloffMinDistance,
        float forcedRolloffMin,
        bool forcedRolloffPresent,
        float globalRolloffMin)
    {
        if (ShouldRejectInitial(validSound, volume, nosfx, soundEnabled, blockNewSounds))
            return new StartSoundOutcome(true, incomingFlags, nearLimit, volume, 0, false);

        sourceType = NormalizeUnattachedType(sourceType, hasPoint);
        volume = ScaleSfxVolume(volume, sfxVolume);
        if (volume <= 0)
            return new StartSoundOutcome(true, incomingFlags, nearLimit, volume, 0, false);

        for (var i = 0; i < sfxLinkFrames.Count; i++)
        {
            var frame = sfxLinkFrames[i];
            if (!frame.ValidSoundId)
                return new StartSoundOutcome(true, incomingFlags, nearLimit, volume, 0, false);

            if (!frame.SfxChanged)
                return new StartSoundOutcome(true, incomingFlags, nearLimit, volume, 0, false);

            if (nearLimit < 0)
            {
                nearLimit = frame.NearLimit;
                limitRange = frame.LimitRange;
                defPitch = frame.DefPitch;
                defPitchMax = frame.DefPitchMax;
            }

            if (rolloffMinDistance == 0)
                rolloffMinDistance = frame.RolloffMinDistance;
        }

        attenuation *= sfxAttenuation;

        rolloffMinDistance = PickRolloffMinDistance(
            forcedRolloffPresent, forcedRolloffMin, rolloffMinDistance, globalRolloffMin);

        var chanFlags = incomingFlags;
        if (ShouldMarkSingularEvict(sfxSingular, requestSingular, singularAlreadyPlaying))
            chanFlags |= ChanfEvicted;

        if (ShouldClearNearLimit(sourceType, sourceIsListener))
            nearLimit = 0;

        if (ShouldMarkLimitEvict(nearLimit, limitReached))
            chanFlags |= ChanfEvicted;

        if (ShouldAbortEvictedNonLoop(chanFlags))
            return new StartSoundOutcome(true, chanFlags, nearLimit, volume, 0, false);

        chanFlags = ApplyAutoOverlapChannel(channel, chanFlags);
        var priority = BasePriority(sourceType, sourceIsListener);
        var use3D = Use3DPlayback(attenuation, sourceType);
        chanFlags = Merge3DChannelFlags(chanFlags, use3D);

        if (ShouldDeferUnpausedOneShot(chanFlags, soundPaused))
            return new StartSoundOutcome(true, chanFlags, nearLimit, volume, priority, use3D);

        return new StartSoundOutcome(false, chanFlags, nearLimit, volume, priority, use3D);
    }
}
