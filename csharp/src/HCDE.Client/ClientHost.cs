using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client;

public sealed class ClientHost : IDisposable
{
    public const uint HardwareWall = 0x00B42828;
    private bool _disposed;
    private readonly string? _viewPreferencesPath;

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            if (_viewPreferencesPath is not null) CaptureViewPreferences().SaveFile(_viewPreferencesPath);
        }
        finally
        {
            _disposed = true;
            Simulation.ActorDestroyed -= OnActorDestroyed;
            Simulation.StateRestored -= ResetAudioState;
            Simulation.LevelExited -= ResetAudioState;
            Simulation.SoundRequestsReady -= ConsumeSoundRequests;
            ResetAudioState();
            _soundArchive = null;
        }
    }

    private void ResetAudioState()
    {
        Audio.StopAllChannels();
        _audioSources.Clear();
        _listenerHeightSources.Clear();
        _actorSoundChannels.Clear();
        _soundOwners.Clear();
        _playingSounds.Clear();
    }

    public ClientHost(AuthoritySimulation simulation, int width = 32, int height = 32, string? viewPreferencesPath = null)
    {
        // Parse before subscribing to simulation events so failed loads cannot retain the host.
        var preferences = viewPreferencesPath is null ? null : ClientViewPreferences.LoadFile(viewPreferencesPath);
        _viewPreferencesPath = viewPreferencesPath is null ? null : Path.GetFullPath(viewPreferencesPath);
        Simulation = simulation;
        simulation.ActorDestroyed += OnActorDestroyed;
        simulation.StateRestored += ResetAudioState;
        simulation.LevelExited += ResetAudioState;
        simulation.SoundRequestsReady += ConsumeSoundRequests;
        Frame = new SoftwareFrame(width, height);
        Audio = new AudioMixer();
        ZScript = new ZScriptVm();
        Hardware = new RecordedHardwareDevice();
        if (preferences is not null) ApplyViewPreferences(preferences);
    }

    public AuthoritySimulation Simulation { get; }
    public SoftwareFrame Frame { get; }
    public AudioMixer Audio { get; }
    public ZScriptVm ZScript { get; }
    public RecordedHardwareDevice Hardware { get; }
    public bool UseHardware { get; set; }
    public bool SmoothPain { get; set; }
    public bool PoisonPaletteFlash { get; set; }
    private float _painSmoothStrength = 0.6f;
    public float PainSmoothStrength
    {
        get => _painSmoothStrength;
        set => _painSmoothStrength = float.IsFinite(value)
            ? Math.Clamp(value, 0, 1) : throw new ArgumentOutOfRangeException(nameof(value));
    }
    private double _bloodFadeScalar = 1;
    public double BloodFadeScalar
    {
        get => _bloodFadeScalar;
        set => _bloodFadeScalar = double.IsFinite(value) && value >= 0
            ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
    public bool SilentPickupCompatibility { get; set; }
    public ClientViewPreferences CaptureViewPreferences() => new()
    {
        BloodFadeScalar = BloodFadeScalar,
        SmoothPain = SmoothPain,
        PoisonPaletteFlash = PoisonPaletteFlash,
        PainSmoothStrength = PainSmoothStrength,
    };

    public void ApplyViewPreferences(ClientViewPreferences preferences)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(preferences);
        preferences.Validate();
        BloodFadeScalar = preferences.BloodFadeScalar;
        SmoothPain = preferences.SmoothPain;
        PoisonPaletteFlash = preferences.PoisonPaletteFlash;
        PainSmoothStrength = preferences.PainSmoothStrength;
    }
    public bool MagicSilenceCompatibility { get; set; }
    public bool SoundCutoffCompatibility { get; set; }
    private bool _gamePaused;
    public bool GamePaused
    {
        get => _gamePaused;
        set { _gamePaused = value; Audio.Paused = value; }
    }
    public short[] LastMix { get; private set; } = Array.Empty<short>();
    public System.Numerics.Vector3? AudioListenerPosition { get; private set; }
    private readonly Dictionary<Guid, uint> _audioSources = new();
    private readonly HashSet<Guid> _listenerHeightSources = new();
    private readonly Dictionary<(uint ActorId, int Channel), Guid> _actorSoundChannels = new();
    private readonly Dictionary<Guid, (uint ActorId, int Channel)> _soundOwners = new();
    private readonly Dictionary<Guid, (uint? ActorId, int Channel, string Sound)> _playingSounds = new();
    private byte[]? _soundArchive;
    public IReadOnlyList<string> LastSoundRequestErrors { get; private set; } = Array.Empty<string>();

    public bool TrySetSoundArchive(ReadOnlySpan<byte> wad, out string? error)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!WadArchiveReader.TryReadDirectory(wad, out _, out error)) return false;
        _soundArchive = wad.ToArray();
        return true;
    }

    private void ConsumeSoundRequests()
    {
        if (Simulation.Exited)
        {
            Simulation.DrainSoundRequests();
            return;
        }
        if (_soundArchive is null) return;
        var errors = new List<string>();
        foreach (var handle in _playingSounds.Keys.Where(handle => !Audio.IsPlaying(handle)).ToArray())
            _playingSounds.Remove(handle);
        foreach (var request in Simulation.DrainSoundRequests())
        {
            if (request.ChangePitch)
            {
                if (request.Pitch is not { } pitch) { errors.Add($"audio-pitch-missing: {request.ActorId}"); continue; }
                foreach (var (handle, owner) in _soundOwners)
                    if (owner.ActorId == request.ActorId && owner.Channel == request.Channel && Audio.IsPlaying(handle))
                        if (!Audio.TrySetChannelPitch(handle, pitch, out var pitchError))
                            errors.Add($"{pitchError}: {request.ActorId}");
                continue;
            }
            if (request.ChangeVolume)
            {
                foreach (var (handle, owner) in _soundOwners)
                    if (owner.ActorId == request.ActorId && (MagicSilenceCompatibility || owner.Channel == request.Channel || request.Channel == -1) && Audio.IsPlaying(handle))
                        if (!Audio.TrySetChannelVolume(handle, request.Volume, out var volumeError))
                            errors.Add($"{volumeError}: {request.ActorId}");
                continue;
            }
            if (request.Stop)
            {
                if (request.StopChannelMax is { } maximum) StopActorSounds(request.ActorId, request.Channel, maximum);
                else StopActorSound(request.ActorId, request.Channel);
                continue;
            }
            const int supported = (int)(SimulationSoundFlags.ListenerZ | SimulationSoundFlags.MaybeLocal | SimulationSoundFlags.Loop | SimulationSoundFlags.NoStop | SimulationSoundFlags.Overlap | SimulationSoundFlags.Local | SimulationSoundFlags.Singular | SimulationSoundFlags.Ui | SimulationSoundFlags.NoPause | SimulationSoundFlags.Force | SimulationSoundFlags.NoRumble);
            if ((request.Flags & ~supported) != 0)
            {
                errors.Add($"audio-request-flags-unsupported: {request.Sound}");
                continue;
            }
            var listener = Simulation.Players.FirstOrDefault();
            var sourceActor = !request.NonPositional && request.DetachedOrigin is null
                ? Simulation.Actors.FirstOrDefault(actor => actor.Id == request.ActorId && !actor.Destroyed) : null;
            if (sourceActor is not null && IsInSilentSector(sourceActor)) continue;
            if (!request.NonPositional && request.DetachedOrigin is null && SilentPickupCompatibility && (request.Flags & (int)SimulationSoundFlags.MaybeLocal) != 0 &&
                listener?.Id != request.ActorId) continue;
            var local = request.NonPositional || request.DetachedOrigin is null &&
                (request.Local || (request.Flags & (int)SimulationSoundFlags.Local) != 0);
            var loop = request.Loop || (request.Flags & (int)SimulationSoundFlags.Loop) != 0;
            var noStop = request.Loop || (request.Flags & (int)SimulationSoundFlags.NoStop) != 0;
            var singular = (request.Flags & (int)SimulationSoundFlags.Singular) != 0;
            var sourceId = local ? (uint?)null : request.ActorId;
            if (request.OnlyIfChannelFree && _playingSounds.Any(pair => Audio.IsPlaying(pair.Key) &&
                pair.Value.ActorId == sourceId && (request.Channel == 0 || !local && MagicSilenceCompatibility || pair.Value.Channel == request.Channel))) continue;
            if (!request.NonPositional && (noStop || request.OnlyIfSoundNotPlaying) && (local
                ? Audio.IsUnpositionedSoundPlaying(request.Channel, request.Sound)
                : _playingSounds.Any(pair => Audio.IsPlaying(pair.Key) &&
                pair.Value.ActorId == sourceId && (request.Channel == 0 || !local && MagicSilenceCompatibility || pair.Value.Channel == request.Channel) &&
                pair.Value.Sound.Equals(request.Sound, StringComparison.OrdinalIgnoreCase)))) continue;
            if (local)
            {
                if (!request.NonPositional && listener?.Id != request.ActorId) continue;
                if (!Audio.TryPlayWadSound(_soundArchive, Simulation.Level, request.Sound, out var localHandle,
                    out var localError, volume: request.Volume, loop: loop, pitch: request.Pitch, startTime: request.StartTime,
                    entityChannel: request.Channel, attenuation: 0, singular: singular, noPause: (request.Flags & 96) != 0, force: (request.Flags & 65536) != 0))
                {
                    errors.Add($"{localError}: {request.Sound}");
                    continue;
                }
                if (localHandle != Guid.Empty) _playingSounds[localHandle] = (null, request.Channel, request.Sound);
                continue;
            }
            var attenuation = listener?.Id == request.ActorId ? 0 : request.Attenuation;
            if (!TryPlayActorWadSound(_soundArchive, Simulation.Level, request.Sound, request.ActorId,
                out _, out var error, loop, request.Volume, attenuation, pitch: request.Pitch, startTime: request.StartTime, entityChannel: request.Channel,
                overlap: (request.Flags & (int)SimulationSoundFlags.Overlap) != 0, singular: singular,
                listenerHeight: (request.Flags & (int)SimulationSoundFlags.ListenerZ) != 0, noPause: (request.Flags & 96) != 0, force: (request.Flags & 65536) != 0,
                detachedOrigin: request.DetachedOrigin))
                errors.Add($"{error}: {request.Sound}");
        }
        LastSoundRequestErrors = errors.AsReadOnly();
    }

    public bool TryPlayActorWadSound(ReadOnlySpan<byte> wad, PlayLevel level, string sound, uint actorId,
        out Guid channelId, out string? error, bool loop = false, float volume = 1, float attenuation = 1,
        Func<int>? nextRandom = null, float? pitch = null, int entityChannel = 0, bool overlap = false, bool singular = false, bool listenerHeight = false, bool noPause = false, bool force = false, float startTime = 0,
        SimulationSoundOrigin? detachedOrigin = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        channelId = Guid.Empty;
        if (Simulation.Exited) { error = null; return true; }
        if (entityChannel is < 0 or > 7) { error = "audio-entity-channel-invalid"; return false; }
        var actor = detachedOrigin is null
            ? Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed) : null;
        if (actor is null && detachedOrigin is null)
        { error = "audio-source-actor-missing"; return false; }
        var sectorIndex = actor?.SectorIndex ?? detachedOrigin!.Value.SectorIndex;
        if ((uint)sectorIndex < (uint)Simulation.Level.Sectors.Count && Simulation.Level.Sectors[sectorIndex].Silent)
        { error = null; return true; }
        if (actor is not null && MagicSilenceCompatibility) entityChannel = 1;
        var listener = Simulation.Players.FirstOrDefault();
        if (listener is null) { error = "audio-listener-missing"; return false; }
        var position = actor is not null ? SoundPosition(actor) :
            new System.Numerics.Vector3(detachedOrigin!.Value.X, detachedOrigin.Value.Y, detachedOrigin.Value.Z);
        if (listenerHeight) position.Y = SoundPosition(listener).Y;
        if (!Audio.TryPlayWadSound(wad, level, sound, out var queued, out error,
            nextRandom, volume, loop, pitch, attenuation: attenuation, singular: singular, noPause: noPause, force: force,
            sourcePosition: position, sourceActorId: actor?.Id, entityChannel: entityChannel, listenerSource: actor == listener, startTime: startTime)) return false;
        if (queued == Guid.Empty) return true;
        if (actor is not null && !TryBindAudioSource(queued, actorId, out error, listenerHeight))
        {
            Audio.StopChannel(queued);
            return false;
        }
        if (actor is null)
        {
            if (!Audio.TrySetChannelPosition(queued, position, SoundPosition(listener), out error))
            {
                Audio.StopChannel(queued);
                return false;
            }
            Audio.SetChannelListenerHeight(queued, listenerHeight);
            if (entityChannel != 0 && !overlap) Audio.ReplacePositionedChannel(position, entityChannel, queued);
            channelId = queued;
            return true;
        }
        foreach (var key in _actorSoundChannels.Where(pair => !Audio.IsPlaying(pair.Value)).Select(pair => pair.Key).ToArray())
            _actorSoundChannels.Remove(key);
        if (entityChannel != 0 && !overlap)
        {
            var key = (actorId, entityChannel);
            foreach (var previous in _soundOwners.Where(pair => pair.Value == key).Select(pair => pair.Key).ToArray())
            {
                Audio.StopChannel(previous);
                _audioSources.Remove(previous);
                _listenerHeightSources.Remove(previous);
                _soundOwners.Remove(previous);
                _playingSounds.Remove(previous);
            }
            _actorSoundChannels[key] = queued;
        }
        channelId = queued;
        _soundOwners[queued] = (actorId, entityChannel);
        _playingSounds[queued] = (actorId, entityChannel, sound);
        return true;
    }

    public int GetActorSoundPlayingCount(uint actorId, string? sound = null, int entityChannel = -1)
    {
        return _playingSounds.Count(pair => Audio.IsPlaying(pair.Key) && pair.Value.ActorId == actorId &&
            (entityChannel == -1 || pair.Value.Channel == entityChannel) &&
            (sound is null || pair.Value.Sound.Equals(sound, StringComparison.OrdinalIgnoreCase)));
    }

    public int GetSoundPlayingCount(string? sound = null, int entityChannel = -1)
        => Audio.GetSoundPlayingCount(sound, entityChannel);

    public bool IsUnpositionedSoundPlaying(int entityChannel = 0, string? sound = null)
        => Audio.IsUnpositionedSoundPlaying(entityChannel, sound);

    public bool IsPositionedSoundPlaying(int entityChannel = 0, string? sound = null)
        => Audio.IsPositionedSoundPlaying(entityChannel, sound);

    public int StopSoundByName(string sound) => Audio.StopSoundByName(sound);

    public int StopUnpositionedSound(int entityChannel = 0, string? sound = null)
        => Audio.StopUnpositionedSound(entityChannel, sound);

    public bool IsActorSoundPlaying(uint actorId, int entityChannel, string? sound = null)
        => GetActorSoundPlayingCount(actorId, sound, MagicSilenceCompatibility || entityChannel == 0 ? -1 : entityChannel) != 0;

    public int StopActorSound(uint actorId, int entityChannel)
    {
        if (MagicSilenceCompatibility) entityChannel = -1;
        return StopOwnedSounds(actorId, channel => entityChannel < 0 || channel == entityChannel);
    }

    public int StopActorSounds(uint actorId, int channelMin, int channelMax)
    {
        var all = channelMin == 0 && channelMax == 0;
        if (channelMax < channelMin) (channelMin, channelMax) = (channelMax, channelMin);
        return StopOwnedSounds(actorId, channel => all || channel >= channelMin && channel <= channelMax);
    }

    private int StopOwnedSounds(uint actorId, Func<int, bool> matches)
    {
        var stopped = 0;
        foreach (var (handle, owner) in _soundOwners.ToArray())
        {
            if (owner.ActorId != actorId || !matches(owner.Channel)) continue;
            if (Audio.StopChannel(handle)) stopped++;
            _audioSources.Remove(handle);
            _listenerHeightSources.Remove(handle);
            _soundOwners.Remove(handle);
            _playingSounds.Remove(handle);
        }
        foreach (var key in _actorSoundChannels.Where(pair => !Audio.IsPlaying(pair.Value)).Select(pair => pair.Key).ToArray())
            _actorSoundChannels.Remove(key);
        return stopped;
    }

    public bool TryBindAudioSource(Guid channelId, uint actorId, out string? error, bool listenerHeight = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var actor = Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed);
        if (actor is null) { error = "audio-source-actor-missing"; return false; }
        var listener = Simulation.Players.FirstOrDefault();
        if (listener is null) { error = "audio-listener-missing"; return false; }
        var sourcePosition = SoundPosition(actor);
        var listenerPosition = SoundPosition(listener);
        if (listenerHeight) sourcePosition.Y = listenerPosition.Y;
        if (!Audio.TrySetChannelPosition(channelId, sourcePosition, listenerPosition, out error)) return false;
        Audio.SetChannelListenerHeight(channelId, listenerHeight);
        _audioSources[channelId] = actorId;
        if (listenerHeight) _listenerHeightSources.Add(channelId);
        else _listenerHeightSources.Remove(channelId);
        return true;
    }

    public bool TryRelinkActorSounds(uint fromActorId, uint? toActorId, out int relinked, out string? error)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        relinked = 0;
        error = null;
        if (toActorId is null)
        {
            var source = Simulation.Actors.FirstOrDefault(actor => actor.Id == fromActorId && !actor.Destroyed);
            if (source is null) { error = "audio-source-actor-missing"; return false; }
            relinked = DetachActorSounds(source);
            return true;
        }
        if (!Simulation.Actors.Any(actor => actor.Id == toActorId && !actor.Destroyed))
        { error = "audio-destination-actor-missing"; return false; }
        foreach (var (handle, actorId) in _audioSources.ToArray())
        {
            if (actorId != fromActorId || !Audio.IsPlaying(handle)) continue;
            Audio.RelinkChannelSource(handle, toActorId.Value);
            _audioSources[handle] = toActorId.Value;
            if (_soundOwners.TryGetValue(handle, out var owner))
                _soundOwners[handle] = (toActorId.Value, owner.Channel);
            if (_playingSounds.TryGetValue(handle, out var playing))
                _playingSounds[handle] = (toActorId.Value, playing.Channel, playing.Sound);
            relinked++;
        }
        foreach (var (key, handle) in _actorSoundChannels.Where(pair => pair.Key.Item1 == fromActorId).ToArray())
        {
            _actorSoundChannels.Remove(key);
            if (Audio.IsPlaying(handle)) _actorSoundChannels[(toActorId.Value, key.Item2)] = handle;
        }
        return true;
    }

    private void OnActorDestroyed(Actor actor) => DetachActorSounds(actor);

    private int DetachActorSounds(Actor actor)
    {
        var detached = 0;
        var position = SoundPosition(actor);
        foreach (var (handle, actorId) in _audioSources.ToArray())
        {
            if (actorId != actor.Id) continue;
            if (Audio.IsPlaying(handle)) detached++;
            Audio.DetachChannelSource(handle, SoundCutoffCompatibility, position);
            _audioSources.Remove(handle);
            _listenerHeightSources.Remove(handle);
            _soundOwners.Remove(handle);
            _playingSounds.Remove(handle);
        }
        foreach (var key in _actorSoundChannels.Keys.Where(key => key.Item1 == actor.Id).ToArray())
            _actorSoundChannels.Remove(key);
        return detached;
    }

    private static System.Numerics.Vector3 SoundPosition(Actor actor)
    {
        var position = actor.SoundPos();
        return new(position.X, position.Y, position.Z);
    }

    private bool IsInSilentSector(Actor actor)
        => (uint)actor.SectorIndex < (uint)Simulation.Level.Sectors.Count &&
            Simulation.Level.Sectors[actor.SectorIndex].Silent;

    public static ClientHost DemoRoom(int width = 32, int height = 32)
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things = new[] { new LevelThing { Type = 1, X = 32, Y = 64 } },
            Lines = new[]
            {
                new LevelLine
                {
                    X1 = 64,
                    Y1 = 0,
                    X2 = 64,
                    Y2 = 128,
                    SideBack = LevelLine.NoSide,
                },
            },
        };
        return new ClientHost(AuthoritySimulation.Start(level), width, height);
    }

    public void Tick(PlayerCommand command)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var player = Simulation.Players.FirstOrDefault();
        if (player != null && !GamePaused)
            Simulation.QueueCommand(player.PlayerNum, command);
        if (!GamePaused)
        {
            Simulation.Tick();
            ZScript.Tick(Simulation);
        }
        ConsumeSoundRequests();
        foreach (var (channel, actorId) in _audioSources.ToArray())
        {
            var actor = Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed);
            if (Audio.IsPlaying(channel) && actor is not null) continue;
            if (actor is null) Audio.DetachChannelSource(channel, SoundCutoffCompatibility);
            _audioSources.Remove(channel);
            _listenerHeightSources.Remove(channel);
            _soundOwners.Remove(channel);
            _playingSounds.Remove(channel);
        }
        var listener = Simulation.Players.FirstOrDefault();
        if (listener is not null)
        {
            var position = SoundPosition(listener);
            foreach (var (channel, actorId) in _audioSources.ToArray())
            {
                var actor = Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed);
                if (actor is null) continue;
                var sourcePosition = SoundPosition(actor);
                if (_listenerHeightSources.Contains(channel)) sourcePosition.Y = position.Y;
                if (!Audio.TrySetChannelPosition(channel, sourcePosition, position, out var sourceError))
                    throw new InvalidOperationException($"Audio source update failed: {sourceError}");
            }
            if (!Audio.TryUpdateListenerPosition(position, out var error))
                throw new InvalidOperationException($"Audio listener update failed: {error}");
            AudioListenerPosition = position;
        }
        else AudioListenerPosition = null;
        if (UseHardware)
        {
            Frame.Clear(0, 0, 0);
            Hardware.BeginFrame();
            Hardware.DrawQuad(0, Frame.Height / 4, Frame.Width, Frame.Height / 2, HardwareWall);
            Hardware.Blit(Frame);
            if (player is not null)
            {
                SoftwarePainBlend.Apply(Frame, player.DamageCount, player.GetPainFlash(),
                    BloodFadeScalar, SmoothPain, PainSmoothStrength);
                SoftwarePoisonBlend.Apply(Frame, player.PoisonCount, PoisonPaletteFlash);
            }
        }
        else
        {
            SoftwareView.Draw(Simulation, Frame, BloodFadeScalar, SmoothPain, PainSmoothStrength, PoisonPaletteFlash);
        }

        if (!GamePaused && !Simulation.Exited) Audio.Play(new short[] { 1000, 1000, 1000, 1000 });
        LastMix = Audio.Mix(4);
    }
}
