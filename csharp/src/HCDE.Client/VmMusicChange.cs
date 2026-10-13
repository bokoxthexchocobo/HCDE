namespace HCDE.Client;

public static class VmMusicChange
{
    public static bool Change<TSong>(VmMusicPlaybackState<TSong> state, VmMusicChangeRequest request,
        bool loop, bool force, bool globallyEnabled, int playlistCount, Action<bool> stopMusic,
        Func<TSong, bool> isLooping, Func<TSong, int, bool> setSubsong, Func<TSong, bool> isPlaying,
        Func<TSong, int, bool, bool> startSong, Action reportStartFailure, Action createStream,
        Func<string, int, bool, bool> openNewSong,
        Func<VmMusicChangeRequest, VmMusicChangeRequest>? lookupFileName = null) where TSong : class
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(openNewSong);
        var preparation = state.PrepareChange(globallyEnabled, playlistCount, force, request, stopMusic, lookupFileName);
        if (preparation.Disposition == VmMusicChangeDisposition.Disabled) return false;
        if (preparation.Disposition == VmMusicChangeDisposition.Handled) return true;
        var name = preparation.Request.Name
            ?? throw new InvalidOperationException("A continuing music change requires a song name.");
        var order = preparation.Request.Order;
        // Reuse compares the lookup result before the new-song path strips file://.
        if (state.TryReuse(name, order, loop, isLooping, setSubsong, isPlaying, startSong,
            reportStartFailure, createStream)) return true;
        return openNewSong(name, order, loop);
    }
}
