namespace HCDE.Client;

// The host serializes access. Returned catalogs remain valid after replacement.
public sealed class VmMusicWildMidiCatalogCache
{
    private VmMusicWildMidiInstrumentCatalog? catalog;
    public string LoadedConfiguration { get; private set; } = "";

    public VmMusicWildMidiInstrumentCatalog Load(string fileName,
        Func<string, VmMusicWildMidiInstrumentCatalog> load)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentNullException.ThrowIfNull(load);
        if (catalog is not null && LoadedConfiguration.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            return catalog;
        var next = load(fileName) ?? throw new InvalidOperationException("WildMIDI loader returned no catalog.");
        catalog = next;
        LoadedConfiguration = fileName;
        return next;
    }

    public void Clear()
    {
        catalog = null;
        LoadedConfiguration = "";
    }
}
