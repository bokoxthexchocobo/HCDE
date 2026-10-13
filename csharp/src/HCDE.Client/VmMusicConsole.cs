namespace HCDE.Client;

public sealed class VmMusicConsole<TSong> where TSong : class
{
    private readonly VmMusicPlaybackState<TSong> state;
    private readonly VmMusicPlaylist playlist;
    private readonly Func<bool> enabled;
    private readonly Func<string, int, bool> changeMusic;
    private readonly Action<bool> stopMusic;
    private readonly Action<string> report;
    private readonly VmMusicPlaylistCommands playlistCommands;
    private readonly Func<string, byte[]?>? readPlaylist;
    private readonly Func<int>? nextRandom;
    private readonly Action<IReadOnlyList<string>>? setReplayGain;
    private readonly Action<IReadOnlyList<string>>? testMusicVolume;
    private Action<IReadOnlyList<string>>? writeMidi;
    private Action<IReadOnlyList<string>>? writeWave;
    private readonly Action? listMidiDevices;

    public VmMusicConsole(VmMusicPlaybackState<TSong> state, VmMusicPlaylist playlist, Func<bool> enabled,
        Func<string, int, bool> changeMusic, Action<bool> stopMusic, Action<bool> activatePlaylist, Action<string> report,
        Func<string, byte[]?>? readPlaylist = null, Func<int>? nextRandom = null,
        Action<IReadOnlyList<string>>? setReplayGain = null,
        Action<IReadOnlyList<string>>? testMusicVolume = null,
        Action<IReadOnlyList<string>>? writeMidi = null,
        Action<IReadOnlyList<string>>? writeWave = null,
        Action? listMidiDevices = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(playlist);
        ArgumentNullException.ThrowIfNull(enabled);
        ArgumentNullException.ThrowIfNull(changeMusic);
        ArgumentNullException.ThrowIfNull(stopMusic);
        ArgumentNullException.ThrowIfNull(report);
        this.state = state;
        this.playlist = playlist;
        this.enabled = enabled;
        this.changeMusic = changeMusic;
        this.stopMusic = stopMusic;
        this.report = report;
        this.readPlaylist = readPlaylist;
        this.nextRandom = nextRandom;
        this.setReplayGain = setReplayGain;
        this.testMusicVolume = testMusicVolume;
        this.writeMidi = writeMidi;
        this.writeWave = writeWave;
        this.listMidiDevices = listMidiDevices;
        playlistCommands = new(playlist, activatePlaylist, report);
    }

    public void ConfigureMidiExport(Func<string, int> findMusicNamespace, Func<string, int> findFullName,
        Func<int, byte[]?> readResource)
    {
        ArgumentNullException.ThrowIfNull(findMusicNamespace);
        ArgumentNullException.ThrowIfNull(findFullName);
        ArgumentNullException.ThrowIfNull(readResource);
        var factory = new VmMusicMidiSourceFactory();
        writeMidi = arguments => VmMusicMidiExport.WriteMidiFile(arguments,
            name => VmMusicMidiSource.Open(name, state.Name, findMusicNamespace, findFullName,
                readResource, factory.Create, () => factory.LastError, report),
            () => factory.LastError, report);
    }

    public void ConfigureFmWaveExport(int preferredDevice, Func<string, int> findMusicNamespace,
        Func<string, int> findFullName, Func<int, byte[]?> readResource,
        Func<VmMusicWaveRequest, VmMusicFmSessionDevice> createDevice,
        Func<string, int, bool, bool, bool> restoreMusic,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(findMusicNamespace);
        ArgumentNullException.ThrowIfNull(findFullName);
        ArgumentNullException.ThrowIfNull(readResource);
        var factory = new VmMusicMidiSourceFactory();
        ConfigureFmWaveExport(preferredDevice,
            name => VmMusicMidiSource.Open(name, state.Name, findMusicNamespace, findFullName,
                readResource, factory.Create, () => factory.LastError, report),
            createDevice, restoreMusic, () => factory.LastError, cancellationToken);
    }

    public void ConfigureFmWaveExport(int preferredDevice, Func<string, IVmMusicSmfSource?> openSource,
        Func<VmMusicWaveRequest, VmMusicFmSessionDevice> createDevice,
        Func<string, int, bool, bool, bool> restoreMusic, Func<string> lastError,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(openSource);
        ArgumentNullException.ThrowIfNull(createDevice);
        ArgumentNullException.ThrowIfNull(restoreMusic);
        ArgumentNullException.ThrowIfNull(lastError);
        writeWave = arguments => VmMusicWaveExport.WriteFmWave(state, arguments, preferredDevice,
            openSource, stopMusic, createDevice, restoreMusic, lastError, report, cancellationToken);
    }

    public void ConfigureWildMidiWaveExport(int preferredDevice, Func<string, int> findMusicNamespace,
        Func<string, int> findFullName, Func<int, byte[]?> readResource,
        Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer> createRenderer,
        Func<string, int, bool, bool, bool> restoreMusic,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(findMusicNamespace);
        ArgumentNullException.ThrowIfNull(findFullName);
        ArgumentNullException.ThrowIfNull(readResource);
        var factory = new VmMusicMidiSourceFactory();
        ConfigureWildMidiWaveExport(preferredDevice,
            name => VmMusicMidiSource.Open(name, state.Name, findMusicNamespace, findFullName,
                readResource, factory.Create, () => factory.LastError, report),
            createRenderer, restoreMusic, () => factory.LastError, cancellationToken);
    }

    public void ConfigureWildMidiWaveExport(int preferredDevice, Func<string, IVmMusicSmfSource?> openSource,
        Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer> createRenderer,
        Func<string, int, bool, bool, bool> restoreMusic, Func<string> lastError,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(openSource);
        ArgumentNullException.ThrowIfNull(createRenderer);
        ArgumentNullException.ThrowIfNull(restoreMusic);
        ArgumentNullException.ThrowIfNull(lastError);
        writeWave = arguments => VmMusicWaveExport.WriteWildMidiWave(state, arguments, preferredDevice,
            openSource, stopMusic, createRenderer, restoreMusic, lastError, report, cancellationToken);
    }

    public void ConfigureSynthWaveExport(int preferredDevice, Func<string, IVmMusicSmfSource?> openSource,
        VmMusicMidiDeviceKind defaultDevice,
        Func<VmMusicWaveRequest, VmMusicFmSessionDevice>? createFm,
        Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer>? createWildMidi,
        Func<string, int, bool, bool, bool> restoreMusic, Func<string> lastError,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(openSource);
        ArgumentNullException.ThrowIfNull(restoreMusic);
        ArgumentNullException.ThrowIfNull(lastError);
        var dump = VmMusicSynthWaveDump.CreateRouter(defaultDevice, createFm, createWildMidi, cancellationToken);
        writeWave = arguments => VmMusicWaveExport.WriteWave(state, arguments, preferredDevice,
            openSource, stopMusic, dump, restoreMusic, lastError, report);
    }

    public void ConfigureSynthWaveExport(int preferredDevice, Func<string, int> findMusicNamespace,
        Func<string, int> findFullName, Func<int, byte[]?> readResource,
        VmMusicMidiDeviceKind defaultDevice,
        Func<VmMusicWaveRequest, VmMusicFmSessionDevice>? createFm,
        Func<VmMusicWaveRequest, VmMusicWildMidiManagedRenderer>? createWildMidi,
        Func<string, int, bool, bool, bool> restoreMusic, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(findMusicNamespace);
        ArgumentNullException.ThrowIfNull(findFullName);
        ArgumentNullException.ThrowIfNull(readResource);
        var factory = new VmMusicMidiSourceFactory();
        ConfigureSynthWaveExport(preferredDevice,
            name => VmMusicMidiSource.Open(name, state.Name, findMusicNamespace, findFullName,
                readResource, factory.Create, () => factory.LastError, report),
            defaultDevice, createFm, createWildMidi, restoreMusic, () => factory.LastError, cancellationToken);
    }

    // Input is already tokenized and includes the command name at index zero.
    public bool TryExecute(IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Count == 0) return false;
        switch (arguments[0].ToLowerInvariant())
        {
            case "snd_listmididevices":
                (listMidiDevices ?? throw new InvalidOperationException("MIDI device listing is not configured."))();
                break;
            case "writewave":
                (writeWave ?? throw new InvalidOperationException("Wave export command is not configured."))(arguments);
                break;
            case "writemidi":
                (writeMidi ?? throw new InvalidOperationException("MIDI export command is not configured."))(arguments);
                break;
            case "testmusicvol":
                (testMusicVolume ?? throw new InvalidOperationException("Relative volume command is not configured."))(arguments);
                break;
            case "playlist":
                if (arguments.Count is < 2 or > 3) report("playlist <playlist.m3u> [<position>|shuffle]\n");
                else playlistCommands.Load(arguments[1], arguments.Count == 3 ? arguments[2] : null,
                    readPlaylist ?? throw new InvalidOperationException("Playlist reading is not configured."),
                    nextRandom ?? (() => throw new InvalidOperationException("Playlist random source is not configured.")));
                break;
            case "setreplaygain":
                (setReplayGain ?? throw new InvalidOperationException("Replay gain command is not configured."))(arguments);
                break;
            case "changemus":
                VmMusicCommands.ChangeArguments(state, playlist, enabled(), arguments, changeMusic, report);
                break;
            case "stopmus":
                state.StopCommand(playlist, () => stopMusic(false));
                break;
            case "currentmusic":
                report(state.CurrentMusicStatus());
                break;
            case "playlistpos":
                playlistCommands.Position(arguments.Count > 1 ? arguments[1] : null);
                break;
            case "playlistnext":
                playlistCommands.Next();
                break;
            case "playlistprev":
                playlistCommands.Previous();
                break;
            case "playliststatus":
                playlistCommands.Status();
                break;
            default:
                return false;
        }
        return true;
    }
}
