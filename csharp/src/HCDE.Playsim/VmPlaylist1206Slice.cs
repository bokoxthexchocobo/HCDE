namespace HCDE.Playsim;

/// <summary>
/// M3U and PLS playlist load, shuffle, and cursor from <c>s_playlist.cpp</c> (<b>1–198</b>).
/// Chunk <b>1206</b>. The file text stands in for <c>FileReader</c>. A null text is a failed open.
/// This host is not Windows, so a drive letter is not an absolute path.
/// </summary>
public sealed class VmPlaylist1206Slice
{
    public readonly List<string> Songs = new();
    public uint Position;
    public static Func<int> NextRand = DefaultRand;

    public sealed class ScriptError : Exception
    {
        public ScriptError(string message) : base(message) { Detail = message; }
        public string Detail;
    }

    public static void ResetRand()
    {
        NextRand = DefaultRand;
    }

    static int DefaultRand()
    {
        return Random.Shared.Next();
    }

    public bool ChangeList(string path, string? body)
    {
        if (body == null)
            return false;
        Songs.Clear();
        Position = 0;
        string dir = ExtractFilePath(path);
        int cursor = 0;
        bool first = true;
        bool pls = false;
        while (true)
        {
            string song = NextLine(body, ref cursor);
            if (song.Length == 0)
                break;
            if (first)
            {
                first = false;
                if (song == "[playlist]")
                {
                    pls = true;
                    continue;
                }
            }

            if (pls)
            {
                if (!song.StartsWith("File", StringComparison.Ordinal))
                    continue;
                int i = 4;
                while (true)
                {
                    if (i >= song.Length)
                        throw Bounds(i, song.Length);
                    char c = song[i];
                    if (c < '0' || c > '9')
                        break;
                    i++;
                }

                if (song[i] != '=')
                    continue;
                song = song[(i + 1)..];
            }

            int slash = song.IndexOf('/');
            if (slash != 0 && song.IndexOf("://", StringComparison.Ordinal) != slash - 1)
                song = dir + song;
            if (song.Length != 0)
                Songs.Add(song);
        }

        return Songs.Count != 0;
    }

    public void Shuffle()
    {
        for (int i = 0; i < Songs.Count; i++)
        {
            int j = (NextRand() % (Songs.Count - i)) + i;
            (Songs[i], Songs[j]) = (Songs[j], Songs[i]);
        }

        Position = 0;
    }

    public int GetNumSongs() => Songs.Count;

    public int SetPosition(int position)
    {
        if ((uint)position >= (uint)Songs.Count)
            Position = 0;
        else
            Position = (uint)position;
        return (int)Position;
    }

    public int GetPosition() => (int)Position;

    public int Advance()
    {
        if (++Position >= (uint)Songs.Count)
            Position = 0;
        return (int)Position;
    }

    public int Backup()
    {
        uint current = Position;
        Position = current - 1;
        if (current == 0)
            Position = (uint)Songs.Count - 1;
        return unchecked((int)Position);
    }

    public string? GetSong(int position)
    {
        if ((uint)position >= (uint)Songs.Count)
            return null;
        return Songs[position];
    }

    static string NextLine(string body, ref int cursor)
    {
        while (true)
        {
            if (cursor >= body.Length)
                return "";
            int start = cursor;
            int limit = Math.Min(body.Length, cursor + 511);
            int end = start;
            while (end < limit && body[end] != '\n')
                end++;
            if (end < limit && body[end] == '\n')
                end++;
            cursor = end;
            string chunk = body[start..end];
            int skip = 0;
            while (skip < chunk.Length && chunk[skip] != '\0' && unchecked((sbyte)chunk[skip]) <= ' ')
                skip++;
            if (skip >= chunk.Length || chunk[skip] == '#')
                continue;
            return FixSlashes(StripRightCrLf(chunk[skip..]));
        }
    }

    static string ExtractFilePath(string path)
    {
        if (path.Length == 0)
            return "";
        int src = path.Length - 1;
        while (src != 0 && path[src - 1] != '/')
            src--;
        return path[..src];
    }

    static string StripRightCrLf(string text)
    {
        if (text.Length == 0)
            return text;
        int max = text.Length - 1;
        int i = max;
        for (; i > 0; i--)
        {
            if (text[i] != '\r' && text[i] != '\n')
                break;
        }

        if (i == max)
            return text;
        return text[..(i + 1)];
    }

    static string FixSlashes(string text) => text.Replace('\\', '/');

    static ScriptError Bounds(int index, int length)
    {
        return new ScriptError($"VM execution aborted: array access out of bounds. string index {index} out of bounds (string length is {length})");
    }
}
