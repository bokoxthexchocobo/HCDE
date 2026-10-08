namespace HCDE.Client;

/// <summary>
/// Sums queued PCM into one buffer and clamps to 16-bit. ZMusic is probed and not called.
/// </summary>
public sealed class AudioMixer
{
    private sealed class Channel(short[] samples, bool loop, string? originalSound, float distanceScale, HCDE.Gamedata.SoundRolloff? rolloff, byte[] curve, bool noPause, float volume)
    {
        public Guid Id { get; } = Guid.NewGuid();
        public short[] Samples { get; } = samples;
        public int Position { get; set; }
        public bool Loop { get; } = loop;
        public bool NoPause { get; } = noPause;
        public float Volume { get; set; } = volume;
        public string? OriginalSound { get; } = originalSound;
        public float DistanceScale { get; } = distanceScale;
        public HCDE.Gamedata.SoundRolloff? Rolloff { get; } = rolloff;
        public float DistanceGain { get; set; } = 1;
        public byte[] Curve { get; } = curve;
        public System.Numerics.Vector3? SourcePosition { get; set; }
        public string? ResolvedSound { get; set; }
        public uint? SourceActorId { get; set; }
        public int EntityChannel { get; set; }
        public int NearLimit { get; set; }
        public float LimitRange { get; set; }
        public bool Virtual { get; set; }
        public bool DefinedSingular { get; set; }
    }

    private readonly List<Channel> _channels = new();

    public int SampleRate { get; init; } = 11025;
    public bool Muted { get; set; }
    public bool Paused { get; set; }

    public bool TryPlayWadSound(ReadOnlySpan<byte> wad, HCDE.MapLoader.PlayLevel level, string sound,
        out string? error, Func<int>? nextRandom = null, float volume = 1, bool loop = false, float? pitch = null, bool pitched = true, Func<byte>? nextPitchByte = null, Func<float>? nextPitchFloat = null, float attenuation = 1, HCDE.Gamedata.SoundRolloff? forcedRolloff = null, bool singular = false, bool noPause = false, bool force = false)
        => TryPlayWadSound(wad, level, sound, out _, out error, nextRandom, volume, loop, pitch, pitched, nextPitchByte, nextPitchFloat, attenuation, forcedRolloff, singular, noPause, force);

    public bool TryPlayWadSound(ReadOnlySpan<byte> wad, HCDE.MapLoader.PlayLevel level, string sound,
        out Guid channelId, out string? error, Func<int>? nextRandom = null, float volume = 1, bool loop = false, float? pitch = null, bool pitched = true, Func<byte>? nextPitchByte = null, Func<float>? nextPitchFloat = null, float attenuation = 1, HCDE.Gamedata.SoundRolloff? forcedRolloff = null, bool singular = false, bool noPause = false, bool force = false,
        System.Numerics.Vector3? sourcePosition = null, uint? sourceActorId = null, int entityChannel = 0, bool listenerSource = false)
    {
        channelId = Guid.Empty;
        error = null;
        if (sourcePosition is { } position && (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || !float.IsFinite(position.Z)))
        { error = "audio-position-invalid"; return false; }
        if (forcedRolloff is not null && (!float.IsFinite(forcedRolloff.MinDistance) || !float.IsFinite(forcedRolloff.MaxDistance))) { error = "audio-rolloff-invalid"; return false; }
        if (!float.IsFinite(attenuation)) { error = "audio-attenuation-invalid"; return false; }
        if (pitch is { } explicitPitch && (!float.IsFinite(explicitPitch) || explicitPitch <= 0)) { error = "audio-pitch-invalid"; return false; }
        var settingVolume = level.SoundSettings.TryGetValue(sound, out var settings) ? settings.Volume : 1;
        if (!float.IsFinite(volume) || !float.IsFinite(settingVolume))
        { error = "audio-volume-invalid"; return false; }
        var gain = Math.Min(volume * settingVolume, 1);
        if (gain <= 0) return true;
        var path = new List<string>();
        if (!HCDE.MapLoader.SoundResourceResolver.TryResolveNamed(level, sound, nextRandom, out _, out var resolvedSound, out error, path.Add)) return false;
        var definedSingular = level.SoundSettings.TryGetValue(resolvedSound, out var resolvedSettings) && resolvedSettings.Singular;
        var distanceScale = attenuation;
        foreach (var name in path)
            if (level.RandomSoundGroups.ContainsKey(name) && level.SoundSettings.TryGetValue(name, out var groupSettings))
                distanceScale *= groupSettings.Attenuation;
        distanceScale *= resolvedSettings?.Attenuation ?? 1;
        if (!float.IsFinite(distanceScale)) { error = "audio-attenuation-invalid"; return false; }
        var singularBlocked = (singular || definedSingular) && CheckSingular(resolvedSound);
        if (singularBlocked && !loop) return true;
        var inherited = settings ?? new HCDE.Gamedata.SndInfoSoundSettings();
        foreach (var name in path.Skip(1))
            if (inherited.NearLimit < 0)
                inherited = level.SoundSettings.TryGetValue(name, out var nextSettings) ? nextSettings : new HCDE.Gamedata.SndInfoSoundSettings();
        var nearLimit = !listenerSource && sourcePosition.HasValue ? inherited.NearLimit : 0;
        var blocked = singularBlocked;
        if (nearLimit > 0)
        {
            if (!float.IsFinite(inherited.LimitRange)) { error = "audio-limit-range-invalid"; return false; }
            blocked |= CheckSoundLimit(resolvedSound, sourcePosition!.Value, nearLimit, inherited.LimitRange,
                distanceScale, sourceActorId, entityChannel);
            if (blocked && !loop) return true;
        }
        if (!HCDE.MapLoader.SoundResourceResolver.TryReadDoomSound(wad, level, resolvedSound, out var decoded, out error)) return false;
        if (decoded is null || decoded.Samples.Length == 0) return true;
        if (SampleRate <= 0) { error = "audio-mixer-sample-rate-invalid"; return false; }
        if (Paused && !loop && !noPause && !force) return true;
        var pitchSettings = (resolvedSettings ?? new HCDE.Gamedata.SndInfoSoundSettings()) with
        { DefPitch = inherited.DefPitch, DefPitchMax = inherited.DefPitchMax };
        if (pitch is null)
        {
            var needsFloat = pitchSettings.DefPitch > 0 && pitchSettings.DefPitchMax > 0 && pitchSettings.DefPitch != pitchSettings.DefPitchMax;
            var needsBytes = pitchSettings.DefPitch <= 0 && pitched && pitchSettings.PitchMask != 0;
            if (needsFloat && nextPitchFloat is null || needsBytes && nextPitchByte is null)
            { error = "audio-pitch-random-provider-required"; return false; }
        }
        var effectivePitch = pitch ?? HCDE.Gamedata.SoundPitchCalculator.Calculate(pitchSettings, pitched, nextPitchByte, nextPitchFloat);
        var samples = PcmResampler.Convert(decoded.Samples, decoded.SampleRate, SampleRate, effectivePitch);
        var rolloff = settings?.Rolloff;
        foreach (var name in path.Skip(1))
            if (rolloff is null || rolloff.MinDistance == 0)
                rolloff = level.SoundSettings.GetValueOrDefault(name)?.Rolloff;
        if (forcedRolloff is not null && forcedRolloff.MinDistance != 0)
            rolloff = forcedRolloff;
        if (rolloff is null || rolloff.MinDistance == 0)
            rolloff = level.GlobalSoundRolloff;
        byte[] curve = [];
        if (rolloff?.Type == HCDE.Gamedata.SoundRolloffType.Custom &&
            !HCDE.MapLoader.SoundResourceResolver.TryReadSoundCurve(wad, out curve, out error)) return false;
        channelId = QueueChannel(samples, loop, sound, distanceScale, rolloff, curve, noPause, gain);
        if (channelId != Guid.Empty)
        {
            var queued = _channels[^1];
            queued.ResolvedSound = resolvedSound;
            queued.SourcePosition = sourcePosition;
            queued.SourceActorId = sourceActorId;
            queued.EntityChannel = entityChannel;
            queued.NearLimit = nearLimit;
            queued.LimitRange = inherited.LimitRange;
            queued.Virtual = blocked;
            queued.DefinedSingular = definedSingular;
        }
        return true;
    }

    public void Play(ReadOnlySpan<short> pcm) => PlayChannel(pcm);

    public Guid PlayChannel(ReadOnlySpan<short> pcm, bool loop = false, bool noPause = false)
        => QueueChannel(pcm, loop, null, 1, null, [], noPause, 1);

    private Guid QueueChannel(ReadOnlySpan<short> pcm, bool loop, string? originalSound, float distanceScale, HCDE.Gamedata.SoundRolloff? rolloff, byte[] curve, bool noPause, float volume)
    {
        if (pcm.IsEmpty)
            return Guid.Empty;
        var channel = new Channel(pcm.ToArray(), loop, originalSound, distanceScale, rolloff, curve, noPause, volume);
        _channels.Add(channel);
        return channel.Id;
    }

    public bool IsPlaying(Guid channelId) => _channels.Any(channel => channel.Id == channelId);

    public bool IsChannelVirtual(Guid channelId) => _channels.Any(channel => channel.Id == channelId && channel.Virtual);

    private bool CheckSingular(string sound)
        => _channels.Any(channel => string.Equals(channel.OriginalSound, sound, StringComparison.OrdinalIgnoreCase));

    private bool CheckSoundLimit(string sound, System.Numerics.Vector3 origin, int limit, float range,
        float attenuation, uint? actorId = null, int entityChannel = 0)
    {
        var count = 0;
        foreach (var channel in _channels.AsEnumerable().Reverse())
        {
            if (channel.Virtual || !string.Equals(channel.ResolvedSound, sound, StringComparison.OrdinalIgnoreCase)) continue;
            if (actorId.HasValue && channel.SourceActorId == actorId && channel.EntityChannel == entityChannel) return false;
            var scale = Math.Min(channel.DistanceScale, attenuation);
            if (scale <= 0 || System.Numerics.Vector3.DistanceSquared(channel.SourcePosition ?? default, origin) <= range / scale)
                count++;
            if (count >= limit) return true;
        }
        return false;
    }

    public bool TryGetChannelDistanceScale(Guid channelId, out float distanceScale)
    {
        var channel = _channels.FirstOrDefault(channel => channel.Id == channelId);
        distanceScale = channel?.DistanceScale ?? 0;
        return channel is not null;
    }

    public bool StopChannel(Guid channelId) => _channels.RemoveAll(channel => channel.Id == channelId) != 0;

    public bool DetachChannelSource(Guid channelId, bool cutoff = false)
    {
        var channel = _channels.FirstOrDefault(channel => channel.Id == channelId);
        if (channel is null) return false;
        if (cutoff || channel.Loop) return StopChannel(channelId);
        channel.SourceActorId = null;
        return true;
    }

    public bool TrySetChannelVolume(Guid channelId, float volume, out string? error)
    {
        error = null;
        if (!float.IsFinite(volume)) { error = "audio-volume-invalid"; return false; }
        var channel = _channels.FirstOrDefault(channel => channel.Id == channelId);
        if (channel is null) { error = "audio-channel-missing"; return false; }
        channel.Volume = Math.Clamp(volume, 0, 1);
        return true;
    }

    public bool TryGetChannelRolloff(Guid channelId, out HCDE.Gamedata.SoundRolloff? rolloff)
    {
        var channel = _channels.FirstOrDefault(channel => channel.Id == channelId);
        rolloff = channel?.Rolloff;
        return channel is not null;
    }

    public void StopAllChannels() => _channels.Clear();

    public bool TrySetChannelPosition(Guid channelId, System.Numerics.Vector3 source,
        System.Numerics.Vector3 listener, out string? error, ReadOnlySpan<byte> customCurve = default)
    {
        if (!float.IsFinite(source.X) || !float.IsFinite(source.Y) || !float.IsFinite(source.Z) ||
            !float.IsFinite(listener.X) || !float.IsFinite(listener.Y) || !float.IsFinite(listener.Z))
        { error = "audio-position-invalid"; return false; }
        var distance = System.Numerics.Vector3.Distance(source, listener);
        if (!float.IsFinite(distance)) { error = "audio-position-invalid"; return false; }
        if (!TrySetChannelDistance(channelId, distance, out error, customCurve)) return false;
        _channels.First(channel => channel.Id == channelId).SourcePosition = source;
        return true;
    }

    public bool TryUpdateListenerPosition(System.Numerics.Vector3 listener, out string? error)
    {
        error = null;
        if (!float.IsFinite(listener.X) || !float.IsFinite(listener.Y) || !float.IsFinite(listener.Z))
        { error = "audio-position-invalid"; return false; }
        var positioned = _channels.Where(channel => channel.SourcePosition.HasValue).ToArray();
        var gains = positioned.Select(channel => channel.DistanceGain).ToArray();
        for (var index = 0; index < positioned.Length; index++)
        {
            var channel = positioned[index];
            if (TrySetChannelPosition(channel.Id, channel.SourcePosition!.Value, listener, out error)) continue;
            for (var restore = 0; restore < positioned.Length; restore++)
                positioned[restore].DistanceGain = gains[restore];
            return false;
        }
        return true;
    }

    public bool TrySetChannelDistance(Guid channelId, float? distance, out string? error,
        ReadOnlySpan<byte> customCurve = default)
    {
        error = null;
        var channel = _channels.FirstOrDefault(channel => channel.Id == channelId);
        if (channel is null) { error = "audio-channel-missing"; return false; }
        if (distance is { } value && (!float.IsFinite(value) || value < 0))
        { error = "audio-distance-invalid"; return false; }
        var gain = 1f;
        if (distance is { } positionedDistance && channel.DistanceScale > 0)
        {
            if (channel.Rolloff is null) { error = "audio-rolloff-required"; return false; }
            var scaledDistance = positionedDistance * channel.DistanceScale;
            if (!float.IsFinite(scaledDistance)) { error = "audio-distance-invalid"; return false; }
            try
            {
                gain = HCDE.Gamedata.SoundRolloffCalculator.Calculate(channel.Rolloff, scaledDistance,
                    customCurve.IsEmpty ? channel.Curve : customCurve);
            }
            catch (ArgumentException)
            {
                error = "audio-rolloff-invalid";
                return false;
            }
        }
        channel.DistanceGain = Math.Clamp(gain, 0, 1);
        channel.SourcePosition = null;
        return true;
    }

    public short[] Mix(int sampleCount)
    {
        if (sampleCount < 0)
            throw new ArgumentOutOfRangeException(nameof(sampleCount));
        var output = new short[sampleCount];
        // Native restoration visits older evicted channels first.
        foreach (var channel in _channels.Where(channel => channel.Virtual))
        {
            if (Paused && !channel.NoPause) continue;
            if (channel.DefinedSingular && CheckSingular(channel.ResolvedSound!)) continue;
            if (channel.NearLimit > 0 && CheckSoundLimit(channel.ResolvedSound!, channel.SourcePosition ?? default,
                channel.NearLimit, channel.LimitRange, channel.DistanceScale)) continue;
            channel.Virtual = false;
        }
        foreach (var channel in _channels)
        {
            if (channel.Virtual) continue;
            if (Paused && !channel.NoPause) continue;
            var count = channel.Loop ? sampleCount : Math.Min(sampleCount, channel.Samples.Length - channel.Position);
            if (!Muted)
            {
                for (var i = 0; i < count; i++)
                {
                    var position = channel.Loop ? (int)(((long)channel.Position + i) % channel.Samples.Length) : channel.Position + i;
                    var scaled = Math.Round(channel.Samples[position] * channel.Volume, MidpointRounding.AwayFromZero);
                    var sample = (int)Math.Round(scaled * channel.DistanceGain, MidpointRounding.AwayFromZero);
                    var sum = output[i] + sample;
                    output[i] = (short)Math.Clamp(sum, short.MinValue, short.MaxValue);
                }
            }
            channel.Position = channel.Loop ? (int)(((long)channel.Position + count) % channel.Samples.Length) : channel.Position + count;
        }

        _channels.RemoveAll(channel => !channel.Loop && channel.Position == channel.Samples.Length);
        return output;
    }
}
