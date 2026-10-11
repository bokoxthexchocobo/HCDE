namespace HCDE.Client;

public sealed record VmSearchPathConfiguration(IReadOnlyList<VmFileSearchPath> IwadPaths,
    IReadOnlyList<VmFileSearchPath> FilePaths)
{
    public static VmSearchPathConfiguration Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var iwads = new List<VmFileSearchPath>();
        var files = new List<VmFileSearchPath>();
        string? section = null;
        using var reader = new StringReader(text);
        while (reader.ReadLine() is { } raw)
        {
            var line = Trim(raw);
            if (line.Length < 2 || line.StartsWith('#') || line.StartsWith("//", StringComparison.Ordinal)) continue;
            if (line[0] == '[')
            {
                section = line.EndsWith(']') ? line[1..^1] : line[1..];
                continue;
            }
            if (section is null) throw new InvalidDataException("Configuration entry precedes its section");
            var equal = line.IndexOf('=');
            if (equal <= 0) continue;
            var key = Trim(line[..equal]);
            var value = Trim(line[(equal + 1)..]);
            if (value.StartsWith("<<<", StringComparison.Ordinal) && value.Length > 3)
                throw new NotSupportedException("Multiline configuration values are not supported by search-path configuration");
            var isIwad = section.Equals("IWADSearch.Directories", StringComparison.OrdinalIgnoreCase);
            var isFile = section.Equals("FileSearch.Directories", StringComparison.OrdinalIgnoreCase);
            if (!isIwad && !isFile) continue;
            VmFileSearchPathKind? kind = key.ToLowerInvariant() switch
            {
                "path" => VmFileSearchPathKind.Path,
                "recursivepath" => VmFileSearchPathKind.RecursivePath,
                "pathlist" when isFile => VmFileSearchPathKind.PathList,
                _ => null,
            };
            if (kind is { } recognized) (isIwad ? iwads : files).Add(new(recognized, value));
        }
        return new(iwads.AsReadOnly(), files.AsReadOnly());
    }

    private static string Trim(string text)
    {
        var start = 0;
        var end = text.Length;
        while (start < end && text[start] <= ' ') start++;
        while (end > start && text[end - 1] <= ' ') end--;
        return text[start..end];
    }
}
