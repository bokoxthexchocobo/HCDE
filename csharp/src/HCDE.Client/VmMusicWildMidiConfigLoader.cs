using System.Text;

namespace HCDE.Client;

public readonly record struct VmMusicWildMidiConfigResource(string FileName, ReadOnlyMemory<byte> Data);

public static class VmMusicWildMidiConfigLoader
{
    // Borrows the reader: keep it alive while the catalog can load patches lazily.
    public static VmMusicWildMidiInstrumentCatalog LoadBank(IVmMusicBankReader reader, bool windowsDrivePaths)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var root = reader.Open(null);
        if (root.Data is null) throw new FileNotFoundException("The bank's default configuration was not found.", root.Name);
        var first = true;
        return Load(root.Name, name =>
        {
            if (first)
            {
                first = false;
                return new VmMusicWildMidiConfigResource(root.Name, root.Data);
            }
            var resource = reader.Open(name);
            return resource.Data is null ? null : new VmMusicWildMidiConfigResource(resource.Name, resource.Data);
        }, name => reader.Open(name).Data, windowsDrivePaths);
    }

    public static VmMusicWildMidiInstrumentCatalog LoadFile(string fileName)
    {
        return Load(fileName, name => new(name, VmMusicWildMidiFileReader.Read(name)),
            VmMusicWildMidiFileReader.Read, OperatingSystem.IsWindows());
    }

    public static VmMusicWildMidiInstrumentCatalog Load(string fileName,
        Func<string, VmMusicWildMidiConfigResource?> readConfig, Func<string, byte[]?> readPatch, bool windowsDrivePaths)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentNullException.ThrowIfNull(readConfig);
        ArgumentNullException.ThrowIfNull(readPatch);
        var catalog = new VmMusicWildMidiInstrumentCatalog(readPatch);
        ReadInto(catalog, fileName, readConfig, windowsDrivePaths, 0);
        return catalog;
    }

    private static void ReadInto(VmMusicWildMidiInstrumentCatalog catalog, string requestedName,
        Func<string, VmMusicWildMidiConfigResource?> readConfig, bool windowsDrivePaths, int depth)
    {
        if (depth > 64) throw new InvalidDataException("WildMIDI configuration includes exceed the managed depth limit of 64.");
        var resource = readConfig(requestedName) ?? throw new FileNotFoundException("The configuration resource was not found.", requestedName);
        ArgumentException.ThrowIfNullOrEmpty(resource.FileName);
        if (resource.Data.Length > 0x1fffffff) throw new InvalidDataException("The configuration resource exceeds the native file size limit.");
        // Native configuration is a byte string; preserve every byte rather than replacing invalid UTF-8.
        var reader = new VmMusicWildMidiConfigReader(catalog, resource.FileName, windowsDrivePaths)
        {
            LoadInclude = name => ReadInto(catalog, name, readConfig, windowsDrivePaths, depth + 1)
        };
        reader.Read(Encoding.Latin1.GetString(resource.Data.Span));
    }
}
