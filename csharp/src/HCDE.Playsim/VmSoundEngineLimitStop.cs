namespace HCDE.Playsim;

/// <summary>Sound limit and stop helpers from <c>s_sound.cpp</c> (831–975).</summary>
public static class VmSoundEngineLimitStop
{
    public const int ChanfForgettable = 8;
    public const int ChanfEvicted = 1;
    public const int InvalidSound = -1;
    public const int ChanAuto = -1;

    public static bool SkipForgettableChannel(int chanFlags) => (chanFlags & ChanfForgettable) != 0;

    public static bool SkipEvictedChannel(int chanFlags) => (chanFlags & ChanfEvicted) != 0;

    public static bool IsRestartingSameSource(
        object? actor,
        int channel,
        int chanEntChannel,
        int chanSourceType,
        object? chanSource,
        int sourcetype) =>
        actor is not null && chanEntChannel == channel && chanSourceType == sourcetype && chanSource == actor;

    public static bool WithinLimitDistance(
        double distanceSquared,
        float limitRange,
        float attenuation,
        float chanDistanceScale)
    {
        var attn = Math.Min(chanDistanceScale, attenuation);
        return attn <= 0 || distanceSquared <= limitRange / attn;
    }

    public static bool SingularAlreadyPlaying(bool foundMatchingOrgId) => foundMatchingOrgId;

    public static bool CheckSoundLimitNative(
        int nearLimit,
        int matchingCount,
        bool limitReachedForNew) =>
        limitReachedForNew && matchingCount >= nearLimit;

    public static bool ShouldStopUnpositioned(int chanSourceType, int soundId, int chanOrgId, int channel, int chanEntChannel) =>
        chanSourceType == 0
        && (soundId == InvalidSound || soundId == chanOrgId)
        && (channel == ChanAuto || channel == chanEntChannel);

    public static bool ShouldStopActorSound(
        int chanSourceType,
        int sourcetype,
        object? chanSource,
        object? actor,
        int soundId,
        int chanOrgId,
        int channel,
        int chanEntChannel) =>
        chanSourceType == sourcetype
        && chanSource == actor
        && (soundId == InvalidSound
            ? chanEntChannel == channel || channel < 0
            : chanOrgId == soundId);

    public static bool ShouldStopActorChannel(
        int chanSourceType,
        int sourcetype,
        object? chanSource,
        object? actor,
        bool allChannels,
        int chanEntChannel,
        int chanMin,
        int chanMax) =>
        chanSourceType == sourcetype
        && chanSource == actor
        && (allChannels || (chanEntChannel >= chanMin && chanEntChannel <= chanMax));

    public static (int Min, int Max) NormalizeChannelRange(int chanMin, int chanMax) =>
        chanMax < chanMin ? (chanMax, chanMin) : (chanMin, chanMax);

    public static bool ShouldRelinkSource(int chanSourceType, int sourcetype, object? chanSource, object? from) =>
        chanSourceType == sourcetype && chanSource == from;

    public static bool SfxMatchesChannel(int chanSoundIndex, int sfxIndex) => chanSoundIndex == sfxIndex;

    public static bool CountTowardNearLimit(bool notEvicted, bool sfxMatches) => notEvicted && sfxMatches;

    public static int IncrementNearCount(int count, bool withinDistance) => withinDistance ? count + 1 : count;

    public static bool StopAllWhenNextNull(object? nextChan) => nextChan is null;
}
