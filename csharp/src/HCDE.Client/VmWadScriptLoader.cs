using System.Text;
using HCDE.MapLoader;
using HCDE.Scripting;

namespace HCDE.Client;

public sealed record VmScriptWad(string Name, byte[] Data);

/// <summary>Loads a script source set from ordered WAD lump resources.</summary>
public static class VmWadScriptLoader
{
    public static VmScriptSourceSet Load(IReadOnlyList<VmScriptWad> archives, string rootLump = "ZSCRIPT", Version? maximumVersion = null)
    {
        ArgumentNullException.ThrowIfNull(rootLump);
        var catalog = new Catalog(archives);
        var root = catalog.Resolve(rootLump) ?? throw new InvalidOperationException($"Script root lump {rootLump} not found.");
        return VmScriptSourceLoader.Load(root, catalog.Resolve, maximumVersion);
    }

    public static IReadOnlyList<VmScriptSourceSet> LoadAll(IReadOnlyList<VmScriptWad> archives, Version? maximumVersion = null)
    {
        ArgumentNullException.ThrowIfNull(archives);
        return VmArchiveScriptLoader.LoadAll(archives.Select(archive =>
        {
            ArgumentNullException.ThrowIfNull(archive);
            return new VmScriptArchive(archive.Name, archive.Data, VmScriptArchiveFormat.Wad);
        }).ToArray(), maximumVersion);
    }

    private sealed record Resource(VmScriptWad Archive, WadLumpEntry Entry, int Container);

    private sealed class Catalog
    {
        private readonly Dictionary<string, Resource> _resources = new(StringComparer.OrdinalIgnoreCase);
        public List<Resource> Roots { get; } = new();

        public Catalog(IReadOnlyList<VmScriptWad> archives)
        {
            ArgumentNullException.ThrowIfNull(archives);
            for (var container = 0; container < archives.Count; container++)
            {
                var archive = archives[container];
                ArgumentNullException.ThrowIfNull(archive);
                ArgumentNullException.ThrowIfNull(archive.Name);
                ArgumentNullException.ThrowIfNull(archive.Data);
                if (!WadArchiveReader.TryReadDirectory(archive.Data, out var entries, out var error))
                    throw new InvalidDataException($"{archive.Name}: {error}");
                var global = VmWadScriptVisibility.GetGlobalEntries(entries);
                for (var index = 0; index < entries.Length; index++)
                {
                    if (!global[index]) continue;
                    var entry = entries[index];
                    var resource = new Resource(archive, entry, container);
                    _resources[entry.Name] = resource;
                    if (entry.Name.Equals("ZSCRIPT", StringComparison.OrdinalIgnoreCase)) Roots.Add(resource);
                }
            }
        }

        public VmScriptSource? Resolve(string path)
        {
            // Native full-name lookup strips exactly one leading slash.
            if (path.StartsWith('/')) path = path[1..];
            if (path.Length > 8 || path.Contains('/') || path.Contains('.')) return null;
            return _resources.TryGetValue(path, out var resource) ? Read(resource) : null;
        }

        public VmScriptSource Read(Resource resource)
        {
            if (!WadArchiveReader.TryReadLumpData(resource.Archive.Data, resource.Entry, out var data, out var error))
                throw new InvalidDataException($"{resource.Archive.Name}:{resource.Entry.Name}: {error}");
            var text = new UTF8Encoding(false, true).GetString(data);
            return new VmScriptSource(resource.Archive.Name + ":" + resource.Entry.Name, text, resource.Container);
        }
    }
}
