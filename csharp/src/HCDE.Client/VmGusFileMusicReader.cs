namespace HCDE.Client;

public sealed class VmGusFileMusicReader : IVmMusicBankReader
{
    private readonly string configurationPath;
    private readonly string basePath;
    public VmMusicPatchPaths Paths { get; } = new(allowAbsolutePaths: true,
        caseSensitivePaths: !OperatingSystem.IsWindows());

    public VmGusFileMusicReader(string filename, IReadOnlyList<string>? fallbackDirectories = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        string? selected = File.Exists(filename) ? filename : null;
        if (selected is null && !VmMusicPatchPaths.IsAbsolute(filename))
        {
            fallbackDirectories ??= OperatingSystem.IsWindows()
                ? ["C:/TIMIDITY", "/TIMIDITY", AppContext.BaseDirectory]
                : ["/usr/local/lib/timidity", "/etc/timidity", "/etc"];
            foreach (var directory in fallbackDirectories)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(directory);
                var candidate = directory.Replace('\\', '/').TrimEnd('/') + "/" + filename;
                if (File.Exists(candidate)) selected = candidate;
            }
        }
        configurationPath = selected?.Replace('\\', '/')
            ?? throw new FileNotFoundException("GUS configuration was not found", filename);
        var slash = configurationPath.LastIndexOf('/');
        basePath = slash < 0 ? "" : configurationPath[..(slash + 1)];
    }

    public (byte[]? Data, string Name) Open(string? name)
    {
        // Native callback filenames are relative to this reader's base directory.
        if (name is null) return (File.ReadAllBytes(configurationPath), "timidity.cfg");
        return Paths.Lookup(name, candidate =>
        {
            var path = VmMusicPatchPaths.IsAbsolute(candidate) ? candidate : basePath + candidate;
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        });
    }
}
