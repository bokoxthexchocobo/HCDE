namespace HCDE.Client.Tests;

public class VmMusicSmfSourceTests
{
    private sealed class Source : IVmMusicSmfSource
    {
        public ushort Division => 480;
        public uint InitialTempo => 500000;
        public int LoopLimit { get; set; }
        public List<string> Calls { get; } = [];
        public Queue<uint[]> Batches { get; } = new();
        public bool Fail { get; init; }
        public int? InvalidCount { get; init; }
        public bool Disposed { get; private set; }
        public void CheckCapabilities(int technology) { Assert.Equal(1, technology); Calls.Add("caps"); }
        public void Restart() => Calls.Add("restart:" + LoopLimit);
        public void StartPlayback(bool looped, int loopLimit) { Assert.False(looped); Assert.Equal(LoopLimit, loopLimit); Calls.Add("start"); }
        public bool CheckDone() { Calls.Add("done"); return !Fail && InvalidCount is null && Batches.Count == 0; }
        public int MakeEvents(Span<uint> buffer, uint maximumTime)
        {
            Assert.Equal(384, buffer.Length);
            Assert.Equal(600000000u, maximumTime);
            Calls.Add("events");
            if (Fail) throw new IOException("decoder failed");
            if (InvalidCount is not null) return InvalidCount.Value;
            var batch = Batches.Dequeue();
            batch.CopyTo(buffer);
            return batch.Length;
        }
        public void Dispose() => Disposed = true;
    }

    [Theory]
    [InlineData(0, 30)]
    [InlineData(-1, 30)]
    [InlineData(7, 7)]
    public void SetupAndBatchIterationPreserveNativeOrderAndRunningStatus(int requested, int expected)
    {
        var source = new Source();
        source.Batches.Enqueue([0, 0, 0x403c90]);
        source.Batches.Enqueue([128, 0, 0x403d90]);
        var bytes = VmMusicSmfSource.Create(source, requested);
        Assert.Equal(VmMusicSmf.Serialize(480, 500000, [new(0, 0x403c90), new(128, 0x403d90)]), bytes);
        Assert.Equal(new[] { "caps", "restart:" + expected, "start", "done", "events", "done", "events", "done" }, source.Calls);
        Assert.Equal(0, source.LoopLimit);
        Assert.False(source.Disposed);
    }

    [Fact]
    public void LongPayloadWordPaddingAndStreamIdAreNotExported()
    {
        var source = new Source();
        source.Batches.Enqueue([3, 99, 0x80000005, 0x030201f0, 0xeeeeeeF7, 4, 42, 0x05c0]);
        Assert.Equal(VmMusicSmf.Serialize(480, 500000,
            [new(3, 0x80000005, new byte[] { 240, 1, 2, 3, 247 }), new(4, 0x05c0)]), VmMusicSmfSource.Create(source, 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(385)]
    public void InvalidBufferCountThrowsAndResetsLoopLimit(int count)
    {
        var source = new Source { InvalidCount = count };
        Assert.Throws<InvalidDataException>(() => VmMusicSmfSource.Create(source, 1));
        Assert.Equal(0, source.LoopLimit);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TruncatedEventsThrowAndResetLoopLimit(bool longEvent)
    {
        var source = new Source();
        source.Batches.Enqueue(longEvent ? new uint[] { 0, 0, 0x80000005, 0x030201f0 } : new uint[] { 0, 0 });
        Assert.Throws<InvalidDataException>(() => VmMusicSmfSource.Create(source, 1));
        Assert.Equal(0, source.LoopLimit);
    }

    [Fact]
    public void DecoderFailurePropagatesAndResetsLoopLimit()
    {
        var source = new Source { Fail = true };
        Assert.Throws<IOException>(() => VmMusicSmfSource.Create(source, 1));
        Assert.Equal(0, source.LoopLimit);
    }

    [Fact]
    public void CommandCanExportPlaybackSourceWithoutCustomSerializationDelegate()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-source-" + Guid.NewGuid().ToString("N") + ".mid");
        var source = new Source();
        try
        {
            VmMusicMidiExport.WriteMidiFile(["writemidi", "song", path], _ => source,
                () => "unused", _ => throw new Exception("unexpected report"));
            Assert.Equal(VmMusicSmf.Serialize(480, 500000, []), File.ReadAllBytes(path));
            Assert.Contains("restart:1", source.Calls);
            Assert.True(source.Disposed);
        }
        finally { File.Delete(path); }
    }
}
