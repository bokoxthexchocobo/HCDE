namespace HCDE.Client;

public static class VmMusicSmfFile
{
    public static bool Write<TSource>(TSource? source, string output, int loopLimit,
        Func<TSource, int, byte[]> createSmf) where TSource : class
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(createSmf);
        if (source is null) return false;
        // Generate before opening the destination so decoder failures do not truncate it.
        var bytes = createSmf(source, loopLimit);
        ArgumentNullException.ThrowIfNull(bytes);
        try
        {
            File.WriteAllBytes(output, bytes);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
