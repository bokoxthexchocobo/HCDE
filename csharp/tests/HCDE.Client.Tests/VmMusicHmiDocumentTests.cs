using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicHmiDocumentTests
{
    private static byte[] Hmi()
    {
        var bytes = new byte[0x1aa];
        "HMI-MIDISONG061595\0"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xd4), 30);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xe4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0xe8), 0xf0);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0xf0), 0x100);
        "HMI-MIDITRACK\0"u8.CopyTo(bytes.AsSpan(0x100));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x157), 0xa9);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0x199), 0xa001);
        bytes[^1] = 99;
        return bytes;
    }
    private static byte[] Hmp(bool newer)
    {
        var offset = newer ? 0x388 : 0x308;
        var bytes = new byte[offset + 26];
        "HMIMIDIP"u8.CopyTo(bytes);
        if (newer) "013195\0"u8.CopyTo(bytes.AsSpan(8));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x30), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0x38), 120);
        for (var i = 0; i < 2; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + i * 13 + 4), 13);
            bytes[offset + i * 13 + 12] = (byte)(i + 1);
        }
        return bytes;
    }

    [Fact]
    public void HmiUsesQuarterDivisionAndOwnsEventsAndDesignations()
    {
        var bytes = Hmi();
        var document = VmMusicHmiDocument.Parse(bytes);
        Array.Clear(bytes);
        Assert.False(document.IsHmp);
        Assert.Equal(120, document.Division);
        Assert.Equal(4000000u, document.InitialTempo);
        var track = Assert.Single(document.Tracks);
        Assert.Equal(new byte[] { 99 }, track.Events.ToArray());
        Assert.Equal(0xa001, track.Designations[0]);
        Assert.Equal(8, track.Designations.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothHmpVersionsPreserveTrackOrderAndDefaultDesignations(bool newer)
    {
        var bytes = Hmp(newer);
        var document = VmMusicHmiDocument.Parse(bytes);
        Array.Clear(bytes);
        Assert.True(document.IsHmp);
        Assert.Equal(120, document.Division);
        Assert.Equal(1000000u, document.InitialTempo);
        Assert.Equal(new byte[] { 1, 2 }, document.Tracks.Select(x => x.Events.Span[0]));
        Assert.Equal(new ushort[] { 0xa000, 0xa00a, 0xa002, 0, 0, 0, 0, 0 }, document.Tracks[0].Designations);
    }

    [Theory]
    [InlineData(0xe8, uint.MaxValue)]
    [InlineData(0xf0, uint.MaxValue)]
    [InlineData(0x157, 0x80u)]
    [InlineData(0x157, uint.MaxValue)]
    public void HmiInvalidDirectoryAndOffsetsFail(int offset, uint value)
    {
        var bytes = Hmi();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
        Assert.Throws<InvalidDataException>(() => VmMusicHmiDocument.Parse(bytes));
    }

    [Theory]
    [InlineData(0x30, 0u)]
    [InlineData(0x30, uint.MaxValue)]
    [InlineData(0x38, 0u)]
    [InlineData(0x38, 65536u)]
    [InlineData(0x30c, 0u)]
    [InlineData(0x30c, 11u)]
    [InlineData(0x30c, uint.MaxValue)]
    public void HmpInvalidCountsDivisionAndLengthsFail(int offset, uint value)
    {
        var bytes = Hmp(false);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
        Assert.Throws<InvalidDataException>(() => VmMusicHmiDocument.Parse(bytes));
    }

    [Fact]
    public void UnknownSignaturesVersionsAndEmptyTracksFail()
    {
        Assert.Throws<InvalidDataException>(() => VmMusicHmiDocument.Parse(new byte[255]));
        Assert.Throws<InvalidDataException>(() => VmMusicHmiDocument.Parse(new byte[256]));
        var bytes = Hmp(false); bytes[8] = 1;
        Assert.Throws<InvalidDataException>(() => VmMusicHmiDocument.Parse(bytes));
        bytes = Hmi(); bytes[0x100] = 0;
        Assert.Throws<InvalidDataException>(() => VmMusicHmiDocument.Parse(bytes));
        bytes = Hmi(); BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0xd4), 16384);
        Assert.Throws<InvalidDataException>(() => VmMusicHmiDocument.Parse(bytes));
    }
}
