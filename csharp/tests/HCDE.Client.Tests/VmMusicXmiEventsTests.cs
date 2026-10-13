namespace HCDE.Client.Tests;

public class VmMusicXmiEventsTests
{
    [Fact]
    public void AdditiveDelaysAndDurationUseDifferentEncodings()
    {
        var track = VmMusicXmiEvents.Decode([127, 3, 0x92, 60, 100, 0x81, 0, 0xff, 47, 0]);
        Assert.Equal(new VmMusicSmfEvent[] { new(130, 0x643c92), new(128, 0x3c92) }, track.Events);
        Assert.Equal(500000u, track.InitialTempo);
    }

    [Fact]
    public void OffsPrecedeRealEventsAtSameTickAndContinueAfterEnd()
    {
        var track = VmMusicXmiEvents.Decode([0x90, 60, 100, 5, 5, 0x91, 61, 90, 10, 0xff, 47, 0]);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x643c90), new(5, 0x3c90), new(0, 0x5a3d91), new(10, 0x3d91) }, track.Events);
    }

    [Fact]
    public void ZeroDurationEndsNoteBeforeNextRealEvent()
    {
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x3c90), new(0, 0x3c90), new(0, 0x02c0) },
            VmMusicXmiEvents.Decode([0x90, 60, 0, 0, 0xc0, 2]).Events);
    }

    [Fact]
    public void MetadataDoesNotChangeTempoAndRetainsDelay()
    {
        var track = VmMusicXmiEvents.Decode([0xff, 81, 3, 1, 2, 3, 8, 0xff, 1, 1, 42, 0xc0, 4]);
        Assert.Equal(new VmMusicSmfEvent[] { new(8, 0x02000000), new(0, 0x04c0) }, track.Events);
        Assert.False(track.HasInitialTempo);
    }

    [Theory]
    [InlineData(110)]
    [InlineData(111)]
    [InlineData(112)]
    [InlineData(113)]
    [InlineData(115)]
    [InlineData(118)]
    [InlineData(119)]
    [InlineData(120)]
    public void InternalControllersPreserveElapsedTime(byte controller)
    {
        Assert.Equal(new VmMusicSmfEvent[] { new(7, 0x02000000), new(0, 0x09c0) },
            VmMusicXmiEvents.Decode([7, 0xb0, controller, 1, 0xc0, 9]).Events);
    }

    [Fact]
    public void BankSelectionAndChannelMessagesAreTranslated()
    {
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x0300b2), new(0, 0x05d2), new(0, 0x403ce2), new(0, 0x203c82) },
            VmMusicXmiEvents.Decode([0xb2, 114, 3, 0xd2, 5, 0xe2, 60, 64, 0x82, 60, 32]).Events);
    }

    [Fact]
    public void SysexDoesNotConsumeFollowingStatusAndOwnsPayload()
    {
        byte[] bytes = [0xf0, 2, 1, 0xf7, 0xc0, 4, 0xf7, 1, 0xf8];
        var track = VmMusicXmiEvents.Decode(bytes);
        Array.Clear(bytes);
        Assert.Equal(new byte[] { 0xf0, 1, 0xf7 }, track.Events[0].Data.ToArray());
        Assert.Equal(0x04c0u, track.Events[1].Event);
        Assert.Equal(new byte[] { 0xf8 }, track.Events[2].Data.ToArray());
        Assert.NotEmpty(VmMusicSmf.Serialize(60, track.InitialTempo, track.Events));
    }

    [Theory]
    [InlineData(116)]
    [InlineData(117)]
    public void UnmatchedLoopControllersAreConsumed(byte controller) =>
        Assert.Empty(VmMusicXmiEvents.Decode([0xb0, controller, 2]).Events);

    [Theory]
    [InlineData(new byte[] { 3 })]
    [InlineData(new byte[] { 0x90, 60, 100 })]
    [InlineData(new byte[] { 0x90, 60, 100, 0x80 })]
    [InlineData(new byte[] { 0xc0, 128 })]
    [InlineData(new byte[] { 0xf0, 3, 1 })]
    [InlineData(new byte[] { 0xff, 1, 0x80, 0x80, 0x80, 0x80 })]
    [InlineData(new byte[] { 0xf2, 1 })]
    public void MalformedOrUnsupportedEventsFail(byte[] bytes) =>
        Assert.Throws<InvalidDataException>(() => VmMusicXmiEvents.Decode(bytes));
}
