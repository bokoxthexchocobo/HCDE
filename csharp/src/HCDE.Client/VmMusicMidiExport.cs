namespace HCDE.Client;

public static class VmMusicMidiExport
{
    public static void WriteMidiFile<TSource>(IReadOnlyList<string> arguments, Func<string, TSource?> openSource,
        Func<string> lastError, Action<string> report) where TSource : class, IVmMusicSmfSource
    {
        WriteMidiFile(arguments, openSource, (source, limit) => VmMusicSmfSource.Create(source, limit), lastError, report);
    }

    public static void WriteMidiFile<TSource>(IReadOnlyList<string> arguments, Func<string, TSource?> openSource,
        Func<TSource, int, byte[]> createSmf, Func<string> lastError, Action<string> report)
        where TSource : class, IDisposable
    {
        ArgumentNullException.ThrowIfNull(createSmf);
        WriteMidi(arguments, openSource,
            (source, output, limit) => VmMusicSmfFile.Write(source, output, limit, createSmf), lastError, report);
    }

    public static void WriteMidi<TSource>(IReadOnlyList<string> arguments, Func<string, TSource?> openSource,
        Func<TSource, string, int, bool> writeSmf, Func<string> lastError, Action<string> report)
        where TSource : class, IDisposable
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(openSource);
        ArgumentNullException.ThrowIfNull(writeSmf);
        ArgumentNullException.ThrowIfNull(lastError);
        ArgumentNullException.ThrowIfNull(report);
        if (arguments.Count != 3)
        {
            report("Usage: writemidi <midisong> <filename> - use '*' as song name to dump the currently playing song\n");
            return;
        }
        using var source = openSource(arguments[1]);
        if (source is null)
        {
            report($"Unable to open {arguments[1]}: {lastError()}\n");
            return;
        }
        // Native passes argv[1] here; use the requested output rather than overwriting the source.
        if (!writeSmf(source, arguments[2], 1)) report($"Unable to write {arguments[2]}\n");
    }
}
