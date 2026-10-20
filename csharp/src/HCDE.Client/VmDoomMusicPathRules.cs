namespace HCDE.Client;

/// <summary>Native <c>s_doomsound.cpp</c> music path/extension helpers used by FindMusic.</summary>
public static class VmDoomMusicPathRules
{
    private static readonly string[] KnownExtensions =
    {
        "ogg", "mid", "midi", "mus", "mp3", "flac", "wav", "it", "xm", "s3m", "mod"
    };

    public static bool IsKnownMusicExtension(string? extension)
    {
        if (string.IsNullOrEmpty(extension))
            return false;
        foreach (var known in KnownExtensions)
        {
            if (string.Equals(known, extension, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool HasMusicPathSegment(string path)
    {
        var segment = path;
        while (!string.IsNullOrEmpty(segment))
        {
            var slash = segment.IndexOfAny(['/', '\\']);
            var segmentLen = slash >= 0 ? slash : segment.Length;
            if (segmentLen == 5 && segment.AsSpan(0, segmentLen).Equals("music".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
            segment = slash >= 0 ? segment[(slash + 1)..] : "";
        }

        return false;
    }

    public static bool MusicPathMatchesName(string fullName, string musicName)
    {
        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(musicName))
            return false;
        var slash = fullName.LastIndexOfAny(['/', '\\']);
        var baseName = slash >= 0 ? fullName[(slash + 1)..] : fullName;
        var dot = baseName.LastIndexOf('.');
        if (dot <= 0 || dot == baseName.Length - 1)
            return false;
        if (!IsKnownMusicExtension(baseName[(dot + 1)..]))
            return false;
        var stem = baseName[..dot];
        return stem.Length == musicName.Length
            && string.Equals(stem, musicName, StringComparison.OrdinalIgnoreCase);
    }
}
