namespace HCDE.Client;

public sealed class VmGusResourceMusicReader : IVmMusicBankReader
{
    private readonly string configurationName;
    private readonly string basePath;
    private readonly Func<string, byte[]?> openResource;
    private readonly Func<byte[]?> openConfiguration;
    public VmMusicPatchPaths Paths { get; } = new();

    public VmGusResourceMusicReader(string configurationName, Func<string, byte[]?> openResource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationName);
        ArgumentNullException.ThrowIfNull(openResource);
        this.configurationName = configurationName;
        this.openResource = openResource;
        openConfiguration = () => openResource(configurationName);
        var normalized = configurationName.Replace('\\', '/');
        var slash = normalized.LastIndexOf('/');
        basePath = slash < 0 ? "" : normalized[..(slash + 1)];
    }

    public VmGusResourceMusicReader(string configurationName, Func<string, int> findResource,
        Func<int, byte[]?> reopenResource)
        : this(configurationName, CreateLookup(findResource, reopenResource))
    {
        var configurationIndex = findResource(configurationName);
        openConfiguration = () => configurationIndex < 0 ? null : reopenResource(configurationIndex);
    }

    public VmGusResourceMusicReader(string configurationName, int configurationIndex,
        Func<string, int> findResource, Func<int, byte[]?> reopenResource)
        : this(configurationName, CreateLookup(findResource, reopenResource))
    {
        openConfiguration = () => configurationIndex < 0 ? null : reopenResource(configurationIndex);
    }

    private static Func<string, byte[]?> CreateLookup(Func<string, int> findResource, Func<int, byte[]?> reopenResource)
    {
        ArgumentNullException.ThrowIfNull(findResource);
        ArgumentNullException.ThrowIfNull(reopenResource);
        return name =>
        {
            var index = findResource(name);
            return index < 0 ? null : reopenResource(index);
        };
    }

    public (byte[]? Data, string Name) Open(string? name)
    {
        // Native callbacks name the root relative to this reader's base directory.
        if (name is null) return (openConfiguration(), "timidity.cfg");
        return Paths.Lookup(name, candidate => VmMusicPatchPaths.IsAbsolute(candidate)
            ? null : openResource(basePath + candidate));
    }
}
