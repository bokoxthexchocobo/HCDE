namespace HCDE.Client.Tests;

public class VmMusicMidiStartupTests
{
    private sealed class Device(string? failure = null) : IVmMusicMidiStartupDevice
    {
        public bool FakeVolume => true;
        public List<string> Calls { get; } = [];
        public List<uint[]> Packets { get; } = [];
        public IReadOnlyList<ushort> Instruments { get; private set; } = [];
        private int Call(string name) { Calls.Add(name); return name == failure ? 17 : 0; }
        public void PrecacheInstruments(IReadOnlyList<ushort> instruments)
        { Instruments = instruments; Calls.Add("precache"); }
        public int SetTimeDivision(ushort division) { Assert.Equal(120, division); return Call("division"); }
        public int SetTempo(uint tempo) { Assert.Equal(500000u, tempo); return Call("tempo"); }
        public void InitializePlayback() => Calls.Add("initialize");
        public int StartStream() => Call("start");
        public bool Pause(bool paused) => false;
        public int Unprepare(int number) => Call($"unprepare:{number}");
        public int Prepare(int number, ReadOnlyMemory<uint> events)
        { Packets.Add(events.ToArray()); return Call($"prepare:{number}"); }
        public int StreamOut(int number) => Call($"stream:{number}");
        public int StreamOutSync(int number) => Call($"sync:{number}");
    }

    private static VmMusicSmfPlaybackSource Source(int count = 300) => new(
        VmMusicSmf.Serialize(120, 500000, Enumerable.Range(0, count)
            .Select(i => new VmMusicSmfEvent(1, i == 0 ? 0x05c0u : 0x643c90u))));

    [Fact]
    public void StartsOnlyAfterBothBuffersAreQueuedAndUsesPrecachedInstruments()
    {
        using var source = Source();
        var device = new Device();
        var startup = new VmMusicMidiStartup(source, device);
        Assert.True(startup.Start(600000, true, 0.5f));
        Assert.Equal(new[] { "precache", "division", "tempo", "initialize", "prepare:0", "sync:0",
            "prepare:1", "sync:1", "start" }, device.Calls);
        Assert.Equal(new ushort[] { 5 }, device.Instruments);
        Assert.Equal(VmMusicMidiPlaybackState.Playing, startup.Lifecycle.State);
        Assert.Equal(0, startup.Buffers.BufferNumber);
        Assert.Contains(0x3207b0u, device.Packets[0]);
        Assert.Throws<InvalidOperationException>(() => startup.Start(600000, true));
    }

    [Theory]
    [InlineData("division")]
    [InlineData("tempo")]
    [InlineData("prepare:0")]
    [InlineData("sync:0")]
    [InlineData("prepare:1")]
    [InlineData("sync:1")]
    [InlineData("start")]
    public void FailureLeavesStoppedStateAndDoesNotPerformLaterOperations(string failure)
    {
        using var source = Source();
        var device = new Device(failure);
        var startup = new VmMusicMidiStartup(source, device);
        Assert.Contains("17", Assert.Throws<InvalidOperationException>(() => startup.Start(600000, true)).Message);
        Assert.Equal(failure, device.Calls[^1]);
        Assert.Equal(VmMusicMidiPlaybackState.Stopped, startup.Lifecycle.State);
        var calls = device.Calls.Count;
        Assert.Equal(0, startup.Lifecycle.Service(startup.Buffers, 600000, false, true));
        Assert.Equal(calls, device.Calls.Count);
        Assert.Throws<InvalidOperationException>(() => startup.Start(600000, true));
    }

    [Fact]
    public void ShortSongReturnsFalseWithoutStartingStream()
    {
        using var source = Source(1);
        var device = new Device();
        var startup = new VmMusicMidiStartup(source, device);
        Assert.False(startup.Start(600000, false));
        Assert.DoesNotContain("start", device.Calls);
        Assert.Single(device.Packets);
        Assert.Equal(VmMusicMidiPlaybackState.Stopped, startup.Lifecycle.State);
    }
}
