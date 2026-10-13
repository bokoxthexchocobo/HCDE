using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigIncludeTests
{
    [Fact]
    public void IncludesHaveIndependentBankAndDirectoryAndShareCatalog()
    {
        var resources = new Dictionary<string, string>
        {
            ["root/child.cfg"] = "0 child\nbank 2\ndir child-patches\n1 violin\nsource nested.cfg",
            ["child-patches/nested.cfg"] = "2 nested"
        };
        var requests = new List<string>();
        var patches = new List<string>();
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { patches.Add(name); return null; });
        var reader = new VmMusicWildMidiConfigReader(catalog, "root/main.cfg", true,
            name => { requests.Add(name); return resources.GetValueOrDefault(name); });
        reader.Read("bank 1\nsource child.cfg\n3 parent");
        foreach (var id in new ushort[] { 0, 0x201, 2, 0x103 })
            Assert.Throws<FileNotFoundException>(() => catalog.LoadSamples(catalog.FindExact(id)!));
        Assert.Equal(new[] { "root/child.cfg", "child-patches/nested.cfg" }, requests);
        Assert.Equal(new[] { "root/child.pat", "child-patches/violin.pat", "child-patches/nested.pat", "root/parent.pat" }, patches);
    }

    [Fact]
    public void AbsoluteIncludesKeepNameAndCanReplaceEarlierDeclarations()
    {
        string? requested = null;
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var original = catalog.Register(0, "first.pat");
        var reader = new VmMusicWildMidiConfigReader(catalog, "root/main.cfg", true,
            name => { requested = name; return "0 replacement"; });
        reader.ApplyLine("SOURCE C:child.cfg");
        Assert.Equal("C:child.cfg", requested);
        Assert.Same(original, catalog.FindExact(0));
    }

    [Fact]
    public void MissingMalformedAndFailingIncludesSurfaceErrors()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var reader = new VmMusicWildMidiConfigReader(catalog, "root/main.cfg", true, _ => null);
        Assert.Throws<InvalidDataException>(() => reader.ApplyLine("source"));
        Assert.Equal("root/missing.cfg", Assert.Throws<FileNotFoundException>(() => reader.ApplyLine("source missing.cfg")).FileName);
        var failure = new IOException("resource failure");
        reader = new(catalog, "main.cfg", true, _ => throw failure);
        Assert.Same(failure, Assert.Throws<IOException>(() => reader.ApplyLine("source child.cfg")));
    }

    [Fact]
    public void RecursiveIncludesStopAtManagedDepthLimit()
    {
        var reads = 0;
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var reader = new VmMusicWildMidiConfigReader(catalog, "main.cfg", true,
            _ => { reads++; return "source main.cfg"; });
        Assert.Throws<InvalidDataException>(() => reader.Read("source main.cfg"));
        Assert.Equal(64, reads);
    }
}
