namespace HCDE.Client.Tests;

public class VmMusicMidiBufferServiceTests
{
    private sealed class Device : IVmMusicMidiBufferDevice
    {
        public List<string> Calls { get; } = [];
        public List<uint[]> Packets { get; } = [];
        public int UnprepareError { get; set; }
        public int PrepareError { get; set; }
        public int StreamError { get; set; }
        public int Unprepare(int bufferNumber) { Calls.Add($"unprepare:{bufferNumber}"); return UnprepareError; }
        public int Prepare(int bufferNumber, ReadOnlyMemory<uint> events)
        { Calls.Add($"prepare:{bufferNumber}"); Packets.Add(events.ToArray()); return PrepareError; }
        public int StreamOut(int bufferNumber) { Calls.Add($"stream:{bufferNumber}"); return StreamError; }
        public int StreamOutSync(int bufferNumber) { Calls.Add($"sync:{bufferNumber}"); return StreamError; }
    }

    private static VmMusicSmfPlaybackSource Source() => new(VmMusicSmf.Serialize(120, 500000, [new(0, 0x643c90)]));

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void LoopDurationIsTrackedAcrossBuffersAndResetForEachRestart(uint firstDelay)
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(120, 500000,
            new[] { new VmMusicSmfEvent(firstDelay, 0x643c90) }
                .Concat(Enumerable.Repeat(new VmMusicSmfEvent(0, 0x01c0), 300))));
        var service = new VmMusicMidiBufferService(new(source), new Device(), 169);
        Assert.Equal(VmMusicMidiPrimeStatus.Ready, service.Prime(600000, false, true, true).Status);
        void Drain()
        {
            for (var i = 0; i < 100; i++) Assert.Equal(0, service.Service(600000, false, true, false));
        }
        if (firstDelay == 0) Assert.Throws<InvalidDataException>(Drain);
        else Drain();
    }

    [Fact]
    public void PausedBufferTimeDoesNotMakeZeroDurationSongValid()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(120, 500000,
            Enumerable.Repeat(new VmMusicSmfEvent(0, 0x01c0), 300)));
        var service = new VmMusicMidiBufferService(new(source), new Device(), 169);
        service.Prime(600000, false, true, true);
        Assert.Equal(0, service.Service(600000, true, true, false));
        Assert.Throws<InvalidDataException>(() =>
        {
            for (var i = 0; i < 100; i++) service.Service(600000, false, true, false);
        });
    }

    [Fact]
    public void OptedInShortSongQueuesStopAndDoesNotRefillCompletedSource()
    {
        using var source = Source();
        var device = new Device();
        var service = new VmMusicMidiBufferService(new(source), device);
        Assert.Equal(VmMusicMidiPrimeStatus.Ready, service.Prime(600000, false, true).Status);
        Assert.Equal(new[] { "prepare:0", "sync:0", "prepare:1", "sync:1" }, device.Calls);
        var expected = new uint[99];
        VmMusicMidiControlBuffers.WriteStop(expected);
        Assert.Equal(expected, device.Packets[1]);
        Assert.Equal(VmMusicMidiEndState.StopQueued, service.EndState);
        Assert.Equal(0, service.Service(600000, false, false, false));
        Assert.Equal(4, device.Calls.Count);
    }

    [Fact]
    public void PrimeQueuesTwoBuffersSynchronouslyWithoutUnprepare()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(120, 500000,
            Enumerable.Repeat(new VmMusicSmfEvent(0, 0x01c0), 200)));
        var device = new Device();
        var service = new VmMusicMidiBufferService(new(source), device);
        Assert.Equal(new VmMusicMidiPrimeResult(VmMusicMidiPrimeStatus.Ready), service.Prime(600000, false));
        Assert.Equal(new[] { "prepare:0", "sync:0", "prepare:1", "sync:1" }, device.Calls);
        Assert.Equal(0, service.BufferNumber);
        Assert.Equal(0x80000006u, device.Packets[0][2]);
        Assert.Equal(0x0107a120u, device.Packets[0][12]);
        Assert.Throws<InvalidOperationException>(() => service.Prime(600000, false));
        Assert.Equal(0, service.Service(600000, false, false, false));
        Assert.Contains("unprepare:0", device.Calls);
    }

    [Fact]
    public void PrimeReportsShortSongAfterFirstSubmission()
    {
        using var source = Source();
        var device = new Device();
        var service = new VmMusicMidiBufferService(new(source), device);
        Assert.Equal(new VmMusicMidiPrimeResult(VmMusicMidiPrimeStatus.ShortSong), service.Prime(600000, false));
        Assert.Equal(new[] { "prepare:0", "sync:0" }, device.Calls);
        Assert.Equal(1, service.BufferNumber);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PrimeReturnsDeviceFailureWithoutTogglingFailedSlot(bool prepare)
    {
        using var source = Source();
        var device = new Device { PrepareError = prepare ? 8 : 0, StreamError = prepare ? 0 : 8 };
        var service = new VmMusicMidiBufferService(new(source), device);
        Assert.Equal(new VmMusicMidiPrimeResult(VmMusicMidiPrimeStatus.DeviceError, 8), service.Prime(600000, false));
        Assert.Equal(prepare ? 1 : 2, device.Calls.Count);
        Assert.Equal(0, service.BufferNumber);
    }

    [Fact]
    public void ServiceBeforePrimeRejectsLatePriming()
    {
        using var source = Source();
        var service = new VmMusicMidiBufferService(new(source), new Device());
        service.Service(600000, false, false, false);
        Assert.Throws<InvalidOperationException>(() => service.Prime(600000, false));
    }

    [Fact]
    public void CompletionQueuesSeparateStopBufferThenIgnoresFurtherCallbacks()
    {
        using var source = Source();
        var device = new Device();
        var service = new VmMusicMidiBufferService(new(source), device);
        Assert.Equal(0, service.Service(1000000, false, false, false));
        Assert.Equal(1, service.BufferNumber);
        Assert.Equal(0, service.Service(1000000, false, false, false));
        Assert.Equal(VmMusicMidiEndState.StopPending, service.EndState);
        Assert.Single(device.Packets);
        Assert.Equal(0, service.Service(1000000, false, false, false));
        Assert.Equal(VmMusicMidiEndState.StopQueued, service.EndState);
        Assert.Equal(99, device.Packets[1].Length);
        Assert.Equal(500u, device.Packets[1][96]);
        var calls = device.Calls.Count;
        Assert.Equal(0, service.Service(1000000, false, false, false));
        Assert.Equal(calls, device.Calls.Count);
    }

    [Fact]
    public void LoopCompletionRestartsAndQueuesOnAlternateSlot()
    {
        using var source = Source();
        var device = new Device();
        var service = new VmMusicMidiBufferService(new(source), device);
        service.Service(1000000, false, true, false);
        service.Service(1000000, false, true, false);
        Assert.Equal(new[] { "unprepare:0", "prepare:0", "stream:0", "unprepare:1", "prepare:1", "stream:1" }, device.Calls);
        Assert.Equal(102, device.Packets[1].Length);
        Assert.Equal(0x0107a120u, device.Packets[1][2]);
        Assert.Equal(0x643c90u, device.Packets[1][101]);
        Assert.Equal(VmMusicMidiEndState.Playing, service.EndState);
    }

    [Fact]
    public void PausedLoopRequestDefersRestartWithoutSpinning()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        var device = new Device();
        var service = new VmMusicMidiBufferService(playback, device);
        service.Service(600000, false, true, false);
        service.Service(600000, true, true, false);
        Assert.True(playback.RestartPending);
        Assert.Equal(new uint[] { 48, 0, 0x02000000 }, device.Packets[1]);
        service.Service(600000, false, true, false);
        Assert.False(playback.RestartPending);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void DeviceErrorsStopAtFailedOperationWithoutTogglingSlot(int step)
    {
        using var source = Source();
        var device = new Device { UnprepareError = step == 1 ? 17 : 0, PrepareError = step == 2 ? 17 : 0,
            StreamError = step == 3 ? 17 : 0 };
        var service = new VmMusicMidiBufferService(new(source), device);
        Assert.Equal(17, service.Service(600000, false, false, false));
        Assert.Equal(step, device.Calls.Count);
        Assert.Equal(0, service.BufferNumber);
    }

    [Fact]
    public void StopPrepareFailureRetainsPendingStopForRetry()
    {
        using var source = Source();
        var device = new Device();
        var service = new VmMusicMidiBufferService(new(source), device);
        service.Service(600000, false, false, false);
        service.Service(600000, false, false, false);
        device.PrepareError = 9;
        Assert.Equal(9, service.Service(600000, false, false, false));
        Assert.Equal(VmMusicMidiEndState.StopPending, service.EndState);
        device.PrepareError = 0;
        Assert.Equal(0, service.Service(600000, false, false, false));
        Assert.Equal(VmMusicMidiEndState.StopQueued, service.EndState);
    }
}
