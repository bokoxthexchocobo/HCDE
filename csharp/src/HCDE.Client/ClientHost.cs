using HCDE.MapLoader;
using HCDE.Playsim;
using HCDE.Scripting;

namespace HCDE.Client;

public sealed class ClientHost : IDisposable
{
    public const uint HardwareWall = 0x00B42828;
    private bool _disposed;
    private IReadOnlyList<VmScriptSourceSet> _vmScriptSources = Array.Empty<VmScriptSourceSet>();
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
            _vmColorCatalog = null;
            _vmScriptSources = Array.Empty<VmScriptSourceSet>();
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
        AudioListenerPosition = null;
        if (Simulation.Exited || _audioListenerActorId is { } cameraId &&
            !Simulation.Actors.Any(actor => actor.Id == cameraId && !actor.Destroyed))
            _audioListenerActorId = null;
    }

    public ClientHost(AuthoritySimulation simulation, int width = 32, int height = 32, string? viewPreferencesPath = null, IReadOnlyList<string>? scriptPaths = null, bool reserveCoreContainer = false, IReadOnlyList<string>? gameTypeFilters = null, IReadOnlyList<string>? blockedPatterns = null, IReadOnlyList<string>? embeddings = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? deletedGlobalLumps = null)
    {
        _vmScriptSources = scriptPaths is null ? Array.Empty<VmScriptSourceSet>() : VmArchiveScriptLoader.LoadPaths(scriptPaths, gameTypeFilters: gameTypeFilters, blockedPatterns: blockedPatterns, embeddings: embeddings, reserveCoreContainer: reserveCoreContainer, deletedGlobalLumps: deletedGlobalLumps);
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
    private uint? _audioListenerActorId;
    private byte? _audioConsolePlayer;

    public bool TrySetAudioConsolePlayer(byte? playerNum, out string? error)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        error = null;
        if (playerNum.HasValue && !Simulation.Players.Any(player => player.PlayerNum == playerNum && !player.Destroyed))
        { error = "audio-console-player-missing"; return false; }
        _audioConsolePlayer = playerNum;
        return true;
    }

    private PlayerPawn? GetAudioConsolePlayer()
        => Simulation.Players.FirstOrDefault(player => !player.Destroyed &&
            (!_audioConsolePlayer.HasValue || player.PlayerNum == _audioConsolePlayer.Value));

    public bool TrySetAudioListener(uint? actorId, out string? error)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        error = null;
        if (actorId.HasValue && !Simulation.Actors.Any(actor => actor.Id == actorId && !actor.Destroyed))
        { error = "audio-listener-actor-missing"; return false; }
        _audioListenerActorId = actorId;
        return true;
    }

    private Actor? GetAudioListener()
        => _audioListenerActorId is { } actorId
            ? Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed)
            : GetAudioConsolePlayer();

    private bool IsLocalAudioView(uint actorId)
    {
        var player = GetAudioConsolePlayer();
        var camera = GetAudioListener();
        if (player is null || camera is null) return false;
        return camera.Id == actorId || player.Id == actorId && camera is not PlayerPawn && !camera.IsMonster;
    }
    private readonly Dictionary<Guid, uint> _audioSources = new();
    private readonly HashSet<Guid> _listenerHeightSources = new();
    private readonly Dictionary<(uint ActorId, int Channel), Guid> _actorSoundChannels = new();
    private readonly Dictionary<Guid, (uint ActorId, int Channel)> _soundOwners = new();
    private readonly Dictionary<Guid, (uint? ActorId, int Channel, string Sound)> _playingSounds = new();
    private byte[]? _soundArchive;
    private VmColorCatalog? _vmColorCatalog;
    public IReadOnlyList<string> LastSoundRequestErrors { get; private set; } = Array.Empty<string>();

    public bool TrySetSoundArchive(ReadOnlySpan<byte> wad, out string? error)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!WadArchiveReader.TryReadDirectory(wad, out var entries, out error)) return false;
        VmColorCatalog? catalog = null;
        for (var index = entries.Length - 1; index >= 0; index--)
        {
            if (!entries[index].Name.Equals("X11R6RGB", StringComparison.OrdinalIgnoreCase)) continue;
            if (!WadArchiveReader.TryReadLumpData(wad, entries[index], out var data, out error)) return false;
            try { catalog = VmColorCatalog.Parse(System.Text.Encoding.UTF8.GetString(data)); }
            catch (FormatException) { error = "vm-color-catalog-invalid"; return false; }
            break;
        }
        _soundArchive = wad.ToArray();
        _vmColorCatalog = catalog;
        return true;
    }

    public IReadOnlyList<VmScriptSourceSet> VmScriptSources => _vmScriptSources;

    public void LoadVmScriptPaths(IReadOnlyList<string> paths, Version? maximumVersion = null,
        IReadOnlyList<string>? gameTypeFilters = null, IReadOnlyList<string>? blockedPatterns = null,
        IReadOnlyList<string>? embeddings = null, Func<string, string, bool>? directoryFilenameCheck = null,
        Action<string>? directoryWarning = null, Action<string>? archiveWarning = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var sources = VmArchiveScriptLoader.LoadPaths(paths, maximumVersion, gameTypeFilters, blockedPatterns,
            embeddings, directoryFilenameCheck, directoryWarning, archiveWarning);
        _vmScriptSources = sources;
    }

    public void LoadVmScriptArchives(IReadOnlyList<VmScriptArchive> archives, Version? maximumVersion = null,
        IReadOnlyList<string>? gameTypeFilters = null, IReadOnlyList<string>? blockedPatterns = null,
        IReadOnlyList<string>? embeddings = null, Func<string, string, bool>? directoryFilenameCheck = null, Action<string>? directoryWarning = null, Action<string>? archiveWarning = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // Publish only after every root and include has loaded successfully.
        var sources = VmArchiveScriptLoader.LoadAll(archives, maximumVersion, gameTypeFilters, blockedPatterns, embeddings, directoryFilenameCheck, directoryWarning, archiveWarning);
        _vmScriptSources = sources;
    }

    public VmMultiResult RunVm(ReadOnlySpan<int> code, ReadOnlySpan<int> constants,
        ReadOnlySpan<double> floatConstants, ReadOnlySpan<string> stringConstants, int requestedReturns = 1,
        IReadOnlyList<object?>? objectConstants = null, int frameExtraSpace = 0, VmClassMetadata? classMetadata = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return VmExec.RunWithReturns(code, constants, floatConstants, stringConstants, requestedReturns, _vmColorCatalog, objectConstants, frameExtraSpace, classMetadata);
    }

    public VmMultiResult RunVmFunction(VmScriptFunction function, IReadOnlyList<VmValue> arguments,
        int requestedReturns = 1, VmClassMetadata? classMetadata = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return VmExec.RunFunction(function, arguments, requestedReturns, _vmColorCatalog, classMetadata);
    }

    public VmMultiResult RunVmNativeFunction(VmFunction function, IReadOnlyList<VmValue> arguments, int requestedReturns = 1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return VmExec.RunNativeFunction(function, arguments, requestedReturns);
    }

    public VmMultiResult RunVmNativeFunction(VmIntegerFunction function, IReadOnlyList<int> arguments, int requestedReturns = 1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return VmExec.RunNativeFunction(function, arguments, requestedReturns);
    }

    public VmMultiResult RunVmNativeFunctionWithDefaults(VmFunction function, IReadOnlyList<VmValue> arguments, int requestedReturns = 1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return VmExec.RunNativeFunctionWithDefaults(function, arguments, requestedReturns);
    }

    public VmMultiResult RunVmNativeFunctionWithDefaults(VmIntegerFunction function, IReadOnlyList<int> arguments, int requestedReturns = 1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return VmExec.RunNativeFunctionWithDefaults(function, arguments, requestedReturns);
    }

    public VmMultiResult RunVmNamedFunction(VmScriptFunction function, IReadOnlyList<VmCallArgument> arguments,
        int requestedReturns = 1, VmClassMetadata? classMetadata = null, Version? scriptVersion = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return VmExec.RunNamedFunction(function, arguments, requestedReturns, _vmColorCatalog, classMetadata, scriptVersion);
    }

    public bool TryGetSoundLength(string sound, out double seconds, out string? error)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        seconds = 0;
        if (_soundArchive is null) { error = "audio-sound-archive-missing"; return false; }
        if (!Audio.TryGetSoundLengthMilliseconds(_soundArchive, Simulation.Level, sound, out var milliseconds, out error))
            return false;
        seconds = milliseconds / 1000.0;
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
                if (!Audio.TrySetActorSoundPitch(request.ActorId, request.Channel, pitch, out _, out var pitchError,
                    request.Sound.Length == 0 ? null : request.Sound))
                    errors.Add($"{pitchError}: {request.ActorId}");
                continue;
            }
            if (request.ChangeVolume)
            {
                if (!Audio.TrySetActorSoundVolume(request.ActorId, MagicSilenceCompatibility ? -1 : request.Channel,
                    request.Volume, out _, out var volumeError))
                    errors.Add($"{volumeError}: {request.ActorId}");
                continue;
            }
            if (request.Stop)
            {
                if (request.StopChannelMax is { } maximum) StopActorSounds(request.ActorId, request.Channel, maximum);
                else StopActorSound(request.ActorId, request.Channel, request.Sound.Length == 0 ? null : request.Sound);
                continue;
            }
            const int supported = (int)(SimulationSoundFlags.ListenerZ | SimulationSoundFlags.MaybeLocal | SimulationSoundFlags.Loop | SimulationSoundFlags.NoStop | SimulationSoundFlags.Overlap | SimulationSoundFlags.Local | SimulationSoundFlags.Singular | SimulationSoundFlags.Ui | SimulationSoundFlags.NoPause | SimulationSoundFlags.Force | SimulationSoundFlags.NoRumble);
            if ((request.Flags & ~supported) != 0)
            {
                errors.Add($"audio-request-flags-unsupported: {request.Sound}");
                continue;
            }
            var listener = GetAudioListener();
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
            if (request.OnlyIfChannelFree && (local
                ? Audio.IsUnpositionedSoundPlaying(request.Channel)
                : IsActorSoundPlaying(request.ActorId, request.Channel))) continue;
            if (!request.NonPositional && (noStop || request.OnlyIfSoundNotPlaying) && (local
                ? Audio.IsUnpositionedSoundPlaying(request.Channel, request.Sound)
                : IsActorSoundPlaying(request.ActorId, request.Channel, request.Sound))) continue;
            if (local)
            {
                if (!request.NonPositional && !IsLocalAudioView(request.ActorId)) continue;
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
        var listener = GetAudioListener();
        var position = actor is not null ? SoundPosition(actor) :
            new System.Numerics.Vector3(detachedOrigin!.Value.X, detachedOrigin.Value.Y, detachedOrigin.Value.Z);
        if (listenerHeight && listener is not null) position.Y = SoundPosition(listener).Y;
        foreach (var source in Simulation.Actors.Where(source => !source.Destroyed))
            Audio.UpdateActorSoundOrigin(source.Id, SoundPosition(source), listener is null ? null : SoundPosition(listener));
        if (!Audio.TryPlayWadSound(wad, level, sound, out var queued, out error,
            nextRandom, volume, loop, pitch, attenuation: attenuation, singular: singular, noPause: noPause, force: force,
            sourcePosition: position, sourceActorId: actor?.Id, entityChannel: entityChannel, listenerSource: actor == listener,
            startTime: startTime, overlap: overlap)) return false;
        RemoveStoppedSoundRecords();
        if (queued == Guid.Empty) return true;
        if (actor is not null && !TryBindAudioSource(queued, actorId, out error, listenerHeight))
        {
            Audio.StopChannel(queued);
            return false;
        }
        if (actor is null)
        {
            if (!Audio.TrySetChannelPosition(queued, position,
                listener is null ? System.Numerics.Vector3.Zero : SoundPosition(listener), out error))
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
            Audio.ReplaceActorChannel(actorId, entityChannel, queued);
            RemoveStoppedSoundRecords();
            _actorSoundChannels[key] = queued;
        }
        channelId = queued;
        _soundOwners[queued] = (actorId, entityChannel);
        _playingSounds[queued] = (actorId, entityChannel, sound);
        return true;
    }

    public int GetActorSoundPlayingCount(uint actorId, string? sound = null, int entityChannel = -1)
    {
        return Audio.GetActorSoundPlayingCount(actorId, sound, entityChannel);
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

    public int StopActorSound(uint actorId, int entityChannel, string? sound = null)
    {
        if (MagicSilenceCompatibility) entityChannel = -1;
        var stopped = sound is not null ? Audio.StopActorSoundByName(actorId, sound) : Audio.StopActorSound(actorId, entityChannel);
        RemoveStoppedSoundRecords();
        return stopped;
    }

    public int StopActorSounds(uint actorId, int channelMin, int channelMax)
    {
        var stopped = Audio.StopActorSounds(actorId, channelMin, channelMax);
        RemoveStoppedSoundRecords();
        return stopped;
    }

    private void RemoveStoppedSoundRecords()
    {
        foreach (var handle in _soundOwners.Keys.ToArray())
        {
            if (Audio.IsPlaying(handle)) continue;
            _audioSources.Remove(handle);
            _listenerHeightSources.Remove(handle);
            _soundOwners.Remove(handle);
            _playingSounds.Remove(handle);
        }
        foreach (var key in _actorSoundChannels.Where(pair => !Audio.IsPlaying(pair.Value)).Select(pair => pair.Key).ToArray())
            _actorSoundChannels.Remove(key);
    }

    public bool TryBindAudioSource(Guid channelId, uint actorId, out string? error, bool listenerHeight = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var actor = Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed);
        if (actor is null) { error = "audio-source-actor-missing"; return false; }
        var listener = GetAudioListener();
        var sourcePosition = SoundPosition(actor);
        var listenerPosition = listener is null ? System.Numerics.Vector3.Zero : SoundPosition(listener);
        if (listenerHeight && listener is not null) sourcePosition.Y = listenerPosition.Y;
        if (!Audio.TrySetChannelPosition(channelId, sourcePosition, listenerPosition, out error)) return false;
        Audio.SetChannelListenerHeight(channelId, listenerHeight);
        Audio.RelinkChannelSource(channelId, actorId);
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
        foreach (var handle in Audio.GetActorSoundChannels(fromActorId))
        {
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
        foreach (var handle in Audio.GetActorSoundChannels(actor.Id))
        {
            detached++;
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
        foreach (var source in Audio.GetActorSoundSources())
        {
            _audioSources[source.Handle] = source.ActorId;
            if (source.ListenerHeight) _listenerHeightSources.Add(source.Handle);
            else _listenerHeightSources.Remove(source.Handle);
        }
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
        var listener = Simulation.Exited ? null : GetAudioListener();
        {
            var position = listener is null ? System.Numerics.Vector3.Zero : SoundPosition(listener);
            foreach (var (channel, actorId) in _audioSources.ToArray())
            {
                var actor = Simulation.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed);
                if (actor is null) continue;
                var sourcePosition = SoundPosition(actor);
                if (listener is not null && _listenerHeightSources.Contains(channel)) sourcePosition.Y = position.Y;
                if (!Audio.TrySetChannelPosition(channel, sourcePosition, position, out var sourceError))
                    throw new InvalidOperationException($"Audio source update failed: {sourceError}");
            }
            if (!Audio.TryUpdateListenerPosition(position, out var error, listener is not null))
                throw new InvalidOperationException($"Audio listener update failed: {error}");
            AudioListenerPosition = listener is null ? null : position;
        }
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
