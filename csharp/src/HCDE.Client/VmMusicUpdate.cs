namespace HCDE.Client;

public static class VmMusicUpdate
{
    public static void Tick(bool hasSong, Action updateSong, Func<bool> isPlaying,
        Func<int> playlistCount, Action advancePlaylist, Action<bool> activatePlaylist, Action<bool> stopMusic)
    {
        ArgumentNullException.ThrowIfNull(updateSong);
        ArgumentNullException.ThrowIfNull(isPlaying);
        ArgumentNullException.ThrowIfNull(playlistCount);
        ArgumentNullException.ThrowIfNull(advancePlaylist);
        ArgumentNullException.ThrowIfNull(activatePlaylist);
        ArgumentNullException.ThrowIfNull(stopMusic);
        if (!hasSong) return;
        updateSong();
        // The playback query also gives backends an opportunity to reconnect streams.
        if (isPlaying()) return;
        if (playlistCount() != 0)
        {
            advancePlaylist();
            activatePlaylist(false);
        }
        else stopMusic(true);
    }
}
