namespace HCDE.Scripting;

public readonly record struct VmScriptInclude(string Path, string SourceName, int SourceLine);

/// <summary>Discovery-ordered, per-compilation include registration.</summary>
public sealed class VmScriptIncludeQueue
{
    private readonly List<VmScriptInclude> _includes = new();
    private readonly HashSet<string> _paths = new(StringComparer.Ordinal);
    private int _next;

    public int PendingCount => _includes.Count - _next;

    public void AddSource(string source, string sourceName)
    {
        foreach (var include in VmScriptIncludes.Extract(source, sourceName))
        {
            if (!_paths.Add(include.Path)) continue;
            _includes.Add(include);
        }
    }

    public bool Add(string path, string sourceName, int sourceLine)
    {
        var resolved = VmScriptIncludes.ResolvePath(path, sourceName);
        if (!_paths.Add(resolved)) return false;
        _includes.Add(new VmScriptInclude(resolved, sourceName, sourceLine));
        return true;
    }

    public bool TryTake(out VmScriptInclude include)
    {
        if (_next == _includes.Count)
        {
            include = default;
            return false;
        }
        include = _includes[_next++];
        return true;
    }

    public void Clear()
    {
        _includes.Clear();
        _paths.Clear();
        _next = 0;
    }
}
