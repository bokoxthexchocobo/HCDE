namespace HCDE.Scripting;

public sealed record VmScriptSource(string Name, string Text, int ContainerIndex);
public sealed record VmScriptSourceSet(Version Version, IReadOnlyList<VmScriptSource> Sources);
public readonly record struct VmScriptIncludeError(string Path, string SourceName, int SourceLine)
{
    public override string ToString() => $"{SourceName}, line {SourceLine}: Include script lump {Path} not found";
}

public sealed class VmScriptIncludeException : InvalidOperationException
{
    public IReadOnlyList<VmScriptIncludeError> Errors { get; }

    internal VmScriptIncludeException(string rootName, IReadOnlyList<VmScriptIncludeError> errors)
        : base($"{errors.Count} errors while parsing {rootName}\n" + string.Join("\n", errors))
        => Errors = Array.AsReadOnly(errors.ToArray());
}

/// <summary>Loads discovered include sources through the caller's archive lookup.</summary>
public static class VmScriptSourceLoader
{
    public static VmScriptSourceSet Load(VmScriptSource root, Func<string, VmScriptSource?> resolve,
        Version? maximumVersion = null)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(resolve);
        Validate(root);
        var version = VmScriptVersion.Read(root.Text, maximumVersion);
        var sources = new List<VmScriptSource> { root };
        var errors = new List<VmScriptIncludeError>();
        var queue = new VmScriptIncludeQueue();
        queue.AddSource(root.Text, root.Name);
        while (queue.TryTake(out var include))
        {
            var source = resolve(include.Path);
            if (source is null)
            {
                errors.Add(new VmScriptIncludeError(include.Path, include.SourceName, include.SourceLine));
                continue;
            }
            Validate(source);
            if (root.ContainerIndex == 0 && source.ContainerIndex != 0)
                throw new InvalidOperationException($"File {source.Name} is overriding core lump {include.Path}.");
            // Native ParseSingleFile shares the root parse version for all includes.
            queue.AddSource(source.Text, source.Name);
            sources.Add(source);
        }
        if (errors.Count != 0) throw new VmScriptIncludeException(root.Name, errors);
        return new VmScriptSourceSet(version, sources.AsReadOnly());
    }

    private static void Validate(VmScriptSource source)
    {
        ArgumentNullException.ThrowIfNull(source.Name);
        ArgumentNullException.ThrowIfNull(source.Text);
        ArgumentOutOfRangeException.ThrowIfNegative(source.ContainerIndex);
    }
}
