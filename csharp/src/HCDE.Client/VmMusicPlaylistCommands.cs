namespace HCDE.Client;

public sealed class VmMusicPlaylistCommands
{
    private readonly VmMusicPlaylist playlist;
    private readonly Action<bool> activate;
    private readonly Action<string> report;

    public VmMusicPlaylistCommands(VmMusicPlaylist playlist, Action<bool> activate, Action<string> report)
    {
        ArgumentNullException.ThrowIfNull(playlist);
        ArgumentNullException.ThrowIfNull(activate);
        ArgumentNullException.ThrowIfNull(report);
        this.playlist = playlist;
        this.activate = activate;
        this.report = report;
    }

    public void Position(string? argument)
    {
        if (!Check() || argument is null) return;
        playlist.SetPosition(checked(VmMusicCommandInteger.Parse(argument) - 1));
        activate(false);
    }

    public void Next()
    {
        if (!Check()) return;
        playlist.Advance();
        activate(false);
    }

    public void Previous()
    {
        if (!Check()) return;
        playlist.Backup();
        activate(true);
    }

    public void Status()
    {
        if (Check()) report($"Song {playlist.Position + 1} of {playlist.Count}:\n{playlist.GetSong(playlist.Position)}\n");
    }

    public bool Load(string path, string? option, Func<string, byte[]?> readFile, Func<int> nextRandom)
    {
        if (!playlist.ChangeList(path, readFile))
        {
            report($"Could not load playlist: {path}");
            return false;
        }
        if (option is not null)
        {
            if (option.Equals("shuffle", StringComparison.OrdinalIgnoreCase)) playlist.Shuffle(nextRandom);
            else playlist.SetPosition(VmMusicCommandInteger.Parse(option));
        }
        activate(false);
        return true;
    }

    private bool Check()
    {
        if (playlist.Count != 0) return true;
        report("No playlist is playing.\n");
        return false;
    }

}
