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
    private readonly Action<IReadOnlyList<string>>? writeMidi;
    private readonly Action<IReadOnlyList<string>>? writeWave;
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
