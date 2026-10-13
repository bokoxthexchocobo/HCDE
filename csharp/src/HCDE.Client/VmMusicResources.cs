namespace HCDE.Client;

public sealed class VmMusicResources
{
    public static string ChangeName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return name.StartsWith("file://", StringComparison.Ordinal) ? name[7..] : name;
    }

    private readonly Func<string, int> findMusic;
    private readonly Func<string, int> findMusicNamespace;
    private readonly Func<int, long> resourceLength;
    private readonly Func<int, byte[]> readResource;
    private readonly Action<string> report;

    public VmMusicResources(Func<string, int> findMusic, Func<string, int> findMusicNamespace,
        Func<int, long> resourceLength, Func<int, byte[]> readResource, Action<string> report)
    {
        ArgumentNullException.ThrowIfNull(findMusic);
        ArgumentNullException.ThrowIfNull(findMusicNamespace);
        ArgumentNullException.ThrowIfNull(resourceLength);
        ArgumentNullException.ThrowIfNull(readResource);
        ArgumentNullException.ThrowIfNull(report);
        this.findMusic = findMusic;
        this.findMusicNamespace = findMusicNamespace;
        this.resourceLength = resourceLength;
        this.readResource = readResource;
        this.report = report;
    }

    private int Find(string name)
    {
        var index = findMusic(name);
        return index == -1 ? findMusicNamespace(name) : index;
    }

    public bool Exists(string? name)
    {
        if (name is null) return false;
        if (File.Exists(name)) return true;
        var index = Find(name);
        return index != -1 && resourceLength(index) != 0;
    }

    public VmMusicByteReader? Open(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (File.Exists(name)) return new(File.ReadAllBytes(name));
        var index = Find(name);
        if (index == -1)
        {
            report($"Music \"{name}\" not found\n");
            return null;
        }
        return resourceLength(index) == 0 ? null : new VmMusicByteReader(readResource(index));
    }
}
