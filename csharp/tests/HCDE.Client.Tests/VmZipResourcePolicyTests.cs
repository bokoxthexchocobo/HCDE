using System.IO.Compression;

namespace HCDE.Client.Tests;

public class VmZipResourcePolicyTests
{
    [Fact]
    public void TryFindEndOfCentralDirectory_LocatesPkEndRecord()
    {
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
            zip.CreateEntry("hello.txt");
        var bytes = memory.ToArray();
        Assert.True(VmZipResourcePolicy.TryFindEndOfCentralDirectory(bytes, out var offset, out var zip64));
        Assert.False(zip64);
        Assert.Equal((byte)'P', bytes[offset]);
    }

    [Fact]
    public void IsMultipartEndRecord_RejectsSplitArchiveFields()
    {
        Span<byte> record = stackalloc byte[22];
        record[4] = 1;
        Assert.True(VmZipResourcePolicy.IsMultipartEndRecord(record, 0, zip64: false));
    }
}
