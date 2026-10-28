namespace HCDE.Playsim;

/// <summary>Limit/stop/relink from <c>s_sound.cpp</c> (831–1022).</summary>
public static class VmSoundEngineLimitStop
{
    public const int ChanfForgettable = 8;
    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;
    public const int InvalidSound = -1;
    public const int ChanAuto = -1;

    public sealed class CheckSoundLimitContext
    {
        public int NearLimit { get; set; }
        public float LimitRange { get; set; }
        public int SfxId { get; set; }
        public int SourceType { get; set; }
        public object? Actor { get; set; }
        public int Channel { get; set; }
        public float Attenuation { get; set; }
        public IReadOnlyList<SoundLimitChannel> Channels { get; set; } = Array.Empty<SoundLimitChannel>();
        public Func<SoundLimitChannel, (double X, double Y, double Z)>? CalcPosVel { get; init; }
    }

    public readonly record struct SoundLimitChannel(
        int ChanFlags,
        int SoundId,
        int EntChannel,
        int SourceType,
        object? Source,
        float DistanceScale);

    public static bool CheckSoundLimit(CheckSoundLimitContext ctx)
    {
        var count = 0;
        for (var i = 0; i < ctx.Channels.Count && count < ctx.NearLimit; i++)
        {
            var chan = ctx.Channels[i];
            if ((chan.ChanFlags & ChanfForgettable) != 0)
                continue;
            if ((chan.ChanFlags & ChanfEvicted) != 0 || chan.SoundId != ctx.SfxId)
                continue;

            if (ctx.Actor is not null && chan.EntChannel == ctx.Channel
                && chan.SourceType == ctx.SourceType && chan.Source == ctx.Actor)
            {
                return false;
            }

            var origin = ctx.CalcPosVel?.Invoke(chan) ?? (0, 0, 0);
            var attn = Math.Min(chan.DistanceScale, ctx.Attenuation);
            var distSq = origin.X * origin.X + origin.Y * origin.Y + origin.Z * origin.Z;
            if (attn <= 0 || distSq <= ctx.LimitRange / attn)
                count++;
        }

        return count >= ctx.NearLimit;
    }

    public static void StopSoundId(
        int soundId,
        IReadOnlyList<int> channelOrgIds,
        Action<int> stopChannel)
    {
        for (var i = 0; i < channelOrgIds.Count; i++)
        {
            if (channelOrgIds[i] == soundId)
                stopChannel(i);
        }
    }

    public static void StopSound(
        int channel,
        int soundId,
        IReadOnlyList<(int SourceType, int OrgId, int EntChannel)> channels,
        Action<int> stopChannel)
    {
        for (var i = 0; i < channels.Count; i++)
        {
            var chan = channels[i];
            if (chan.SourceType != 0)
                continue;
            if (soundId != InvalidSound && soundId != chan.OrgId)
                continue;
            if (channel != ChanAuto && channel != chan.EntChannel)
                continue;
            stopChannel(i);
        }
    }

    public static void StopActorSound(
        int sourcetype,
        object? actor,
        int channel,
        int soundId,
        IReadOnlyList<(int SourceType, object? Source, int OrgId, int EntChannel)> channels,
        Action<int> stopChannel)
    {
        for (var i = 0; i < channels.Count; i++)
        {
            var chan = channels[i];
            if (chan.SourceType != sourcetype || chan.Source != actor)
                continue;
            if (soundId == InvalidSound)
            {
                if (chan.EntChannel != channel && channel >= 0)
                    continue;
            }
            else if (chan.OrgId != soundId)
            {
                continue;
            }

            stopChannel(i);
        }
    }

    public static void StopActorSounds(
        int sourcetype,
        object? actor,
        int chanMin,
        int chanMax,
        IReadOnlyList<(int SourceType, object? Source, int EntChannel)> channels,
        Action<int> stopChannel)
    {
        var all = chanMin == 0 && chanMax == 0;
        if (chanMax < chanMin)
            (chanMin, chanMax) = (chanMax, chanMin);

        for (var i = 0; i < channels.Count; i++)
        {
            var chan = channels[i];
            if (chan.SourceType != sourcetype || chan.Source != actor)
                continue;
            if (!all && (chan.EntChannel < chanMin || chan.EntChannel > chanMax))
                continue;
            stopChannel(i);
        }
    }

    public static void StopAllChannels(
        IReadOnlyList<int> channelCount,
        Action<int> stopChannel,
        Action? updateSounds)
    {
        for (var i = 0; i < channelCount.Count; i++)
            stopChannel(i);
        updateSounds?.Invoke();
    }

    public static void RelinkSound(
        int sourcetype,
        object? from,
        object? to,
        bool hasOptionalPoint,
        double pointX,
        double pointY,
        double pointZ,
        IList<(int SourceType, object? Source, int ChanFlags, double X, double Y, double Z)> channels,
        Action<int> stopChannel)
    {
        if (from is null)
            return;

        for (var i = 0; i < channels.Count; i++)
        {
            var chan = channels[i];
            if (chan.SourceType != sourcetype || chan.Source != from)
                continue;

            if (to is not null)
            {
                var updated = chan;
                updated.Source = to;
                channels[i] = updated;
            }
            else if ((chan.ChanFlags & ChanfLoop) == 0 && hasOptionalPoint)
            {
                channels[i] = (0, null, chan.ChanFlags, pointX, pointY, pointZ);
            }
            else
            {
                stopChannel(i);
            }
        }
    }
}
