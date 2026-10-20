using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client.Tests;

public class VmPakArchiveReaderTests
{
    [Fact]
    public void Read_PakDirectory_ReturnsStoredLump()
    {
        var payload = Encoding.ASCII.GetBytes("lump");
        var dirOffset = 12u;
        var archive = new byte[dirOffset + 64 + payload.Length];
        "PACK"u8.CopyTo(archive);
        BinaryPrimitives.WriteUInt32LittleEndian(archive.AsSpan(4), dirOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(archive.AsSpan(8), 64);
        var entry = archive.AsSpan((int)dirOffset, 64);
        Encoding.ASCII.GetBytes("maps/start.bsp").CopyTo(entry);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[56..], dirOffset + 64);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[60..], (uint)payload.Length);
        payload.CopyTo(archive, dirOffset + 64);
        var entries = VmPakArchiveReader.Read(archive);
        Assert.Single(entries);
        Assert.Equal("maps/start.bsp", entries[0].Name);
        Assert.Equal(payload, entries[0].Data);
    }
}
