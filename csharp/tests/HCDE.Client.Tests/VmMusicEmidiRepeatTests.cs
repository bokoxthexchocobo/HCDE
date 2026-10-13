namespace HCDE.Client.Tests;

public class VmMusicEmidiRepeatTests
{
    private static VmMusicSmfTrack Track(int repeats) => new(500000,
        [new(0, (uint)(0x74b0 | (repeats << 16))), new(3, 0x403c90), new(5, 0x7f75b0), new(7, 0x403d90)]);

    [Theory]
    [InlineData(2, 30, 3)]
    [InlineData(2, 1, 1)]
    [InlineData(0, 3, 3)]
    [InlineData(1, 30, 2)]
    public void LocalLoopsUseNativeRepeatCountsAndRetainEveryIterationDelay(int repeats, int limit, int plays)
    {
        var output = VmMusicEmidi.Apply(Track(repeats), 480, loopLimit: limit);
        Assert.Equal(plays, output.Events.Count(x => x.Event == 0x403c90));
        Assert.Equal(plays, output.Events.Count(x => x.Event == 0x02000000));
        Assert.Equal(0x403d90u, output.Events[^1].Event);
        Assert.Equal((uint)(plays * 8 + 7), output.Events.Aggregate(0u, (sum, item) => sum + item.Delay));
    }

    [Fact]
    public void UnboundedLocalLoopStopsNonloopedExportAtEndMarker()
    {
        var output = VmMusicEmidi.Apply(Track(0), 480, loopLimit: 0);
        Assert.Equal(new uint[] { 0x403c90, 0x02000000 }, output.Events.Select(x => x.Event));
    }

    [Fact]
    public void Non127EndValueDoesNotJumpAndUnmatchedEndIsConsumed()
    {
        var track = new VmMusicSmfTrack(500000, [new(0, 0x0174b0), new(1, 0x4075b0), new(2, 0x403c90)]);
        var output = VmMusicEmidi.Apply(track, 480, loopLimit: 30);
        Assert.Equal(new uint[] { 0x02000000, 0x403c90 }, output.Events.Select(x => x.Event));
        Assert.Empty(VmMusicEmidi.Apply(new(500000, [new(0, 0x7f75b0)]), 480, loopLimit: 30).Events);
    }

    [Fact]
    public void ConcreteSourceRecompilesLocalLoopsForEachExportLimit()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, Track(0).Events));
        var expected = VmMusicSmf.Serialize(480, 500000, VmMusicEmidi.Apply(Track(0), 480, loopLimit: 3).Events);
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 3));
        var once = VmMusicSmf.Serialize(480, 500000, VmMusicEmidi.Apply(Track(0), 480, loopLimit: 1).Events);
        Assert.Equal(once, VmMusicSmfSource.Create(source, 1));
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 3));
    }
}
