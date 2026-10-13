namespace HCDE.Client.Tests;

public class VmGusFileMusicReaderTests
{
    [Fact]
    public void LastFallbackConfigurationWinsAndPatchesResolveRelativeToItsDirectory()
    {
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var first = Directory.CreateDirectory(Path.Combine(root, "first")).FullName;
            var last = Directory.CreateDirectory(Path.Combine(root, "last")).FullName;
            var name = Guid.NewGuid().ToString("N") + ".cfg";
            File.WriteAllBytes(Path.Combine(first, name), [1]);
            File.WriteAllBytes(Path.Combine(last, name), [2]);
            var reader = new VmGusFileMusicReader(name, [first, last]);
            Assert.Equal(new byte[] { 2 }, reader.Open(null).Data);
            Assert.Equal("timidity.cfg", reader.Open(null).Name);
            var patches = Directory.CreateDirectory(Path.Combine(last, "patches")).FullName;
            File.WriteAllBytes(Path.Combine(patches, "piano.pat"), [0, 255]);
            reader.Paths.Add("patches");
            Assert.Equal(new byte[] { 0, 255 }, reader.Open("piano.pat").Data);
            Assert.Null(reader.Open("missing.pat").Data);
            var direct = new VmGusFileMusicReader(Path.Combine(first, name), [last]);
            Assert.Equal(new byte[] { 1 }, direct.Open(null).Data);
            Assert.Equal(new byte[] { 1 }, reader.Open(Path.Combine(first, name)).Data);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void MissingConfigurationFailsExplicitly() =>
        Assert.Throws<FileNotFoundException>(() => new VmGusFileMusicReader(Guid.NewGuid().ToString("N") + ".cfg", []));
}
