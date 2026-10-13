namespace HCDE.Client.Tests;

public class VmMusicHmiEventsTests
{
    [Fact]
    public void ZeroDurationOffFollowsCurrentTrackContinuation()
    {
        var track = VmMusicHmiEvents.Decode([0, 0x90, 60, 100, 0, 0, 0xc0, 7]);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x643c90), new(0, 0x07c0), new(0, 0x3c90) }, track.Events);
    }

    [Fact]
    public void IgnoredMetadataAndSpecialRecordsRetainCurrentTrackPriority()
    {
        var track = VmMusicHmiEvents.Decode([0, 0x90, 60, 100, 0,
            0, 0xff, 1, 0, 0, 0xfe, 0x12, 1, 2, 0, 0xc0, 7]);
        Assert.Equal(new uint[] { 0x643c90, 0x07c0, 0x3c90 }, track.Events.Select(x => x.Event));
    }

    [Fact]
    public void OffAtFutureTickPrecedesRealContinuation()
    {
        var track = VmMusicHmiEvents.Decode([0, 0x90, 60, 100, 2, 2, 0xc0, 7, 0, 0xc0, 8]);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x643c90), new(2, 0x3c90), new(0, 0x07c0), new(0, 0x08c0) }, track.Events);
    }

    [Fact]
    public void EndOfTrackEndsCurrentTrackPriorityAndDrainsZeroDurationOff()
    {
        var track = VmMusicHmiEvents.Decode([0, 0x90, 60, 100, 0, 0, 0xff, 47, 0, 0, 0xc0, 7]);
        Assert.Equal(new uint[] { 0x643c90, 0x3c90 }, track.Events.Select(x => x.Event));
        Assert.Empty(VmMusicHmiEvents.Decode([]).Events);
    }

    [Fact]
    public void StandardVlqDelaysAndDurationsScheduleOffsBeforeRealEvents()
    {
        var track = VmMusicHmiEvents.Decode([0x81, 0, 0x92, 60, 100, 5, 5, 0xc0, 7, 0, 0xff, 47, 0]);
        Assert.Equal(new VmMusicSmfEvent[] { new(128, 0x643c92), new(5, 0x3c92), new(0, 0x07c0) }, track.Events);
        Assert.Equal(4000000u, track.InitialTempo);
    }

    [Fact]
    public void RunningNotesAndZeroVelocityStillConsumeDurationsAndDrainAfterEnd()
    {
        var track = VmMusicHmiEvents.Decode([0, 0x90, 60, 100, 8, 2, 61, 0, 3, 0, 0xff, 47, 0]);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x643c90), new(2, 0x3d90),
            new(3, 0x3d90), new(3, 0x3c90) }, track.Events);
    }

    [Fact]
    public void LeadingTempoBecomesInitialTempoAndLaterTempoIsEmitted()
    {
        var track = VmMusicHmiEvents.Decode([0, 0xff, 81, 3, 7, 0xa1, 0x20,
            0, 0xc0, 1, 4, 0xff, 81, 3, 3, 0xd0, 0x90]);
        Assert.Equal(500000u, track.InitialTempo);
        Assert.True(track.HasInitialTempo);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x01c0), new(4, 0x0103d090) }, track.Events);
    }

    [Theory]
    [InlineData(0x12, 2)]
    [InlineData(0x14, 2)]
    [InlineData(0x13, 6)]
    [InlineData(0x15, 6)]
    public void FixedSpecialPayloadsPreserveDelayAndRunningStatus(byte kind, int length)
    {
        var bytes = new byte[] { 0, 0xc0, 1, 5, 0xfe, kind }.Concat(new byte[length])
            .Concat(new byte[] { 0, 2 }).ToArray();
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x01c0), new(5, 0x02000000), new(0, 0x02c0) },
            VmMusicHmiEvents.Decode(bytes).Events);
    }

    [Fact]
    public void VariableSpecialPayloadConsumesLengthBytePlusFourAndPayload()
    {
        byte[] bytes = [0, 0xfe, 0x10, 0, 0, 2, 1, 2, 3, 4, 5, 6, 0, 0xc0, 7];
        Assert.Equal(new VmMusicSmfEvent(0, 0x07c0), Assert.Single(VmMusicHmiEvents.Decode(bytes).Events));
        byte[] hmp = [0x80, 0xfe, 0x12, 1, 2, 0x80, 0xc0, 7];
        Assert.Equal(new VmMusicSmfEvent(0, 0x07c0), Assert.Single(VmMusicHmpEvents.Decode(hmp).Events));
    }

    [Fact]
    public void SysexUsesStandardLengthAndOwnsPayloadForSerialization()
    {
        byte[] bytes = [0, 0xf0, 2, 1, 0xf7, 0, 0xc0, 5];
        var track = VmMusicHmiEvents.Decode(bytes);
        Array.Clear(bytes);
        Assert.Equal(new byte[] { 0xf0, 1, 0xf7 }, track.Events[0].Data.ToArray());
        Assert.NotEmpty(VmMusicSmf.Serialize(120, track.InitialTempo, track.Events));
    }

    [Theory]
    [InlineData(new byte[] { 0, 0x90, 60, 100 })]
    [InlineData(new byte[] { 0, 0x90, 60, 100, 128 })]
    [InlineData(new byte[] { 0, 0xfe, 0x13, 1 })]
    [InlineData(new byte[] { 0, 0xfe, 0x10, 0, 0, 5, 1 })]
    [InlineData(new byte[] { 0, 0xfe, 0x11 })]
    [InlineData(new byte[] { 128, 128, 128, 128 })]
    public void MalformedDurationVlqAndSpecialPayloadsFail(byte[] bytes) =>
        Assert.Throws<InvalidDataException>(() => VmMusicHmiEvents.Decode(bytes));
}
