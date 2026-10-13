namespace HCDE.Client.Tests;

public class VmMusicEmidiGlobalTests
{
    [Theory]
    [InlineData(0, 3, 3)]
    [InlineData(2, 30, 3)]
    [InlineData(2, 1, 1)]
    public void SingleTrackGlobalLoopMatchesLocalLoopOutput(int count, int limit, int plays)
    {
        VmMusicSmfTrack Track(int begin, int end) => new(500000,
            [new(2, (uint)(0xb0 | (begin << 8) | (count << 16))), new(3, 0x403c90),
                new(5, (uint)(0xb0 | (end << 8) | (127 << 16))), new(7, 0x403d90)]);
        var global = VmMusicEmidi.Apply(Track(118, 119), 480, loopLimit: limit, singleTrackGlobalLoops: true);
        var local = VmMusicEmidi.Apply(Track(116, 117), 480, loopLimit: limit);
        Assert.Equal(local.Events, global.Events);
        Assert.Equal(plays, global.Events.Count(x => x.Event == 0x403c90));
    }

    [Fact]
    public void GlobalBeginSharesNativeLoopSlotWithLocalEnd()
    {
        var track = new VmMusicSmfTrack(500000,
            [new(0, 0x0176b0), new(1, 0x403c90), new(2, 0x7f75b0)]);
        var result = VmMusicEmidi.Apply(track, 480, loopLimit: 30, singleTrackGlobalLoops: true);
        Assert.Equal(2, result.Events.Count(x => x.Event == 0x403c90));
    }

    [Fact]
    public void UnsupportedMultitrackGlobalRestoreRemainsExplicit()
    {
        var track = new VmMusicSmfTrack(500000, [new(0, 0x0176b0)]);
        Assert.Throws<InvalidDataException>(() => VmMusicEmidi.Apply(track, 480, loopLimit: 30));
        Assert.Empty(VmMusicEmidi.Apply(track, 480, loopLimit: 1).Events);
    }
}
