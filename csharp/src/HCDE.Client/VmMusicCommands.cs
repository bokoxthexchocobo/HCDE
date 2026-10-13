namespace HCDE.Client;

public static class VmMusicCommands
{
    // Arguments include the command name at index zero, as in native console argv.
    public static void ChangeArguments<TSong>(VmMusicPlaybackState<TSong> state, VmMusicPlaylist playlist,
        bool enabled, IReadOnlyList<string> arguments, Func<string, int, bool> changeMusic, Action<string> report)
        where TSong : class
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(changeMusic);
        Change(state, playlist, enabled, arguments.Count > 1 ? arguments[1] : null, 0,
            (name, _) => changeMusic(name, arguments.Count > 2 ? VmMusicCommandInteger.Parse(arguments[2]) : 0), report);
    }

    public static void Change<TSong>(VmMusicPlaybackState<TSong> state, VmMusicPlaylist playlist,
        bool enabled, string? name, int order, Func<string, int, bool> changeMusic, Action<string> report)
        where TSong : class
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(playlist);
        ArgumentNullException.ThrowIfNull(changeMusic);
        ArgumentNullException.ThrowIfNull(report);
        if (!enabled)
        {
            report("Music is disabled\n");
            return;
        }
        if (name is not null)
        {
            playlist.Clear();
            changeMusic(name, order);
        }
        else report(state.Name.Length != 0 ? $"currently playing {state.Name}\n" : "no music playing\n");
    }

    public static bool Start(string name, Func<string, int, bool, bool> changeMusic)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(changeMusic);
        return changeMusic(name, 0, false);
    }
}
