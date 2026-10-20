using SharpCompress.Writers.SevenZip;

namespace HCDE.Client.Tests;

public class VmSevenZipArchiveReaderTests
{
    [Fact]
    public void GetEntries_SkipsDirectoriesAndReadsFile()
    {
        using var memory = new MemoryStream();
        using (var writer = new SevenZipWriter(memory, new SevenZipWriterOptions(SharpCompress.Common.CompressionType.LZMA2)))
            writer.Write("hello.txt", new MemoryStream("data"u8.ToArray()), null);

        using var reader = new VmSevenZipArchiveReader(memory.ToArray());
        var entries = reader.GetEntries();
        Assert.Single(entries);
        Assert.Equal("hello.txt", entries[0].Name);
        Assert.Equal("data"u8.ToArray(), entries[0].Read());
    }
}
