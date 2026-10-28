using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client.Tests;

public class VmLegacyArchiveReaderTests
{
    [Fact]
    public void HogArchiveReader_ReadsSingleEntry()
    {
        var payload = Encoding.ASCII.GetBytes("data");
        var archive = new byte[3 + 17 + payload.Length];
        "DHF"u8.CopyTo(archive);
        Encoding.ASCII.GetBytes("test").CopyTo(archive.AsSpan(3, 4));
        BinaryPrimitives.WriteUInt32LittleEndian(archive.AsSpan(16), (uint)payload.Length);
        payload.AsSpan().CopyTo(archive.AsSpan(20));
        var entries = VmHogArchiveReader.Read(archive);
        Assert.Equal("test", entries[0].Name);
        Assert.Equal(payload, entries[0].Data);
    }

    [Fact]
    public void GrpArchiveReader_ReadsKenSilvermanHeader()
    {
        var payload = new byte[] { 9, 8, 7 };
        var archive = new byte[16 + 16 + payload.Length];
        Encoding.ASCII.GetBytes("KenSilverman").AsSpan().CopyTo(archive);
        BinaryPrimitives.WriteUInt32LittleEndian(archive.AsSpan(12), 1);
        Encoding.ASCII.GetBytes("FILE1").CopyTo(archive.AsSpan(16, 5));
        BinaryPrimitives.WriteUInt32LittleEndian(archive.AsSpan(28), (uint)payload.Length);
        payload.AsSpan().CopyTo(archive.AsSpan(32));
        var entries = VmGrpArchiveReader.Read(archive);
        Assert.Equal("FILE1", entries[0].Name);
    }

    [Fact]
    public void RffArchiveReader_DecryptsDirectoryAndPayloadPrefix()
    {
        var payload = new byte[] { 1, 2, 3, 4 };
        var directoryOffset = 16u;
        var archive = new byte[16 + 48 + payload.Length];
        "RFF\u001a"u8.CopyTo(archive);
        BinaryPrimitives.WriteUInt32LittleEndian(archive.AsSpan(8), directoryOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(archive.AsSpan(12), 1);
        var directory = new byte[48];
        BinaryPrimitives.WriteUInt32LittleEndian(directory.AsSpan(16), 16);
        BinaryPrimitives.WriteUInt32LittleEndian(directory.AsSpan(20), (uint)payload.Length);
        directory[32] = 0x10;
        Encoding.ASCII.GetBytes("NAM").CopyTo(directory.AsSpan(33, 3));
        Encoding.ASCII.GetBytes("E").CopyTo(directory.AsSpan(36, 1));
        for (var index = 0; index < directory.Length; index++)
            directory[index] ^= unchecked((byte)(directoryOffset + (uint)(index >> 1)));
        directory.CopyTo(archive, 16);
        payload.AsSpan().CopyTo(archive.AsSpan(16 + 48));
        var entries = VmRffArchiveReader.Read(archive);
        Assert.Equal("E.NAM", entries[0].Name);
        Assert.NotEqual(payload[0], entries[0].Data[0]);
    }
}
