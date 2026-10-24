using SharpCompress.Archives;
using SharpCompress.Common;
using System.Text;
using HCDE.Scripting;
using HCDE.MapLoader;

namespace HCDE.Client;

public enum VmScriptArchiveFormat { Wad, Pk3, RawLump, Grp, Pak, Rff, Hog, Mvl, Ssi, Directory, Whres, SevenZip, Auto }

public sealed record VmScriptArchive(string Name, byte[] Data, VmScriptArchiveFormat Format = VmScriptArchiveFormat.Auto,
    IReadOnlyList<string>? DeletedGlobalLumps = null);

/// <summary>Loads ZScript sources from ordered WAD, PK3, GRP, PAK, RFF, HOG, MVL, SSI, WHRES, 7z, directory, and raw lump archives.</summary>
public static class VmArchiveScriptLoader
{
    public static IReadOnlyList<VmScriptSourceSet> LoadPaths(IReadOnlyList<string> paths, Version? maximumVersion = null,
        IReadOnlyList<string>? gameTypeFilters = null, IReadOnlyList<string>? blockedPatterns = null,
        IReadOnlyList<string>? embeddings = null, Func<string, string, bool>? directoryFilenameCheck = null,
        Action<string>? directoryWarning = null, Action<string>? archiveWarning = null, bool reserveCoreContainer = false,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? deletedGlobalLumps = null)
    {
        ArgumentNullException.ThrowIfNull(paths);
        var archives = paths.Select(path =>
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            var fullPath = Path.GetFullPath(path);
            var deleted = deletedGlobalLumps is not null && deletedGlobalLumps.TryGetValue(fullPath, out var names) ? names : null;
            return Directory.Exists(fullPath)
                ? new VmScriptArchive(fullPath, [], VmScriptArchiveFormat.Directory, deleted)
                : new VmScriptArchive(fullPath, File.ReadAllBytes(fullPath), DeletedGlobalLumps: deleted);
        }).ToArray();
        if (reserveCoreContainer)
            archives = [new VmScriptArchive("<reserved-engine-core>", new byte[] {80,87,65,68,0,0,0,0,12,0,0,0}, VmScriptArchiveFormat.Wad), .. archives];
        return LoadAll(archives, maximumVersion, gameTypeFilters, blockedPatterns, embeddings,
            directoryFilenameCheck, directoryWarning, archiveWarning);
    }

    public static IReadOnlyList<VmScriptSourceSet> LoadAll(IReadOnlyList<VmScriptArchive> archives, Version? maximumVersion = null,
        IReadOnlyList<string>? gameTypeFilters = null, IReadOnlyList<string>? blockedPatterns = null,
        IReadOnlyList<string>? embeddings = null, Func<string, string, bool>? directoryFilenameCheck = null, Action<string>? directoryWarning = null, Action<string>? archiveWarning = null)
        => LoadCore(archives, maximumVersion, gameTypeFilters, blockedPatterns, embeddings,
            directoryFilenameCheck, directoryWarning, archiveWarning, "ZSCRIPT", false).Sources;

    public static string? ReadGlobalText(VmScriptArchive archive, string shortName,
        IReadOnlyList<string>? gameTypeFilters = null)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentException.ThrowIfNullOrWhiteSpace(shortName);
        if (Encoding.UTF8.GetByteCount(shortName) > 8 || shortName.Contains('/') || shortName.Contains('.'))
            throw new ArgumentException("A global short name must fit in eight bytes", nameof(shortName));
        return LoadCore([archive], null, gameTypeFilters, null, null, null, null, null, shortName, true).Text;
    }

    private sealed record ArchiveLoadResult(IReadOnlyList<VmScriptSourceSet> Sources, string? Text);

    public static IReadOnlyList<VmIwadResourceName> ReadResourceNames(VmScriptArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);
        var names = new List<VmIwadResourceName>();
        LoadCore([archive], null, null, null, null, null, null, null, "", true, names.Add);
        return names.AsReadOnly();
    }

    private static ArchiveLoadResult LoadCore(IReadOnlyList<VmScriptArchive> archives, Version? maximumVersion,
        IReadOnlyList<string>? gameTypeFilters, IReadOnlyList<string>? blockedPatterns,
        IReadOnlyList<string>? embeddings, Func<string, string, bool>? directoryFilenameCheck,
        Action<string>? directoryWarning, Action<string>? archiveWarning, string rootName, bool readTextOnly,
        Action<VmIwadResourceName>? collectNames = null)
    {
        ArgumentNullException.ThrowIfNull(archives);
        var filters = gameTypeFilters?.ToArray();
        var embeddingNames = embeddings?.ToArray();
        if (embeddingNames is not null) foreach (var name in embeddingNames) ArgumentNullException.ThrowIfNull(name);
        if (filters is not null) foreach (var filter in filters) ArgumentNullException.ThrowIfNull(filter);
        var patterns = blockedPatterns?.ToArray();
        if (patterns is not null)
            foreach (var pattern in patterns)
            {
                ArgumentNullException.ThrowIfNull(pattern);
                VmArchivePattern.Matches("", pattern);
            }
        var opened = new List<IArchive>();
        var sevenZipReaders = new List<VmSevenZipArchiveReader>();
        var streams = new List<MemoryStream>();
        var pendingArchives = archives.ToList();
        try
        {
            var resources = new Dictionary<string, Resource>(StringComparer.Ordinal);
            var shortNames = new Dictionary<string, Resource>(StringComparer.Ordinal);
            var roots = new List<Resource>();
            for (var container = 0; container < pendingArchives.Count; container++)
            {
                var archive = pendingArchives[container];
                ArgumentNullException.ThrowIfNull(archive);
                ArgumentNullException.ThrowIfNull(archive.Name);
                ArgumentNullException.ThrowIfNull(archive.Data);
                if (archive.Format == VmScriptArchiveFormat.Auto)
                    archive = archive with { Format = DetectFormat(archive.Data) };
                var deletedNames = archive.DeletedGlobalLumps?.ToArray();
                if (deletedNames is not null)
                {
                    foreach (var name in deletedNames) ArgumentNullException.ThrowIfNull(name);
                }
                if (archive.Format == VmScriptArchiveFormat.Ssi)
                {
                    var children = new List<VmScriptArchive>();
                    foreach (var entry in VmSsiArchiveReader.Read(archive.Data))
                    {
                        var name = NormalizeName(entry.Name);
                        if (entry.Embedded)
                        {
                            children.Add(new(archive.Name + ":" + name, entry.Data, DetectFormat(entry.Data)));
                            continue;
                        }
                        if (HasNativeNamespace(name)) name = StripEncodedId(name);
                        var resource = new Resource(archive.Name, name, null, container, RawData: entry.Data);
                        resources[FullNameKey(name)] = resource;
                        CollectResourceName(name, collectNames);
                        if (name.Contains('/')) continue;
                        var dot = name.LastIndexOf('.');
                        var key = ShortNameKey(dot < 0 ? name : name[..dot]);
                        if (IsDeletedShortName(key, deletedNames)) continue;
                        shortNames[key] = resource;
                        if (key == ShortNameKey(rootName)) roots.Add(resource);
                    }
                    pendingArchives.InsertRange(container + 1, children);
                    continue;
                }
                if (archive.Format is VmScriptArchiveFormat.Pak or VmScriptArchiveFormat.Directory or VmScriptArchiveFormat.SevenZip)
                {
                    IEnumerable<(string Name, ArchivePayload Payload)> archiveEntries;
                    if (archive.Format == VmScriptArchiveFormat.SevenZip)
                    {
                        var reader = new VmSevenZipArchiveReader(archive.Data);
                        sevenZipReaders.Add(reader);
                        archiveEntries = reader.GetEntries().Select(entry => (entry.Name, new ArchivePayload(null, null, entry.Read)));
                    }
                    else if (archive.Format == VmScriptArchiveFormat.Directory)
                        archiveEntries = VmDirectoryArchiveReader.Read(archive.Name, directoryFilenameCheck, directoryWarning).Select(entry => (entry.Name, new ArchivePayload(null, entry.FilePath)));
                    else
                        archiveEntries = VmPakArchiveReader.Read(archive.Data).Select(entry => (entry.Name, new ArchivePayload(entry.Data, null)));
                    if (archive.Format == VmScriptArchiveFormat.Directory)
                        archive = archive with { Name = Path.GetFullPath(archive.Name).Replace('\\', '/').TrimEnd('/') + "/" };
                    var pakItems = archiveEntries.Select(entry => (Entry: entry.Payload, Name: NormalizeName(entry.Name)))
                        .OrderBy(item => item.Name, Utf8NameComparer.Instance).ToList();
                    ApplyCompatibilityExclusions(pakItems, archive.Name);
                    if (patterns is not null)
                        for (var index = 0; index < pakItems.Count; index++)
                            if (patterns.Any(pattern => VmArchivePattern.Matches(pakItems[index].Name, pattern))) pakItems[index] = (pakItems[index].Entry, "");
                    if (filters is not null) { BlockUnwantedEntries(pakItems); StripCommonFolder(pakItems); ApplyFilters(pakItems, filters); }
                    var children = new List<VmScriptArchive>();
                    foreach (var item in pakItems)
                    {
                        var name = item.Name;
                        if (name.Length == 0 || name.EndsWith('/')) continue;
                        if (IsEmbeddedWad(name, archive.Name) || embeddingNames is not null && embeddingNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                        {
                            var data = item.Entry.ReadBytes();
                            children.Add(new(archive.Name + ":" + name, data, DetectFormat(data)));
                            continue;
                        }
                        if (HasNativeNamespace(name)) name = StripEncodedId(name);
                        var resource = new Resource(archive.Name, name, null, container, RawData: item.Entry.Data, FilePath: item.Entry.FilePath, ReadBytes: item.Entry.Read);
                        resources[FullNameKey(name)] = resource;
                        CollectResourceName(name, collectNames);
                        if (name.Contains('/')) continue;
                        var dot = name.LastIndexOf('.');
                        var key = ShortNameKey(dot < 0 ? name : name[..dot]);
                        if (IsDeletedShortName(key, deletedNames)) continue;
                        shortNames[key] = resource;
                        if (key == ShortNameKey(rootName)) roots.Add(resource);
                    }
                    pendingArchives.InsertRange(container + 1, children);
                    continue;
                }
                if (archive.Format is VmScriptArchiveFormat.Grp or VmScriptArchiveFormat.Rff or VmScriptArchiveFormat.Hog or VmScriptArchiveFormat.Mvl or VmScriptArchiveFormat.Whres)
                {
                    var entries = archive.Format switch
                    {
                        VmScriptArchiveFormat.Grp => VmGrpArchiveReader.Read(archive.Data),
                        VmScriptArchiveFormat.Rff => VmRffArchiveReader.Read(archive.Data),
                        VmScriptArchiveFormat.Hog => VmHogArchiveReader.Read(archive.Data),
                        VmScriptArchiveFormat.Mvl => VmMvlArchiveReader.Read(archive.Data),
                        _ => VmWhresArchiveReader.Read(archive.Name, archive.Data)
                    };
                    foreach (var entry in entries)
                    {
                        var name = NormalizeName(entry.Name);
                        if (HasNativeNamespace(name)) name = StripEncodedId(name);
                        var resource = new Resource(archive.Name, name, null, container, RawData: entry.Data);
                        resources[FullNameKey(name)] = resource;
                        CollectResourceName(name, collectNames);
                        if (name.Contains('/')) continue;
                        var dot = name.LastIndexOf('.');
                        var key = ShortNameKey(dot < 0 ? name : name[..dot]);
                        if (IsDeletedShortName(key, deletedNames)) continue;
                        shortNames[key] = resource;
                        if (key == ShortNameKey(rootName)) roots.Add(resource);
                    }
                    continue;
                }
                if (archive.Format == VmScriptArchiveFormat.RawLump)
                {
                    var name = NormalizeName(archive.Name[(archive.Name.LastIndexOfAny(['/', '\\']) + 1)..]);
                    if (HasNativeNamespace(name)) name = StripEncodedId(name);
                    var resource = new Resource(archive.Name, name, null, container, RawData: archive.Data);
                    resources[FullNameKey(name)] = resource;
                    CollectResourceName(name, collectNames);
                    var dot = name.LastIndexOf('.');
                    var key = ShortNameKey(dot < 0 ? name : name[..dot]);
                    if (IsDeletedShortName(key, deletedNames)) continue;
                    shortNames[key] = resource;
                    if (key == ShortNameKey(rootName)) roots.Add(resource);
                    continue;
                }
                if (archive.Format == VmScriptArchiveFormat.Wad)
                {
                    if (!WadArchiveReader.TryReadDirectory(archive.Data, out var entries, out var error))
                        throw new InvalidDataException($"{archive.Name}: {error}");
                    var global = VmWadScriptVisibility.GetGlobalEntries(entries);
                    for (var index = 0; index < entries.Length; index++)
                    {
                        collectNames?.Invoke(new(NativeShortName(ShortNameKey(entries[index].Name))));
                        if (!global[index]) continue;
                        var entry = entries[index];
                        if (IsDeletedShortName(ShortNameKey(entry.Name), deletedNames)) continue;
                        var resource = new Resource(archive.Name, entry.Name, null, container, archive.Data, entry);
                        // Native WAD records have no full name; they participate only in short-name fallback.
                        shortNames[ShortNameKey(entry.Name)] = resource;
                        if (ShortNameKey(entry.Name) == ShortNameKey(rootName)) roots.Add(resource);
                    }
                    continue;
                }
                if (archive.Format != VmScriptArchiveFormat.Pk3)
                    throw new ArgumentOutOfRangeException(nameof(archives), "Unknown script archive format.");
                var input = new MemoryStream(archive.Data, writable: false);
                streams.Add(input);
                var zip = SharpCompress.Archives.Zip.ZipArchive.OpenArchive(input);
                opened.Add(zip);
                var items = zip.Entries.Where(entry => AcceptZipEntry(entry, archive.Name, archiveWarning)).Select(entry => (Entry: entry, Name: NormalizeName(entry.Key ?? "")))
                    .OrderBy(item => item.Name, Utf8NameComparer.Instance).ToList();
                ApplyCompatibilityExclusions(items, archive.Name);
                if (patterns is not null)
                    for (var index = 0; index < items.Count; index++)
                        if (patterns.Any(pattern => VmArchivePattern.Matches(items[index].Name, pattern)))
                            items[index] = (items[index].Entry, "");
                if (filters is not null)
                {
                    BlockUnwantedEntries(items);
                    StripCommonFolder(items);
                    ApplyFilters(items, filters);
                }
                var embedded = new List<VmScriptArchive>();
                foreach (var item in items)
                {
                    var entry = item.Entry;
                    var name = item.Name;
                    if (name.Length == 0 || name.EndsWith('/')) continue;
                    if (IsEmbeddedWad(name, archive.Name) || embeddingNames is not null && embeddingNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        var data = ReadZipEntry(entry);

                        var format = DetectFormat(data);
                        embedded.Add(new VmScriptArchive(archive.Name + ":" + name, data, format));
                        continue;
                    }
                    if (HasNativeNamespace(name)) name = StripEncodedId(name);
                    var resource = new Resource(archive.Name, name, entry, container);
                    resources[FullNameKey(name)] = resource;
                    CollectResourceName(name, collectNames);
                    if (name.Contains('/')) continue;
                    var dot = name.LastIndexOf('.');
                    var shortName = dot < 0 ? name : name[..dot];
                    var key = ShortNameKey(shortName);
                    if (IsDeletedShortName(key, deletedNames)) continue;
                    shortNames[key] = resource;
                    if (key == ShortNameKey(rootName)) roots.Add(resource);
                }
                pendingArchives.InsertRange(container + 1, embedded);
            }

            VmScriptSource? Resolve(string path)
            {
                if (path.StartsWith('/')) path = path[1..];
                if (resources.TryGetValue(FullNameKey(path), out var resource)) return Read(resource);
                if (Encoding.UTF8.GetByteCount(path) <= 8 && !path.Contains('/') && !path.Contains('.') && shortNames.TryGetValue(ShortNameKey(path), out resource))
                    return Read(resource);
                return null;
            }

            if (collectNames is not null) return new(Array.Empty<VmScriptSourceSet>(), null);
            if (readTextOnly) return new(Array.Empty<VmScriptSourceSet>(), roots.Count == 0 ? null : Read(roots[^1]).Text);
            var sets = new List<VmScriptSourceSet>();
            foreach (var root in roots) sets.Add(VmScriptSourceLoader.Load(Read(root), Resolve, maximumVersion));
            return new(sets.AsReadOnly(), null);
        }
        catch (ArchiveException error)
        {
            throw new InvalidDataException(error.Message, error);
        }
        finally
        {
            foreach (var reader in sevenZipReaders) reader.Dispose();
            foreach (var zip in opened) zip.Dispose();
            foreach (var stream in streams) stream.Dispose();
        }
    }

    private sealed record Resource(string ArchiveName, string Name, IArchiveEntry? Entry, int Container, byte[]? WadData = null,
        WadLumpEntry WadEntry = default, byte[]? RawData = null, string? FilePath = null, Func<byte[]>? ReadBytes = null);

    private sealed record ArchivePayload(byte[]? Data, string? FilePath, Func<byte[]>? Read = null)
    {
        public byte[] ReadBytes() => Data ?? (Read is not null ? Read() : File.ReadAllBytes(FilePath!));
    }

    private static VmScriptArchiveFormat DetectFormat(byte[] data)
    {
        if (data.Length >= 12 && (data.AsSpan().StartsWith("IWAD"u8) || data.AsSpan().StartsWith("PWAD"u8))) return VmScriptArchiveFormat.Wad;
        if (data.Length >= 30 && data.AsSpan().StartsWith(new byte[] {80,75,3,4})) return VmScriptArchiveFormat.Pk3;
        if (data.AsSpan().StartsWith(new byte[] {55,122,188,175,39,28})) return VmScriptArchiveFormat.SevenZip;
        if (data.Length >= 12 && data.AsSpan().StartsWith("PACK"u8)) return VmScriptArchiveFormat.Pak;
        if (data.Length >= 12 && data.AsSpan().StartsWith("KenSilverman"u8)) return VmScriptArchiveFormat.Grp;
        if (data.Length >= 16 && data.AsSpan().StartsWith("RFF\u001a"u8)) return VmScriptArchiveFormat.Rff;
        if (VmSsiArchiveReader.HasHeader(data)) return VmScriptArchiveFormat.Ssi;
        if (data.Length >= 20 && data.AsSpan().StartsWith("DHF"u8)) return VmScriptArchiveFormat.Hog;
        if (data.Length >= 20 && data.AsSpan().StartsWith("DMVL"u8)) return VmScriptArchiveFormat.Mvl;
        if (VmWhresArchiveReader.HasDirectory(data)) return VmScriptArchiveFormat.Whres;
        return VmScriptArchiveFormat.RawLump;
    }
    private static bool IsEmbeddedWad(string name, string archiveName)
    {
        if (name.Length < 4 || name.IndexOf(".wad", StringComparison.Ordinal) != name.Length - 4) return false;
        var slash = name.LastIndexOf('/');
        if (slash < 0) return true;
        // Native ExtractBaseName treats slash/backslash as separators, and removes the last extension.
        var archiveLeaf = archiveName[(archiveName.LastIndexOfAny(['/', '\\']) + 1)..];
        var extension = archiveLeaf.LastIndexOf('.');
        var project = extension < 0 ? archiveLeaf : archiveLeaf[..extension];
        return name.Equals(project + "/" + name[(slash + 1)..], StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyCompatibilityExclusions<T>(List<(T Entry, string Name)> items, string archiveName)
    {
        var separator = archiveName.LastIndexOfAny(['/', '\\', ':']);
        if (!archiveName[(separator + 1)..].Equals("zandronum.pk3", StringComparison.OrdinalIgnoreCase)) return;
        for (var index = 0; index < items.Count; index++)
        {
            var name = items[index].Name;
            if (name.StartsWith("mapinfo/", StringComparison.Ordinal) || name.StartsWith("zscript/", StringComparison.Ordinal)
                || CompatibilityBlockedNames.Contains(name, StringComparer.Ordinal))
                items[index] = (items[index].Entry, "");
        }
    }

    private static readonly string[] CompatibilityBlockedNames =
        ["decorate", "decorate.txt", "decorate.z", "fontdefs", "fontdefs.txt", "gameinfo", "gameinfo.txt", "iwadinfo", "iwadinfo.txt",
            "keyconf", "keyconf.txt", "menudef", "menudef.txt", "menudef.z", "menudef.za", "zscript", "zscript.txt"];

    private static void BlockUnwantedEntries<T>(List<(T Entry, string Name)> items)
    {
        for (var index = 0; index < items.Count; index++)
        {
            var name = items[index].Name;
            // Native GetReserved patterns: *.bat, *.exe, __macosx/*, */__macosx/*, *~.
            if (name.EndsWith(".bat", StringComparison.Ordinal) || name.EndsWith(".exe", StringComparison.Ordinal)
                || name.EndsWith('~') || name.StartsWith("__macosx/", StringComparison.Ordinal)
                || name.Contains("/__macosx/", StringComparison.Ordinal))
                items[index] = (items[index].Entry, "");
        }
    }

    private static void StripCommonFolder<T>(List<(T Entry, string Name)> items)
    {
        var prefix = "";
        var top = "";
        var foundPrefix = false;
        var foundDefinition = false;
        foreach (var item in items)
        {
            var name = item.Name;
            if (name.Length == 0) continue;
            if (name.StartsWith("filter/", StringComparison.Ordinal)) return;
            if (!foundPrefix)
            {
                if (!name.Contains('/') || ReservedFolders.Contains(name, StringComparer.Ordinal)) break;
                prefix = name[..(name.LastIndexOf('/') + 1)];
                top = name[..(name.IndexOf('/') + 1)];
                foundPrefix = true;
            }
            if (!name.StartsWith(prefix, StringComparison.Ordinal))
            {
                prefix = name.StartsWith(top, StringComparison.Ordinal) ? top : "";
                if (prefix.Length == 0) break;
            }
            if (!foundDefinition)
                foundDefinition = RequiredPrefixes.Any(required => name.StartsWith(prefix + required, StringComparison.Ordinal)
                    || name.EndsWith(required, StringComparison.Ordinal));
        }
        if (!foundDefinition || prefix.Length == 0) return;
        for (var index = 0; index < items.Count; index++)
            if (items[index].Name.Length != 0) items[index] = (items[index].Entry, items[index].Name[prefix.Length..]);
    }

    private static readonly string[] ReservedFolders =
        ["flats/", "textures/", "hires/", "sprites/", "voxels/", "colormaps/", "acs/", "maps/", "voices/", "patches/", "graphics/", "sounds/", "music/", "materials/", "models/", "fonts/", "brightmaps/"];
    private static readonly string[] RequiredPrefixes =
        ["mapinfo", "zmapinfo", "umapinfo", "emapinfo", "gameinfo", "sndinfo", "sndseq", "sbarinfo", "menudef", "gldefs", "animdefs", "decorate", "zscript", "iwadinfo", "complvl", "terrain", "maps/"];

    private static void ApplyFilters<T>(List<(T Entry, string Name)> items, IReadOnlyList<string> filters)
    {
        var active = items.Count;
        foreach (var filter in filters)
        {
            for (var index = 0; index <= filter.Length; index++)
            {
                if (index != filter.Length && filter[index] != '.') continue;
                if (index == 0) continue;
                var prefix = "filter/" + filter[..index] + "/";
                var selected = items.Take(active).Where(item => item.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (selected.Length == 0) continue;
                for (var position = active - 1; position >= 0; position--)
                    if (items[position].Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) items.RemoveAt(position);
                items.AddRange(selected.Select(item => (item.Entry, item.Name[prefix.Length..])));
                active -= selected.Length;
            }
        }
        for (var index = 0; index < active; index++)
            if (items[index].Name.StartsWith("filter/", StringComparison.Ordinal)) items[index] = (items[index].Entry, "");
    }

    private static string NormalizeName(string name) => name.ToLowerInvariant().Normalize(NormalizationForm.FormC).Replace('\\', '/');

    private static string FullNameKey(string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        for (var index = 0; index < bytes.Length; index++)
            if (bytes[index] is >= (byte)'A' and <= (byte)'Z') bytes[index] += 32;
        return Convert.ToHexString(bytes);
    }

    private static bool HasNativeNamespace(string name)
    {
        if (!name.Contains('/')) return true;
        foreach (var prefix in NativeNamespacePrefixes)
            if (name.StartsWith(prefix, StringComparison.Ordinal)) return true;
        return false;
    }

    private static readonly string[] NativeNamespacePrefixes =
        ["flats/", "textures/", "hires/", "sprites/", "voxels/", "colormaps/", "acs/", "voices/", "patches/", "graphics/", "sounds/", "music/"];

    private static string StripEncodedId(string name)
    {
        if (!name.Contains(".{", StringComparison.Ordinal)) return name;
        // Preserve native find_last_of(".{") semantics, including its unusual start offset.
        var start = Math.Max(name.LastIndexOf('.'), name.LastIndexOf('{'));
        var position = start + 2;
        if (position > name.Length) return name;
        var numberStart = position;
        while (position < name.Length && name[position] is ' ' or '\t' or '\r' or '\n' or '\v' or '\f') position++;
        if (position < name.Length && name[position] is '+' or '-') position++;
        var digits = position;
        while (position < name.Length && name[position] is >= '0' and <= '9') position++;
        if (position == digits) position = numberStart;
        if (position < name.Length && name[position] == '}' && (position + 1 == name.Length || name[position + 1] == '.'))
            return name.Remove(start, position - start + 1);
        return name;
    }

    private static bool IsDeletedShortName(string key, IReadOnlyList<string>? deletedNames)
    {
        if (deletedNames is null || deletedNames.Count == 0) return false;
        return deletedNames.Contains(NativeShortName(key), StringComparer.Ordinal);
    }

    private static string NativeShortName(string key)
    {
        var bytes = Convert.FromHexString(key);
        var end = Array.IndexOf(bytes, (byte)0);
        return Encoding.UTF8.GetString(bytes, 0, end < 0 ? bytes.Length : end);
    }

    private static void CollectResourceName(string name, Action<VmIwadResourceName>? collect)
    {
        if (collect is null) return;
        var leaf = name[(name.LastIndexOf('/') + 1)..];
        var dot = leaf.LastIndexOf('.');
        var shortName = HasNativeNamespace(name) ? NativeShortName(ShortNameKey(dot < 0 ? leaf : leaf[..dot])) : "";
        collect(new(shortName, name));
    }

    private static string ShortNameKey(string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        var key = new byte[8];
        for (var index = 0; index < Math.Min(bytes.Length, key.Length) && bytes[index] != 0; index++)
            key[index] = bytes[index] is >= (byte)'a' and <= (byte)'z' ? (byte)(bytes[index] - 32) : bytes[index];
        return Convert.ToHexString(key);
    }

    private sealed class Utf8NameComparer : IComparer<string>
    {
        public static readonly Utf8NameComparer Instance = new();

        public int Compare(string? left, string? right)
        {
            if (left is null) return right is null ? 0 : -1;
            if (right is null) return 1;
            // Native entrycmp uses strcmp on normalized UTF-8, rather than UTF-16 ordinal order.
            return Encoding.UTF8.GetBytes(left).AsSpan().SequenceCompareTo(Encoding.UTF8.GetBytes(right));
        }
    }

    private static VmScriptSource Read(Resource resource)
    {
        if (resource.ReadBytes is not null)
            return new VmScriptSource(resource.ArchiveName + ":" + resource.Name,
                new UTF8Encoding(false, true).GetString(resource.ReadBytes()), resource.Container);
        if (resource.FilePath is not null)
            return new VmScriptSource(resource.ArchiveName + ":" + resource.Name,
                new UTF8Encoding(false, true).GetString(File.ReadAllBytes(resource.FilePath)), resource.Container);
        if (resource.RawData is not null)
            return new VmScriptSource(resource.ArchiveName + ":" + resource.Name,
                new UTF8Encoding(false, true).GetString(resource.RawData), resource.Container);
        if (resource.WadData is not null)
        {
            if (!WadArchiveReader.TryReadLumpData(resource.WadData, resource.WadEntry, out var data, out var error))
                throw new InvalidDataException($"{resource.ArchiveName}:{resource.Name}: {error}");
            return new VmScriptSource(resource.ArchiveName + ":" + resource.Name, new UTF8Encoding(false, true).GetString(data), resource.Container);
        }
        var text = new UTF8Encoding(false, true).GetString(ReadZipEntry(resource.Entry!));
        return new VmScriptSource(resource.ArchiveName + ":" + resource.Name, text, resource.Container);
    }

    private static bool AcceptZipEntry(IArchiveEntry entry, string archiveName, Action<string>? warning)
    {
        if (VmZipResourcePolicy.IsSupportedArchiveEntry(entry)) return true;
        var method = entry.CompressionType;
        var supported = VmZipResourcePolicy.IsSupportedCompressionType(method);
        var reason = supported ? "is encrypted. Encryption is not supported." : $"uses an unsupported compression algorithm ({method}).";
        var message = $"{archiveName}: '{entry.Key}' {reason}";
        if (warning is not null) warning(message);
        else Console.Error.WriteLine(message);
        return false;
    }

    private static byte[] ReadZipEntry(IArchiveEntry entry)
    {
        using var output = new MemoryStream();
        entry.WriteTo(output, new ExtractionOptions { CheckCrc = true });
        if (output.Length != entry.Size) throw new InvalidDataException("Truncated ZIP entry.");
        return output.ToArray();
    }
}
