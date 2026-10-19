namespace HCDE.Client;

public sealed class VmMusicPatchPaths
{
    private readonly List<string> paths = [];
    private readonly bool allowAbsolutePaths;
    private readonly StringComparison comparison;

    public VmMusicPatchPaths(bool allowAbsolutePaths = false, bool caseSensitivePaths = false)
    {
        this.allowAbsolutePaths = allowAbsolutePaths;
        comparison = caseSensitivePaths ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }

    public IReadOnlyList<string> Paths => paths.AsReadOnly();

    public void Add(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Length == 0 || (!allowAbsolutePaths && IsAbsolute(path))) return;
        var normalized = path.Replace('\\', '/');
        if (!normalized.EndsWith('/')) normalized += '/';
        var existing = paths.FindIndex(value => value.Equals(normalized, comparison));
        if (existing >= 0) paths.RemoveAt(existing);
        paths.Add(normalized);
    }

    public (byte[]? Data, string Name) Lookup(string name, Func<string, byte[]?> open)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(open);
        if (!IsAbsolute(name))
        {
            for (var index = paths.Count - 1; index >= 0; index--)
            {
                var fullName = paths[index] + name;
                var data = open(fullName);
                if (data is not null) return (data, fullName);
            }
        }
        var direct = open(name);
        return (direct, direct is null ? "" : name);
    }

    internal static bool IsAbsolute(string path) => path.Length > 0
        && (path[0] is '/' or '\\'
            || path.Length > 2 && path[1] == ':' && path[2] is '/' or '\\'
                && path[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
}
