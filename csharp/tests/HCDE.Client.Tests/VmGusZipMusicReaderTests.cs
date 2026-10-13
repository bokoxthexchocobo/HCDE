namespace HCDE.Client.Tests;

public class VmGusZipMusicReaderTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void DiskFallbackHonorsResourceLimitAndReleasesRejectedFiles(int limit)
    {
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var path = Path.Combine(root, "bank.zip");
            using (var file = File.Create(path))
            using (var zip = new System.IO.Compression.ZipArchive(file, System.IO.Compression.ZipArchiveMode.Create))
            {
                using var config = zip.CreateEntry("timidity.cfg").Open();
            }
            var disk = Path.Combine(root, "external.pat");
            using var reader = new VmGusZipMusicReader(path, maximumResourceBytes: limit);
            File.WriteAllBytes(disk, new byte[limit]);
            Assert.Equal(new byte[limit], reader.Open(disk).Data);
            File.WriteAllBytes(disk, new byte[limit + 1]);
            Assert.Throws<InvalidDataException>(() => reader.Open(disk));
            using (var exclusive = new FileStream(disk, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                exclusive.SetLength(limit);
            Assert.Equal(new byte[limit], reader.Open(disk).Data);
            using var strict = new VmGusZipMusicReader(path, allowDiskFallback: false, maximumResourceBytes: limit);
            Assert.Null(strict.Open(disk).Data);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

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
