namespace HCDE.Client;

public enum VmMusicChangeDisposition { Disabled, Handled, Continue }
public readonly record struct VmMusicChangeRequest(string? Name, int Order);
public readonly record struct VmMusicChangePreparation(VmMusicChangeDisposition Disposition, VmMusicChangeRequest Request);

public sealed class VmMusicPlaybackState<TSong> where TSong : class
{
    public string Name { get; private set; }
    public string LastSong { get; private set; } = "";
    public TSong? Song { get; private set; }
    public int BaseOrder { get; private set; }
    public bool Loop { get; private set; }

    public VmMusicPlaybackState(string name, TSong? song, int baseOrder = 0, bool loop = false)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
        Song = song;
        BaseOrder = baseOrder;
        Loop = loop;
    }

    public void Stop(bool force, int playlistCount, Action resumeMusic, Action stopStream,
        Action<TSong> stopSong, Action<TSong> closeSong)
    {
        ArgumentNullException.ThrowIfNull(resumeMusic);
        ArgumentNullException.ThrowIfNull(stopStream);
        ArgumentNullException.ThrowIfNull(stopSong);
        ArgumentNullException.ThrowIfNull(closeSong);
        if ((!force && playlistCount != 0) || Name.Length == 0) return;
        if (Song is { } song)
        {
            resumeMusic();
            stopStream();
            stopSong(song);
            Song = null;
            closeSong(song);
        }
        LastSong = Name;
        Name = "";
    }

    public void StopCommand(VmMusicPlaylist playlist, Action stopMusic)
    {
        ArgumentNullException.ThrowIfNull(playlist);
        ArgumentNullException.ThrowIfNull(stopMusic);
        playlist.Clear();
        stopMusic();
        LastSong = "";
    }

    public string CurrentMusicStatus() => Name.Length != 0
        ? $"Currently playing music '{Name}'\n" : "Currently no music playing\n";

    public (string? Name, int Order) GetMusic() => Name.Length != 0 ? (Name, BaseOrder) : (null, 0);

    public string GetStatistics(Func<TSong, string> getStatistics)
    {
        ArgumentNullException.ThrowIfNull(getStatistics);
        return Song is { } song ? getStatistics(song) : "No song playing";
    }

    public bool RecordDeferred(string name, int order, bool loop, float musicVolume, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!(musicVolume <= 0 || !enabled)) return false;
        if (Song is not null) throw new InvalidOperationException("The previous song must be closed before recording deferred music.");
        Loop = loop;
        Name = name;
        BaseOrder = order;
        LastSong = name;
        return true;
    }

    public bool StartOpened(string name, TSong? song, int order, bool loop,
        Func<TSong, bool, float, int, bool> startMusic, Action reportStartFailure, Action createStream)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(startMusic);
        ArgumentNullException.ThrowIfNull(reportStartFailure);
        ArgumentNullException.ThrowIfNull(createStream);
        if (Song is not null) throw new InvalidOperationException("The previous song must be closed before adopting a new song.");
        Song = song;
        Loop = loop;
        Name = name;
        BaseOrder = 0;
        LastSong = "";
        if (song is null) return false;
        if (!startMusic(song, loop, 1, order))
        {
            reportStartFailure();
            return false;
        }
        createStream();
        BaseOrder = order;
        return true;
    }

    public VmMusicChangePreparation PrepareChange(bool enabled, int playlistCount, bool force,
        VmMusicChangeRequest request, Action<bool> stopMusic,
        Func<VmMusicChangeRequest, VmMusicChangeRequest>? lookupFileName = null)
    {
        ArgumentNullException.ThrowIfNull(stopMusic);
        if (!enabled) return new(VmMusicChangeDisposition.Disabled, request);
        if (!force && playlistCount != 0) return new(VmMusicChangeDisposition.Handled, request);
        if (lookupFileName is not null) request = lookupFileName(request);
        if (!string.IsNullOrEmpty(request.Name)) return new(VmMusicChangeDisposition.Continue, request);
        stopMusic(true);
        Name = LastSong = "";
        return new(VmMusicChangeDisposition.Handled, request);
    }

    public bool TryReuse(string name, int order, bool loop, Func<TSong, bool> isLooping,
        Func<TSong, int, bool> setSubsong, Func<TSong, bool> isPlaying,
        Func<TSong, int, bool, bool> startSong, Action reportStartFailure, Action createStream)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(isLooping);
        ArgumentNullException.ThrowIfNull(setSubsong);
        ArgumentNullException.ThrowIfNull(isPlaying);
        ArgumentNullException.ThrowIfNull(startSong);
        ArgumentNullException.ThrowIfNull(reportStartFailure);
        ArgumentNullException.ThrowIfNull(createStream);
        if (Name.Length == 0 || Song is not { } song || !Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            || isLooping(song) != loop) return false;
        if (order != BaseOrder)
        {
            if (setSubsong(song, order)) BaseOrder = order;
        }
        else if (!isPlaying(song))
        {
            if (!startSong(song, order, loop)) reportStartFailure();
            createStream();
        }
        return true;
    }

    public void MidiDeviceChanged(Func<TSong, bool> isMidi, Func<TSong, bool> isPlaying,
        Action<bool> stopMusic, Func<string, int, bool, bool> changeMusic)
    {
        ArgumentNullException.ThrowIfNull(isMidi);
        ArgumentNullException.ThrowIfNull(isPlaying);
        ArgumentNullException.ThrowIfNull(stopMusic);
        ArgumentNullException.ThrowIfNull(changeMusic);
        if (Song is not { } song || !isMidi(song) || !isPlaying(song)) return;
        var name = Name;
        var order = BaseOrder;
        var loop = Loop;
        stopMusic(true);
        changeMusic(name, order, loop);
    }

    public void Restart(float musicVolume, bool enabled, Func<string, int, bool, bool, bool> changeMusic,
        Action<bool> stopMusic)
    {
        ArgumentNullException.ThrowIfNull(changeMusic);
        ArgumentNullException.ThrowIfNull(stopMusic);
        if (musicVolume <= 0) return;
        if (LastSong.Length != 0 && enabled)
        {
            var song = LastSong;
            LastSong = "";
            changeMusic(song, BaseOrder, Loop, true);
        }
        else stopMusic(true);
    }
}
