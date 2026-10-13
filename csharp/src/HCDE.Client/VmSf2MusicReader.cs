using System.Text;

namespace HCDE.Client;

public sealed class VmSf2MusicReader : IVmMusicBankReader
{
    private readonly string filename;
    private readonly byte[] configuration;
    public VmMusicPatchPaths Paths { get; } = new();

    public VmSf2MusicReader(string filename)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        this.filename = filename;
        configuration = Encoding.UTF8.GetBytes("soundfont \"" + filename + "\"\n");
    }

    public (byte[]? Data, string Name) Open(string? name)
    {
        if (name is null) return (configuration.ToArray(), "timidity.cfg");
        return Paths.Lookup(name, OpenFile);
    }

    private byte[]? OpenFile(string name)
    {
        if (!filename.Equals(name, StringComparison.OrdinalIgnoreCase)) return null;
        return File.ReadAllBytes(name);
    }
}
