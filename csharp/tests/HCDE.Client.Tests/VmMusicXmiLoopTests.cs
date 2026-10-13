namespace HCDE.Client.Tests;

public class VmMusicXmiLoopTests
{
    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(3, 0, 3)]
    [InlineData(3, 1, 1)]
    [InlineData(3, 2, 3)]
    [InlineData(0, 4, 4)]
    public void LoopCountsFollowXmiTotalPassConvention(byte count, int limit, int passes)
    {
        var track = VmMusicXmiEvents.Decode([0xb0, 116, count, 2, 0xc0, 5, 3, 0xb0, 117, 127, 0xc0, 9], limit);
        Assert.Equal(passes, track.Events.Count(x => x.Event == 0x05c0));
        Assert.Equal(5u * (uint)passes, track.Events.Sum(x => (long)x.Delay));
        Assert.Equal(0x09c0u, track.Events[^1].Event);
    }

    [Fact]
    public void BreakControllerAndNestedLoopsRestoreOuterPosition()
    {
        var track = VmMusicXmiEvents.Decode([0xb0, 116, 2, 0xc0, 1, 0xb1, 116, 3,
            0xc0, 2, 0xb2, 117, 0, 0xb3, 117, 127]);
        Assert.Equal(new uint[] { 0x01c0, 0x02c0, 0x01c0, 0x02c0 }, track.Events.Select(x => x.Event));
    }

    [Fact]
    public void NestedFiniteLoopsMultiplyPasses()
    {
        var track = VmMusicXmiEvents.Decode([0xb0, 116, 2, 0xb0, 116, 3, 0xc0, 7,
            0xb0, 117, 127, 0xb0, 117, 127]);
        Assert.Equal(6, track.Events.Count);
    }

    [Fact]
    public void LoopsBeyondFourLevelsAreConsumedWithoutRepeating()
    {
        byte[] begin = [0xb0, 116, 2];
        byte[] end = [0xb0, 117, 127];
        var bytes = Enumerable.Range(0, 5).SelectMany(_ => begin)
            .Concat(new byte[] { 0xc0, 7 }).Concat(Enumerable.Range(0, 5).SelectMany(_ => end)).ToArray();
        Assert.Equal(16, VmMusicXmiEvents.Decode(bytes).Events.Count);
    }

    [Fact]
    public void PendingNoteOffsSurviveLoopJumps()
    {
        var track = VmMusicXmiEvents.Decode([0xb0, 116, 2, 0x90, 60, 100, 8, 3, 0xb0, 117, 127]);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x643c90), new(3, 0x02000000),
            new(0, 0x643c90), new(3, 0x02000000), new(2, 0x3c90), new(3, 0x3c90) }, track.Events);
    }

    [Fact]
    public void NegativeLimitFailsBeforeDecoding() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicXmiEvents.Decode([], -1));
}
