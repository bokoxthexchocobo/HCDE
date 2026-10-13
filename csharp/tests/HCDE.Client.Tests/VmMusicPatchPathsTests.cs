namespace HCDE.Client.Tests;

public class VmMusicPatchPathsTests
{
    [Fact]
    public void ReaddedPathsTakePriorityAndEmptyPayloadsCountAsOpen()
    {
        var paths = new VmMusicPatchPaths();
        paths.Add("first\\patches");
        paths.Add("second");
        paths.Add("FIRST/patches/");
        Assert.Equal(new[] { "second/", "FIRST/patches/" }, paths.Paths);
        var attempts = new List<string>();
        var found = paths.Lookup("piano.pat", name =>
        {
            attempts.Add(name);
            return name.StartsWith("FIRST", StringComparison.Ordinal) ? [] : null;
        });
        Assert.NotNull(found.Data);
        Assert.Equal("FIRST/patches/piano.pat", found.Name);
        Assert.Single(attempts);
    }

    [Fact]
    public void LookupFallsThroughPathsToDirectNameAndReportsMissingName()
    {
        var paths = new VmMusicPatchPaths();
        paths.Add("one");
        paths.Add("two");
        var attempts = new List<string>();
        var found = paths.Lookup("piano.pat", name => { attempts.Add(name); return name == "piano.pat" ? [1] : null; });
        Assert.Equal(new[] { "two/piano.pat", "one/piano.pat", "piano.pat" }, attempts);
        Assert.Equal(new byte[] { 1 }, found.Data);
        Assert.Equal("", paths.Lookup("missing", _ => null).Name);
        Assert.Throws<IOException>(() => paths.Lookup("piano", _ => throw new IOException("reader failed")));
    }

    [Theory]
    [InlineData("/absolute")]
    [InlineData("C:\\absolute")]
    [InlineData("\\absolute")]
    public void AbsoluteNamesBypassSearchAndPathAdmissionCanBeEnabled(string name)
    {
        var restricted = new VmMusicPatchPaths();
        restricted.Add(name);
        Assert.Empty(restricted.Paths);
        restricted.Add("relative");
        var attempts = new List<string>();
        restricted.Lookup(name, value => { attempts.Add(value); return null; });
        Assert.Equal(new[] { name }, attempts);
        var allowed = new VmMusicPatchPaths(allowAbsolutePaths: true, caseSensitivePaths: true);
        allowed.Add(name);
        allowed.Add("Case");
        allowed.Add("case");
        Assert.Equal(3, allowed.Paths.Count);
    }
}
