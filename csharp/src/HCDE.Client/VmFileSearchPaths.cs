namespace HCDE.Client;

public enum VmFileSearchPathKind { Path, RecursivePath, PathList }

public sealed record VmFileSearchPath(VmFileSearchPathKind Kind, string Value);

public static class VmFileSearchPaths
{
    public static IReadOnlyList<VmFileSearchPath> Expand(IReadOnlyList<VmFileSearchPath> entries,
        Func<string, string?>? environment = null, string? programDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var result = new List<VmFileSearchPath>();
        foreach (var entry in entries)
        {
            ArgumentNullException.ThrowIfNull(entry);
            ArgumentNullException.ThrowIfNull(entry.Value);
            if (!Enum.IsDefined(entry.Kind)) throw new ArgumentOutOfRangeException(nameof(entries));
            if (entry.Kind == VmFileSearchPathKind.PathList)
            {
                // Native PathList expands before splitting, then NicePath expands each element again.
                foreach (var path in VmSearchPathExpansion.ExpandEnvironment(entry.Value, environment, programDirectory)
                    .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
                    Add(VmFileSearchPathKind.Path, path);
            }
            else Add(entry.Kind, entry.Value);
        }
        return result.AsReadOnly();

        void Add(VmFileSearchPathKind kind, string value)
        {
            var expanded = value.Length == 0 ? "." : VmSearchPathExpansion.ExpandEnvironment(value, environment, programDirectory);
            if (expanded.Length != 0) result.Add(new(kind, expanded));
        }
    }
}
