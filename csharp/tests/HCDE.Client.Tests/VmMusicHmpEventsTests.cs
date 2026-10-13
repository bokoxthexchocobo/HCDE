namespace HCDE.Client.Tests;

public class VmMusicHmpEventsTests
{
    [Fact]
    public void LittleEndianDelaysUseSetHighBitAsTerminator()
    {
        var track = VmMusicHmpEvents.Decode([1, 0x82, 0x90, 60, 100, 0x80, 0x80, 60, 0]);
        Assert.Equal(new VmMusicSmfEvent[] { new(257, 0x643c90), new(0, 0x3c80) }, track.Events);
        Assert.Equal(1000000u, track.InitialTempo);
    }

    [Fact]
    public void RunningStatusSurvivesMetadataAndSysEx()
    {
        var track = VmMusicHmpEvents.Decode([0x80, 0xc2, 4, 0x80, 0xff, 1, 0x81, 42,
            0x80, 5, 0x80, 0xf0, 0x81, 0xf7, 0x80, 6]);
        Assert.Equal(new uint[] { 0x04c2, 0x05c2, 0x80000002, 0x06c2 }, track.Events.Select(x => x.Event));
        Assert.Equal(new byte[] { 0xf0, 0xf7 }, track.Events[2].Data.ToArray());
    }

    [Fact]
    public void TempoAndIgnoredMetadataRetainTimeAndEndStopsParsing()
    {
        var track = VmMusicHmpEvents.Decode([0x80, 0xff, 81, 0x83, 7, 0xa1, 0x20,
            0x84, 0xff, 1, 0x80, 0x82, 0xff, 47, 0x80, 1]);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x0107a120), new(4, 0x02000000), new(2, 0x02000000) }, track.Events);
        Assert.False(track.HasInitialTempo);
        Assert.NotEmpty(VmMusicSmf.Serialize(120, track.InitialTempo, track.Events));
    }

    [Fact]
    public void AcceptsFull32BitDelayWithoutShiftOverflow()
    {
        Assert.Equal(uint.MaxValue, Assert.Single(VmMusicHmpEvents.Decode([127, 127, 127, 127, 0x8f, 0xc0, 1]).Events).Delay);
    }

    [Fact]
    public void EscapePayloadIsOwnedAndFollowingEventRemainsIntact()
    {
        byte[] bytes = [0x80, 0xf7, 0x82, 1, 2, 0x80, 0xd0, 3];
        var track = VmMusicHmpEvents.Decode(bytes);
        Array.Clear(bytes);
        Assert.Equal(new byte[] { 1, 2 }, track.Events[0].Data.ToArray());
        Assert.Equal(0x03d0u, track.Events[1].Event);
    }

    [Theory]
    [InlineData(new byte[] { 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0x90, 0xc0, 1 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0x80 })]
    [InlineData(new byte[] { 0x80, 60, 100 })]
    [InlineData(new byte[] { 0x80, 0x90, 60 })]
    [InlineData(new byte[] { 0x80, 0xc0, 128 })]
    [InlineData(new byte[] { 0x80, 0xf0, 0x83, 1 })]
    [InlineData(new byte[] { 0x80, 0xff, 81, 0x82, 1, 2 })]
    [InlineData(new byte[] { 0x80, 0xff, 81, 0x83, 0, 0, 0 })]
    [InlineData(new byte[] { 0x80, 0xfe, 0x11, 1, 2 })]
    public void InvalidAndUnconvertedEventsFailExplicitly(byte[] bytes) =>
        Assert.Throws<InvalidDataException>(() => VmMusicHmpEvents.Decode(bytes));
}
