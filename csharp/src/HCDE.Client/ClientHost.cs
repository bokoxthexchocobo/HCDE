using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client;

public sealed class ClientHost
{
    public const uint HardwareWall = 0x00B42828;

    public ClientHost(AuthoritySimulation simulation, int width = 32, int height = 32)
    {
        Simulation = simulation;
        Frame = new SoftwareFrame(width, height);
        Audio = new AudioMixer();
        ZScript = new ZScriptVm();
        Hardware = new RecordedHardwareDevice();
    }

    public AuthoritySimulation Simulation { get; }
    public SoftwareFrame Frame { get; }
    public AudioMixer Audio { get; }
    public ZScriptVm ZScript { get; }
    public RecordedHardwareDevice Hardware { get; }
    public bool UseHardware { get; set; }
    public bool SilentPickupCompatibility { get; set; }
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
        if (!WadArchiveReader.TryReadDirectory(wad, out _, out error)) return false;
        _soundArchive = wad.ToArray();
        return true;
    }

    private void ConsumeSoundRequests()
    {
        if (_soundArchive is null) return;
        var errors = new List<string>();
        foreach (var handle in _playingSounds.Keys.Where(handle => !Audio.IsPlaying(handle)).ToArray())
            _playingSounds.Remove(handle);
        foreach (var request in Simulation.DrainSoundRequests())
        {
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
                StopActorSound(request.ActorId, request.Channel);
                continue;
            }
            const int supported = (int)(SimulationSoundFlags.ListenerZ | SimulationSoundFlags.MaybeLocal | SimulationSoundFlags.Loop | SimulationSoundFlags.NoStop | SimulationSoundFlags.Overlap | SimulationSoundFlags.Local | SimulationSoundFlags.Singular | SimulationSoundFlags.Ui | SimulationSoundFlags.NoPause | SimulationSoundFlags.Force);
            if ((request.Flags & ~supported) != 0)
            {
                errors.Add($"audio-request-flags-unsupported: {request.Sound}");
                continue;
            }
            var listener = Simulation.Players.FirstOrDefault();
            var sourceActor = Simulation.Actors.FirstOrDefault(actor => actor.Id == request.ActorId && !actor.Destroyed);
            if (sourceActor is not null && IsInSilentSector(sourceActor)) continue;
            if (SilentPickupCompatibility && (request.Flags & (int)SimulationSoundFlags.MaybeLocal) != 0 &&
                listener?.Id != request.ActorId) continue;
            var local = request.Local || (request.Flags & (int)SimulationSoundFlags.Local) != 0;
            var loop = request.Loop || (request.Flags & (int)SimulationSoundFlags.Loop) != 0;
            var noStop = request.Loop || (request.Flags & (int)SimulationSoundFlags.NoStop) != 0;
            var singular = (request.Flags & (int)SimulationSoundFlags.Singular) != 0;
            var sourceId = local ? (uint?)null : request.ActorId;
            if (noStop && _playingSounds.Any(pair => Audio.IsPlaying(pair.Key) &&
                pair.Value.ActorId == sourceId && (request.Channel == 0 || !local && MagicSilenceCompatibility || pair.Value.Channel == request.Channel) &&
                pair.Value.Sound.Equals(request.Sound, StringComparison.OrdinalIgnoreCase))) continue;
            if (local)
            {
                if (listener?.Id != request.ActorId) continue;
                if (!Audio.TryPlayWadSound(_soundArchive, Simulation.Level, request.Sound, out var localHandle,
                    out var localError, volume: request.Volume, loop: loop, attenuation: 0, singular: singular, noPause: (request.Flags & 96) != 0, force: (request.Flags & 65536) != 0))
                {
                    errors.Add($"{localError}: {request.Sound}");
                    continue;
                }
                if (localHandle != Guid.Empty) _playingSounds[localHandle] = (null, request.Channel, request.Sound);
                continue;
            }
            var attenuation = listener?.Id == request.ActorId ? 0 : request.Attenuation;
            if (!TryPlayActorWadSound(_soundArchive, Simulation.Level, request.Sound, request.ActorId,
                out _, out var error, loop, request.Volume, attenuation, entityChannel: request.Channel,
                overlap: (request.Flags & (int)SimulationSoundFlags.Overlap) != 0, singular: singular,
                listenerHeight: (request.Flags & (int)SimulationSoundFlags.ListenerZ) != 0, noPause: (request.Flags & 96) != 0, force: (request.Flags & 65536) != 0))
                errors.Add($"{error}: {request.Sound}");
        }
        LastSoundRequestErrors = errors.AsReadOnly();
    }

    public bool TryPlayActorWadSound(ReadOnlySpan<byte> wad, PlayLevel level, string sound, uint actorId,
        out Guid channelId, out string? error, bool loop = false, float volume = 1, float attenuation = 1,
        Func<int>? nextRandom = null, float? pitch = null, int entityChannel = 0, bool overlap = false, bool singular = false, bool listenerHeight = false, bool noPause = false, bool force = false)
    {
        channelId = Guid.Empty;
        if (entityChannel is < 0 or > 7) { error = "audio-entity-channel-invalid"; return false; }
        var actor = Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed);
        if (actor is null)
        { error = "audio-source-actor-missing"; return false; }
        if (IsInSilentSector(actor)) { error = null; return true; }
        if (MagicSilenceCompatibility) entityChannel = 1;
        var listener = Simulation.Players.FirstOrDefault();
        if (listener is null) { error = "audio-listener-missing"; return false; }
        var position = SoundPosition(actor);
        if (listenerHeight) position.Y = SoundPosition(listener).Y;
        if (!Audio.TryPlayWadSound(wad, level, sound, out var queued, out error,
            nextRandom, volume, loop, pitch, attenuation: attenuation, singular: singular, noPause: noPause, force: force,
            sourcePosition: position, sourceActorId: actorId, entityChannel: entityChannel, listenerSource: actor == listener)) return false;
        if (queued == Guid.Empty) return true;
        if (!TryBindAudioSource(queued, actorId, out error, listenerHeight))
        {
            Audio.StopChannel(queued);
            return false;
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

    public bool IsActorSoundPlaying(uint actorId, int entityChannel, string sound)
        => _playingSounds.Any(pair => Audio.IsPlaying(pair.Key) && pair.Value.ActorId == actorId &&
            (MagicSilenceCompatibility || entityChannel == 0 || pair.Value.Channel == entityChannel) &&
            pair.Value.Sound.Equals(sound, StringComparison.OrdinalIgnoreCase));

    public int StopActorSound(uint actorId, int entityChannel)
    {
        if (MagicSilenceCompatibility) entityChannel = -1;
        var stopped = 0;
        foreach (var (handle, owner) in _soundOwners.ToArray())
        {
            if (owner.ActorId != actorId || entityChannel >= 0 && owner.Channel != entityChannel) continue;
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
        var actor = Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed);
        if (actor is null) { error = "audio-source-actor-missing"; return false; }
        var listener = Simulation.Players.FirstOrDefault();
        if (listener is null) { error = "audio-listener-missing"; return false; }
        var sourcePosition = SoundPosition(actor);
        var listenerPosition = SoundPosition(listener);
        if (listenerHeight) sourcePosition.Y = listenerPosition.Y;
        if (!Audio.TrySetChannelPosition(channelId, sourcePosition, listenerPosition, out error)) return false;
        _audioSources[channelId] = actorId;
        if (listenerHeight) _listenerHeightSources.Add(channelId);
        else _listenerHeightSources.Remove(channelId);
        return true;
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
        }
        else
        {
            SoftwareView.Draw(Simulation, Frame);
        }

        if (!GamePaused) Audio.Play(new short[] { 1000, 1000, 1000, 1000 });
        LastMix = Audio.Mix(4);
    }
}
