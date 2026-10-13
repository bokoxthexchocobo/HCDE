namespace HCDE.Client.Tests;

public class VmGusResourceMusicReaderTests
{
    [Fact]
    public void IndexedReaderPinsConfigurationButLooksUpPatchOverridesOnEachOpen()
    {
        var currentConfig = 1;
        var currentPatch = 2;
        var configLookups = 0;
        var reader = new VmGusResourceMusicReader("music/custom.cfg", name =>
        {
            if (name == "music/custom.cfg") { configLookups++; return currentConfig; }
            return name == "music/piano.pat" ? currentPatch : -1;
        }, index => [(byte)index]);
        currentConfig = 3;
        Assert.Equal(new byte[] { 1 }, reader.Open(null).Data);
        Assert.Equal(new byte[] { 1 }, reader.Open(null).Data);
        Assert.Equal(1, configLookups);
        Assert.Equal(new byte[] { 2 }, reader.Open("piano.pat").Data);
        currentPatch = 4;
        Assert.Equal(new byte[] { 4 }, reader.Open("piano.pat").Data);
        Assert.Null(reader.Open("missing.pat").Data);
    }

    [Fact]
    public void MissingPinnedConfigurationDoesNotBindToLaterResource()
    {
        var index = -1;
        var reader = new VmGusResourceMusicReader("timidity.cfg", _ => index, _ => throw new InvalidOperationException());
        index = 0;
        Assert.Null(reader.Open(null).Data);
    }

    [Fact]
    public void ResourceConfigAndRelativePatchPathsPreserveBinaryBytes()
    {
        var attempts = new List<string>();
        var resources = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["music/custom.cfg"] = [0, 255],
            ["music/patches/piano.pat"] = [17],
        };
        var reader = new VmGusResourceMusicReader("music/custom.cfg", name =>
        {
            attempts.Add(name);
            return resources.GetValueOrDefault(name);
        });
        Assert.Equal(new byte[] { 0, 255 }, reader.Open(null).Data);
        Assert.Equal("music/timidity.cfg", reader.Open(null).Name);
        reader.Paths.Add("patches");
        Assert.Equal(new byte[] { 17 }, reader.Open("PIANO.PAT").Data);
        Assert.Equal("patches/PIANO.PAT", reader.Open("PIANO.PAT").Name);
        Assert.Contains("music/patches/PIANO.PAT", attempts);
        attempts.Clear();
        Assert.Null(reader.Open("/external.pat").Data);
        Assert.Empty(attempts);
        Assert.Null(reader.Open("missing.pat").Data);
    }

    [Fact]
    public void MissingResourcesAndProviderFailuresRemainVisible()
    {
        var missing = new VmGusResourceMusicReader("timidity.cfg", _ => null);
        Assert.Null(missing.Open(null).Data);
        Assert.Equal("", missing.Open("missing").Name);
        var failed = new VmGusResourceMusicReader("timidity.cfg", _ => throw new IOException("resource read failed"));
        Assert.Throws<IOException>(() => failed.Open(null));
        Assert.Throws<IOException>(() => failed.Open("piano.pat"));
    }
}
