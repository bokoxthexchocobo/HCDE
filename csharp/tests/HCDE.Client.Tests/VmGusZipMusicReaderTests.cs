namespace HCDE.Client.Tests;

public class VmGusZipMusicReaderTests
{
    [Fact]
    public void ArchiveConfigAndPatchLookupPrecedeDiskFallbackAndPreserveBinaryData()
    {
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var path = Path.Combine(root, "bank.zip");
            using (var file = File.Create(path))
            using (var zip = new System.IO.Compression.ZipArchive(file, System.IO.Compression.ZipArchiveMode.Create))
            {
                foreach (var name in new[] { "timidity.cfg", "patches/piano.pat" })
                {
                    using var data = zip.CreateEntry(name).Open();
                    data.Write([0, 255, 17]);
                }
            }
            using var reader = new VmGusZipMusicReader(path);
            Assert.Equal(new byte[] { 0, 255, 17 }, reader.Open(null).Data);
            Assert.Equal("timidity.cfg", reader.Open(null).Name);
            reader.Paths.Add("patches");
            Assert.Equal(new byte[] { 0, 255, 17 }, reader.Open("PIANO.PAT").Data);
            Assert.Equal("patches/PIANO.PAT", reader.Open("PIANO.PAT").Name);
            var disk = Path.Combine(root, "external.pat");
            File.WriteAllBytes(disk, [42]);
            Assert.Equal(new byte[] { 42 }, reader.Open(disk).Data);
            Assert.Null(reader.Open("missing.pat").Data);
            reader.Dispose();
            Assert.Throws<ObjectDisposedException>(() => reader.Open(null));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
