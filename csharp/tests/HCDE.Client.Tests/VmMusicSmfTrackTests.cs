namespace HCDE.Client.Tests;

public class VmMusicSmfTrackTests
{
    [Fact]
    public void LeadingTempoAndRunningStatusDecodeSerializerOutput()
    {
        var expected = new VmMusicSmfEvent[] { new(0, 0x403c90), new(128, 0x403d90), new(0, 0x05c0) };
        var document = VmMusicSmfDocument.Parse(VmMusicSmf.Serialize(480, 1000000, expected));
        var track = VmMusicSmfTrackDecoder.Decode(document.Tracks[0].Span);
        Assert.Equal(1000000u, track.InitialTempo);
        Assert.Equal(expected, track.Events);
    }

    [Fact]
    public void MetaAndEndOfTrackPreserveDelayAndNativeRunningStatus()
    {
        var track = VmMusicSmfTrackDecoder.Decode(Convert.FromHexString("00903C4005FF010141003D4007FF2F00"));
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x403c90), new(5, 0x02000000), new(0, 0x403d90), new(7, 0x02000000) }, track.Events);
    }

    [Fact]
    public void LaterTempoEventsRemainInEventStream()
    {
        var track = VmMusicSmfTrackDecoder.Decode(Convert.FromHexString("00FF51030F424000C00503FF510307A12000FF2F00"));
        Assert.Equal(1000000u, track.InitialTempo);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x05c0), new(3, 0x0107a120) }, track.Events);
    }

    [Fact]
    public void SysExPayloadsAreOwnedAndUseNativePacketFraming()
    {
        var bytes = Convert.FromHexString("00F0030102F702F7020AF700FF2F00");
        var track = VmMusicSmfTrackDecoder.Decode(bytes);
        Array.Clear(bytes);
        Assert.Equal(new byte[] { 240, 1, 2, 247 }, track.Events[0].Data.ToArray());
        Assert.Equal(0x80000004u, track.Events[0].Event);
        Assert.Equal(new byte[] { 10, 247 }, track.Events[1].Data.ToArray());
        Assert.Equal(2u, track.Events[1].Delay);
    }

    [Theory]
    [InlineData("003C40")]
    [InlineData("00903C")]
    [InlineData("00908040")]
    [InlineData("8180808000")]
    [InlineData("00FF51020102")]
    [InlineData("00FF5103000000")]
    [InlineData("00F0040102")]
    [InlineData("00F700")]
    [InlineData("00FF2F0100")]
    [InlineData("00F100")]
    public void MalformedAndUnsupportedEventsAreRejected(string hex)
    {
        Assert.Throws<InvalidDataException>(() => VmMusicSmfTrackDecoder.Decode(Convert.FromHexString(hex)));
    }

    [Fact]
    public void EndOfTrackIgnoresFollowingBytesAndEmptyTrackIsAllowed()
    {
        Assert.Empty(VmMusicSmfTrackDecoder.Decode(Convert.FromHexString("00FF2F00FFFF")).Events);
        Assert.Empty(VmMusicSmfTrackDecoder.Decode([]).Events);
    }
}
