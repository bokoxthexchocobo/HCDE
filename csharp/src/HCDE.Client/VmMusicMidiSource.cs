namespace HCDE.Client;

public static class VmMusicMidiSource
{
    public static IVmMusicSmfSource? OpenManaged(string name, string currentSong,
        Func<string, int> findMusicNamespace, Func<string, int> findFullName,
        Func<int, byte[]?> readResource, Action<string> report)
    {
        var factory = new VmMusicMidiSourceFactory();
        return Open(name, currentSong, findMusicNamespace, findFullName, readResource,
            factory.Create, () => factory.LastError, report);
    }

    public static TSource? Open<TSource>(string name, string currentSong, Func<string, int> findMusicNamespace,
        Func<string, int> findFullName, Func<int, byte[]?> readResource,
        Func<ReadOnlyMemory<byte>, VmMusicMidiType, TSource?> createSource, Func<string> lastError,
        Action<string> report) where TSource : class
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(currentSong);
        ArgumentNullException.ThrowIfNull(findMusicNamespace);
        ArgumentNullException.ThrowIfNull(findFullName);
        ArgumentNullException.ThrowIfNull(readResource);
        ArgumentNullException.ThrowIfNull(createSource);
        ArgumentNullException.ThrowIfNull(lastError);
        ArgumentNullException.ThrowIfNull(report);
        if (name == "*") name = currentSong;
        var resource = findMusicNamespace(name);
        if (resource < 0) resource = findFullName(name);
        if (resource < 0)
        {
            report($"Cannot find MIDI lump {name}.\n");
            return null;
        }
        var data = readResource(resource);
        if (data is null || data.Length < 32)
        {
            report($"Unable to read lump {name}\n");
            return null;
        }
        var type = VmMusicMidiDetector.Identify(data.AsSpan(0, 32));
        if (type == VmMusicMidiType.None)
        {
            report($"{name} is not MIDI-based.\n");
            return null;
        }
        var source = createSource(data, type);
        if (source is null) report($"Unable to open {name}: {lastError()}\n");
        return source;
    }
}
