using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigPathsTests
{
    [Fact]
    public void ConfigDirectoryAndPatchSuffixFollowNativeStringRules()
    {
        var paths = new VmMusicWildMidiConfigPaths("root\\sub/config.cfg", true);
        Assert.Equal("root\\sub/", paths.Directory);
        Assert.Equal("root\\sub/piano.pat", paths.ResolvePatch("piano"));
        Assert.Equal("root\\sub/a.pat", paths.ResolvePatch("a"));
        Assert.Equal("root\\sub/piano.PAT", paths.ResolvePatch("piano.PAT"));
        Assert.Equal("root\\sub/../piano.pat", paths.ResolvePatch("../piano"));
        Assert.Equal("piano.pat", new VmMusicWildMidiConfigPaths("config.cfg", false).ResolvePatch("piano"));
    }

    [Fact]
    public void AbsoluteNamesAndDriveSpecificationsUseSelectedNativePlatform()
    {
        var windows = new VmMusicWildMidiConfigPaths("root/config.cfg", true);
        var unix = new VmMusicWildMidiConfigPaths("root/config.cfg", false);
        Assert.Equal("/patch.pat", windows.ResolvePatch("/patch"));
        Assert.Equal("\\patch.pat", unix.ResolvePatch("\\patch"));
        Assert.Equal("C:patch.pat", windows.ResolvePatch("C:patch"));
        Assert.Equal("root/C:patch.pat", unix.ResolvePatch("C:patch"));
    }

    [Fact]
    public void DirectoryDirectiveReplacesRatherThanRebasesAndPreservesSeparators()
    {
        var paths = new VmMusicWildMidiConfigPaths("root/config.cfg", true);
        Assert.True(paths.Apply("DiR relative"));
        Assert.Equal("relative/piano.pat", paths.ResolvePatch("piano"));
        Assert.True(paths.Apply("dir patches\\"));
        Assert.Equal("patches\\piano.pat", paths.ResolvePatch("piano"));
        Assert.False(paths.Apply("bank 2"));
        Assert.Throws<InvalidDataException>(() => paths.Apply("dir"));
        Assert.Equal("patches\\", paths.Directory);
    }

    [Fact]
    public void ConfiguredCatalogReadsResolvedResourceName()
    {
        string? readName = null;
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { readName = name; return null; });
        var paths = new VmMusicWildMidiConfigPaths("patches/config.cfg", true);
        var patch = catalog.RegisterConfigured(35, "kick", paths);
        Assert.Throws<FileNotFoundException>(() => catalog.LoadSamples(patch));
        Assert.Equal("patches/kick.pat", readName);
        Assert.Same(patch, catalog.FindExact(35));
    }
}
