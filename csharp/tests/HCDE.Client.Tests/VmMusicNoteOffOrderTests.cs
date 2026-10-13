namespace HCDE.Client.Tests;

public class VmMusicNoteOffOrderTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    public void EqualTimeOffsFollowNativeRootReplacementOrder(int count)
    {
        foreach (var hmi in new[] { false, true })
        {
            var bytes = new List<byte>();
            for (var i = 0; i < count; i++)
            {
                if (hmi) bytes.Add(0);
                bytes.AddRange(new byte[] { 0x90, (byte)(60 + i), 100, 10 });
            }
            var track = hmi ? VmMusicHmiEvents.Decode(bytes.ToArray()) : VmMusicXmiEvents.Decode(bytes.ToArray());
            var offs = track.Events.Where(x => (x.Event >> 16) == 0).ToArray();
            var expectedKeys = new[] { 60 }.Concat(Enumerable.Range(61, count - 1).Reverse()).ToArray();
            Assert.Equal(expectedKeys, offs.Select(x => (int)(x.Event >> 8)));
            Assert.Equal(10u, offs[0].Delay);
            Assert.All(offs.Skip(1), item => Assert.Equal(0u, item.Delay));
        }
    }

    [Fact]
    public void EarlierInsertionsBubbleUpAndRemovalRestoresHeap()
    {
        var track = VmMusicXmiEvents.Decode([0x90, 60, 100, 10, 0x90, 61, 100, 5,
            0x90, 62, 100, 5, 0x90, 63, 100, 20]);
        Assert.Equal(new VmMusicSmfEvent[] { new(5, 0x3d90), new(0, 0x3e90),
            new(5, 0x3c90), new(10, 0x3f90) }, track.Events.Skip(4));
    }

    [Fact]
    public void QueueCanDrainAndAcceptNewNotesDuringPlayback()
    {
        var track = VmMusicXmiEvents.Decode([0x90, 60, 100, 2, 3, 0x91, 61, 100, 4]);
        Assert.Equal(new VmMusicSmfEvent[] { new(0, 0x643c90), new(2, 0x3c90),
            new(1, 0x643d91), new(4, 0x3d91) }, track.Events);
    }
}
