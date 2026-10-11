using System.Text;

namespace HCDE.Client;

public static class VmSearchPathExpansion
{
    public static string ExpandEnvironment(string? path, Func<string, string?>? environment = null,
        string? programDirectory = null)
    {
        if (path is null) return "";
        environment ??= Environment.GetEnvironmentVariable;
        programDirectory ??= AppContext.BaseDirectory;
        var output = new StringBuilder();
        for (var index = 0; index < path.Length; index++)
        {
            if (path[index] != '$') { output.Append(path[index]); continue; }
            var start = index + 1;
            var end = start;
            while (end < path.Length && IsNameCharacter(path[end])) end++;
            if (end == start) { output.Append('$'); continue; }
            var name = path[start..end];
            output.Append(name.Equals("progdir", StringComparison.OrdinalIgnoreCase)
                ? programDirectory : environment(name));
            index = end - 1;
        }
        return output.ToString();
    }

    public static IReadOnlyList<string> ExpandDirectories(IReadOnlyList<string> directories)
    {
        ArgumentNullException.ThrowIfNull(directories);
        var output = new List<string>();
        foreach (var directory in directories)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(directory);
            // Native Windows NicePath is environment substitution without tilde expansion.
            var expanded = ExpandEnvironment(directory);
            if (expanded.Length != 0) output.Add(expanded);
        }
        return output.AsReadOnly();
    }

    private static bool IsNameCharacter(char value) =>
        value is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';
}
