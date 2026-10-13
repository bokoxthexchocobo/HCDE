using System.Text;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigLoaderTests
{
    [Fact]
    public void ResourceReturnedNamesDetermineIncludedPatchDirectories()
    {
        var reads = new List<string>();
        var catalog = VmMusicWildMidiConfigLoader.Load("alias", name => name switch
        {
            "alias" => new("resolved/main.cfg", Encoding.Latin1.GetBytes("source child.cfg\n1 parent")),
            "resolved/child.cfg" => new("archive/child.cfg", Encoding.Latin1.GetBytes("0 child")),
            _ => null
        }, name => { reads.Add(name); return null; }, true);
        Assert.Throws<FileNotFoundException>(() => catalog.LoadSamples(catalog.FindExact(0)!));
        Assert.Throws<FileNotFoundException>(() => catalog.LoadSamples(catalog.FindExact(1)!));
        Assert.Equal(new[] { "archive/child.pat", "resolved/parent.pat" }, reads);
    }

    [Fact]
    public void ByteDecodingPreservesNonAsciiNamesAndUnixBackslashes()
    {
        string? read = null;
        var catalog = VmMusicWildMidiConfigLoader.Load("config", _ => new("root\\config.cfg", new byte[] { 48, 32, 233 }),
            name => { read = name; return null; }, false);
        Assert.Throws<FileNotFoundException>(() => catalog.LoadSamples(catalog.FindExact(0)!));
        Assert.Equal("\u00e9.pat", read);
    }

    [Fact]
    public void MissingAndRecursiveResourcesFailExplicitly()
    {
        Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiConfigLoader.Load("missing", _ => null, _ => null, true));
        var reads = 0;
        Assert.Throws<InvalidDataException>(() => VmMusicWildMidiConfigLoader.Load("config", name =>
        {
            reads++;
            return new(name, Encoding.ASCII.GetBytes("source config"));
        }, _ => null, true));
        Assert.Equal(65, reads);
    }
}
