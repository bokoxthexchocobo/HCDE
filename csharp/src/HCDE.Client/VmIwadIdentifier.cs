using HCDE.MapLoader;

namespace HCDE.Client;

public sealed class VmIwadDefinition
{
    public VmIwadDefinition(string name, VmStartupGameType game, string autoName, IReadOnlyList<string> mustContain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!Enum.IsDefined(game)) throw new ArgumentOutOfRangeException(nameof(game));
        ArgumentNullException.ThrowIfNull(autoName);
        ArgumentNullException.ThrowIfNull(mustContain);
        var required = mustContain.ToArray();
        foreach (var lump in required) ArgumentNullException.ThrowIfNull(lump);
        Name = name;
        Game = game;
        AutoName = autoName;
        MustContain = Array.AsReadOnly(required);
    }

    public string Name { get; }
    public VmStartupGameType Game { get; }
    public string AutoName { get; }
    public IReadOnlyList<string> MustContain { get; }
}

/// <summary>Resource names as supplied by the native filesystem to ScanIWAD.</summary>
public sealed record VmIwadResourceName(string ShortName, string? FullName = null);

public static class VmIwadIdentifier
{
    public static VmIwadDefinition? IdentifyArchive(IReadOnlyList<VmIwadDefinition> definitions, VmScriptArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);
        // Identification precedes IWAD-specific short-name deletion.
        return Identify(definitions, VmArchiveScriptLoader.ReadResourceNames(archive with { DeletedGlobalLumps = null }));
    }

    public static VmIwadDefinition? Identify(IReadOnlyList<VmIwadDefinition> definitions,
        IReadOnlyList<VmIwadResourceName> resources)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(resources);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var resource in resources)
        {
            ArgumentNullException.ThrowIfNull(resource);
            ArgumentNullException.ThrowIfNull(resource.ShortName);
            names.Add(resource.ShortName);
            if (resource.FullName is { } full && full.StartsWith("maps/", StringComparison.OrdinalIgnoreCase))
            {
                var map = full[5..];
                var dot = map.IndexOf('.');
                names.Add(dot < 0 ? map : map[..dot]);
            }
        }
        foreach (var definition in definitions)
        {
            ArgumentNullException.ThrowIfNull(definition);
            if (definition.MustContain.All(names.Contains)) return definition;
        }
        return null;
    }

    public static VmIwadDefinition? IdentifyWad(IReadOnlyList<VmIwadDefinition> definitions, ReadOnlySpan<byte> wad)
    {
        if (!WadArchiveReader.TryReadDirectory(wad, out var entries, out var error))
            throw new InvalidDataException(error);
        // ScanIWAD inspects all names, including entries outside the global namespace.
        return Identify(definitions, entries.Select(entry => new VmIwadResourceName(entry.Name)).ToArray());
    }
}
