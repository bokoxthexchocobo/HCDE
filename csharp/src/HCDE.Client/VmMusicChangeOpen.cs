namespace HCDE.Client;

public static class VmMusicChangeOpen
{
    public static bool OpenAndStart<TSong>(VmMusicPlaybackState<TSong> state, VmMusicResources resources,
        string name, int order, bool loop, float musicVolume, bool enabled, Action<bool> stopMusic,
        Func<string, VmMusicResolvedSettings> resolveSettings, Action<float> setMusicVolume,
        Action<string, int, string> checkReplayGain, Action<int> setModulePlayer,
        Func<VmMusicByteReader, int, string, TSong?> openSong, Func<TSong, bool, float, int, bool> startMusic,
        Action<string> reportOpenFailure, Action reportStartFailure, Action createStream,
        bool transferReaderOwnership = false) where TSong : class
    {
        ArgumentNullException.ThrowIfNull(resolveSettings);
        ArgumentNullException.ThrowIfNull(setMusicVolume);
        ArgumentNullException.ThrowIfNull(checkReplayGain);
        ArgumentNullException.ThrowIfNull(setModulePlayer);
        ArgumentNullException.ThrowIfNull(openSong);
        return OpenAndStart(state, resources, name, order, loop, musicVolume, enabled, stopMusic,
            (reader, musicName) =>
            {
                var settings = resolveSettings(musicName);
                VmMusicResourceSettings.ApplyVolume(settings, setMusicVolume,
                    (device, arguments) => checkReplayGain(musicName, device, arguments));
                setModulePlayer(settings.ModulePlayer);
                return openSong(reader, settings.Device, settings.Arguments);
            }, startMusic, reportOpenFailure, reportStartFailure, createStream, transferReaderOwnership);
    }

    public static bool OpenAndStart<TSong>(VmMusicPlaybackState<TSong> state, VmMusicResources resources,
        string name, int order, bool loop, float musicVolume, bool enabled, Action<bool> stopMusic,
        Func<VmMusicByteReader, string, TSong?> openSong, Func<TSong, bool, float, int, bool> startMusic,
        Action<string> reportOpenFailure, Action reportStartFailure, Action createStream,
        bool transferReaderOwnership = false) where TSong : class
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(stopMusic);
        ArgumentNullException.ThrowIfNull(openSong);
        ArgumentNullException.ThrowIfNull(startMusic);
        ArgumentNullException.ThrowIfNull(reportOpenFailure);
        ArgumentNullException.ThrowIfNull(reportStartFailure);
        ArgumentNullException.ThrowIfNull(createStream);
        name = VmMusicResources.ChangeName(name);
        var reader = resources.Open(name);
        if (reader is null) return false;
        var transferred = false;
        try
        {
            stopMusic(true);
            if (state.RecordDeferred(name, order, loop, musicVolume, enabled)) return true;
            // Successful owning backends must close the reader when their song closes.
            // Borrowing backends must consume or copy input before returning.
            var song = openSong(reader, name);
            transferred = transferReaderOwnership && song is not null;
            if (song is null) reportOpenFailure(name);
            return state.StartOpened(name, song, order, loop, startMusic, reportStartFailure, createStream);
        }
        finally
        {
            if (!transferred) reader.Dispose();
        }
    }
}
