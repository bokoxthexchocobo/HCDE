namespace HCDE.Client;

public delegate bool VmPlaylistChangeMusic(string song, int subsong, bool loop, bool force);

public sealed class VmMusicPlaylist
{
    private readonly List<string> songs;
    public int Count => songs.Count;
    public int Position { get; private set; }

    public VmMusicPlaylist(IEnumerable<string> songs)
    {
        ArgumentNullException.ThrowIfNull(songs);
        this.songs = songs.Select(song => song ?? throw new ArgumentException("Songs cannot be null.", nameof(songs))).ToList();
    }

    public int SetPosition(int position) => Position = position >= 0 && position < Count ? position : 0;
    public bool ChangeList(string path, Func<string, byte[]?> readFile, bool? windowsPaths = null,
        bool signedCharacters = true, bool decodeUtf8 = true)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(readFile);
        var data = readFile(path);
        if (data is null) return false;
        Clear();
        var normalizedPath = path.Replace('\\', '/');
        var directory = normalizedPath[..(normalizedPath.LastIndexOf('/') + 1)];
        using var reader = new VmMusicByteReader(data);
        var buffer = new byte[512];
        var first = true;
        var pls = false;
        while (reader.GetsFileLine(buffer) is { } count)
        {
            var zero = buffer.AsSpan(0, count).IndexOf((byte)0);
            if (zero >= 0) count = zero;
            var start = 0;
            while (start < count && (buffer[start] <= 32 || signedCharacters && buffer[start] >= 128)) start++;
            if (start == count || buffer[start] == '#') continue;
            var song = System.Text.Encoding.Latin1.GetString(buffer, start, count - start)
                .TrimEnd('\r', '\n').Replace('\\', '/');
            if (first)
            {
                first = false;
                if (song == "[playlist]") { pls = true; continue; }
            }
            if (pls)
            {
                if (!song.StartsWith("File", StringComparison.Ordinal)) continue;
                var index = 4;
                while (index < song.Length && song[index] >= '0' && song[index] <= '9') index++;
                if (index == song.Length || song[index] != '=') continue;
                song = song[(index + 1)..];
            }
            var slash = song.IndexOf('/');
            var relative = slash != 0 && !((windowsPaths ?? OperatingSystem.IsWindows()) && slash == 2 && song[1] == ':')
                && song.IndexOf("://", StringComparison.Ordinal) != slash - 1;
            if (decodeUtf8) song = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.Latin1.GetBytes(song));
            if (relative) song = directory + song;
            if (song.Length != 0) songs.Add(song);
        }
        return Count != 0;
    }

    public int Advance() => Position = Position + 1 < Count ? Position + 1 : 0;
    public int Backup() => Position = Count == 0 ? 0 : Position == 0 ? Count - 1 : Position - 1;
    public string? GetSong(int position) => position >= 0 && position < Count ? songs[position] : null;
    public void Shuffle(Func<int> nextRandom)
    {
        ArgumentNullException.ThrowIfNull(nextRandom);
        var count = Count;
        for (var i = 0; i < count; i++)
        {
            var random = nextRandom();
            if (random < 0) throw new InvalidOperationException("Playlist random values must be nonnegative.");
            var selected = i + random % (count - i);
            (songs[i], songs[selected]) = (songs[selected], songs[i]);
        }
        Position = 0;
    }

    public void Clear()
    {
        songs.Clear();
        Position = 0;
    }

    public bool Activate(bool goBack, Action<bool> stopMusic, VmPlaylistChangeMusic changeMusic, Action<string> report)
    {
        ArgumentNullException.ThrowIfNull(stopMusic);
        ArgumentNullException.ThrowIfNull(changeMusic);
        ArgumentNullException.ThrowIfNull(report);
        if (Count == 0) throw new InvalidOperationException("Cannot activate an empty playlist.");
        var start = Position;
        stopMusic(true);
        do
        {
            if (changeMusic(songs[Position], 0, false, true)) return true;
            if (goBack) Backup(); else Advance();
        } while (Position != start);
        Clear();
        report("Cannot play anything in the playlist.");
        return false;
    }
}
