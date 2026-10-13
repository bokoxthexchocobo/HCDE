using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicSmfTests
{
    private static byte[] Track(params VmMusicSmfEvent[] events)
    {
        var bytes = VmMusicSmf.Serialize(480, 500000, events);
        Assert.Equal((uint)(bytes.Length - 22), BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(18, 4)));
        return bytes[29..];
    }

    [Fact]
    public void EmptySongProducesCompleteFormatZeroFile()
    {
        Assert.Equal(Convert.FromHexString("4D546864000000060000000101E04D54726B0000000B00FF510307A12000FF2F00"),
            VmMusicSmf.Serialize(480, 500000, []));
    }

    [Fact]
    public void ChannelMessagesMaskDataAndShareRunningStatus()
    {
        Assert.Equal(Convert.FromHexString("00903C7F81403D408100C00500D00600803C0000FF2F00"),
            Track(new(0, 0xffbc90), new(192, 0x403d90), new(128, 0x05c0),
                new(0, 0x06d0), new(0, 0x3c80)));
    }

    [Fact]
    public void TempoResetsRunningStatusAndIgnoredEventsAccumulateDelay()
    {
        Assert.Equal(Convert.FromHexString("00903C4005FF510301020300903C4007FF2F00"),
            Track(new(0, 0x403c90), new(2, 0x02000000), new(3, 0x01010203),
                new(0, 0x403c90), new(7, 0x02000000)));
    }

    [Fact]
    public void SysExAndEscapePacketsFramePayloadAndResetRunningStatus()
    {
        Assert.Equal(Convert.FromHexString("00903C4001F0030102F700903C4002F7020AF700FF2F00"),
            Track(new(0, 0x403c90), new(1, 0x80000004, new byte[] { 240, 1, 2, 247 }),
                new(0, 0x403c90), new(2, 0x80000002, new byte[] { 10, 247 })));
    }

    [Theory]
    [InlineData(127u, "7F")]
    [InlineData(128u, "8100")]
    [InlineData(16383u, "FF7F")]
    [InlineData(16384u, "818000")]
    [InlineData(268435455u, "FFFFFF7F")]
    public void EndTrackDelayUsesVariableLengthEncoding(uint delay, string hex)
    {
        Assert.Equal(Convert.FromHexString(hex + "FF2F00"), Track(new VmMusicSmfEvent(delay, 0x02000000)));
    }

    [Theory]
    [InlineData(0x80000000u)]
    [InlineData(0x80000002u)]
    [InlineData(0x00000070u)]
    [InlineData(0x000000f0u)]
    public void MalformedPayloadsAreRejected(uint word)
    {
        Assert.Throws<ArgumentException>(() => Track(new VmMusicSmfEvent(0, word)));
    }

    [Fact]
    public void ExcessiveDelayIsRejectedInsteadOfWritingTruncatedVariableValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Track(new VmMusicSmfEvent(0x10000000, 0x02000000)));
    }
}
