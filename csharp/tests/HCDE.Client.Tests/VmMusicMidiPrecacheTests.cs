namespace HCDE.Client.Tests;

public class VmMusicMidiPrecacheTests
{
    [Fact]
    public void ProgramAndPercussionDiscoveryDeduplicatesAndSortsWithoutDefaultPrograms()
    {
        Assert.Equal(new ushort[] { 2, 5, 16419 }, VmMusicMidiPrecache.Scan([
            new(0, 0x05c0), new(0, 0x02c1), new(0, 0x05c0), new(0, 0x642399),
            new(0, 0x002499), new(0, 0x403c90), new(0, 0x00c9)]));
        Assert.Empty(VmMusicMidiPrecache.Scan([]));
    }

    [Fact]
    public void EveryUsedBankPairsWithEveryInstrumentOfItsOwnCategory()
    {
        Assert.Equal(new ushort[] { 5, 133, 261, 6, 134, 262, 16419, 16803, 16931 },
            VmMusicMidiPrecache.Scan([new(0, 0x05c0), new(0, 0x06c1), new(0, 0x0100b0),
                new(0, 0x0200b1), new(0, 0x642399), new(0, 0x03c9), new(0, 0x0400b9)]));
    }

    [Fact]
    public void TempoLongAndBankLsbEventsDoNotCreateInstrumentsOrBanks()
    {
        Assert.Equal(new ushort[] { 5 }, VmMusicMidiPrecache.Scan([
            new(0, 0x0105c0), new(0, 0x80000003, new byte[] { 240, 5, 247 }),
            new(0, 0x0220b0), new(0, 0x05c0)]));
    }

    [Fact]
    public void ConcreteSourceScanUsesSinglePassAndRewindsWithoutDisposing()
    {
        var bytes = VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x0074b0), new(0, 0x05c0), new(2, 0x7f75b0), new(0, 0x642399)]);
        using var source = new VmMusicSmfPlaybackSource(bytes);
        Assert.Equal(new ushort[] { 5, 16419 }, VmMusicMidiPrecache.Collect(source));
        Assert.Equal(1, source.LoopLimit);
        Assert.False(source.CheckDone());
        Assert.Equal(VmMusicMidiPrecache.Collect(source), VmMusicMidiPrecache.Collect(source));
    }

    private sealed class FailedSource : IVmMusicSmfSource
    {
        public ushort Division => 480;
        public uint InitialTempo => 500000;
        public int LoopLimit { get; set; }
        public int Restarts { get; private set; }
        public void Restart() => Restarts++;
        public void CheckCapabilities(int technology) => throw new Exception("unexpected capabilities");
        public void StartPlayback(bool looped, int loopLimit) => throw new Exception("unexpected start");
        public bool CheckDone() => false;
        public int MakeEvents(Span<uint> buffer, uint maximumTime) => throw new IOException("decoder failed");
        public void Dispose() => throw new Exception("unexpected disposal");
    }

    [Fact]
    public void DecoderFailurePropagatesAfterRewind()
    {
        var source = new FailedSource();
        Assert.Throws<IOException>(() => VmMusicMidiPrecache.Collect(source));
        Assert.Equal(2, source.Restarts);
        Assert.Equal(1, source.LoopLimit);
    }
}
