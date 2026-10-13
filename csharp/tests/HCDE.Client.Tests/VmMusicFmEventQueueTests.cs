namespace HCDE.Client.Tests;

public class VmMusicFmEventQueueTests
{
    [Fact]
    public void TickProcessesZeroDelayGroupAndStopsBeforeDelayedEvent()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 44100);
        queue.Submit([2, 0, 0x003c90, 0, 0, 0x003d90, 7, 0, 0x003e90]);
        Assert.Equal(441, queue.InitialDelaySamples);
        Assert.Equal(7u, queue.PlayTick());
        Assert.Equal(new[] { 60, 61 }, device.Notes);
        Assert.Equal(100u, queue.PlayTick());
        Assert.Equal(new[] { 60, 61, 62 }, device.Notes);
        Assert.False(queue.HasEvents);
        Assert.Equal(0u, queue.PlayTick());
    }

    [Fact]
    public void CompletionRefillsEmptyQueueAndContinuesZeroDelayEvents()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 48000);
        var completions = 0;
        queue.BufferCompleted = () =>
        {
            Assert.False(queue.HasEvents);
            if (++completions == 1) queue.Submit([0, 0, 0x003d90]);
        };
        queue.Submit([0, 0, 0x003c90]);
        Assert.Equal(100u, queue.PlayTick());
        Assert.Equal(2, completions);
        Assert.Equal(new[] { 60, 61 }, device.Notes);
    }

    [Fact]
    public void CompletionSeesNextBufferAndItsDelayIsRespected()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 48000);
        queue.Submit([0, 0, 0x003c90]);
        queue.Submit([9, 0, 0x003d90]);
        queue.BufferCompleted = () => Assert.Equal(device.Notes.Count == 1, queue.HasEvents);
        Assert.Equal(9u, queue.PlayTick());
        Assert.Single(device.Notes);
        Assert.Equal(100u, queue.PlayTick());
    }

    [Fact]
    public void TempoChangesRecalculateTickRateBeforeCompletion()
    {
        var queue = new VmMusicFmEventQueue(new Device(), 48000);
        queue.SetTimeDivision(120);
        queue.Submit([0, 0, 0x010f4240]);
        queue.BufferCompleted = () => Assert.Equal(400, queue.SamplesPerTick);
        Assert.Equal(120u, queue.PlayTick());
        Assert.Equal(1000000u, queue.Tempo);
    }

    [Fact]
    public void SubmissionOwnsDataAndRejectsWholeMalformedBufferBeforeQueueing()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 48000);
        Assert.Throws<ArgumentException>(() => queue.Submit(new uint[] { 0, 0, 0x003c90, 0 }));
        Assert.Throws<ArgumentException>(() => queue.Submit(new uint[] { 0, 0, 0x01000000 }));
        Assert.Throws<ArgumentException>(() => queue.Submit(Array.Empty<uint>()));
        Assert.False(queue.HasEvents);
        uint[] buffer = [0, 0, 0x003c90];
        queue.Submit(buffer);
        buffer[2] = 0x003d90;
        queue.PlayTick();
        Assert.Equal(60, Assert.Single(device.Notes));
    }

    [Fact]
    public void CallbackFailurePropagatesAfterCompletedBufferIsRemoved()
    {
        var queue = new VmMusicFmEventQueue(new Device(), 48000);
        var failure = new InvalidOperationException("completion failed");
        queue.BufferCompleted = () => throw failure;
        queue.Submit([0, 0, 0x02000000]);
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => queue.PlayTick()));
        Assert.False(queue.HasEvents);
        Assert.Equal(0u, queue.PlayTick());
    }

    [Fact]
    public void InvalidTimingAndReentrantProcessingFailExplicitly()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VmMusicFmEventQueue(new Device(), 0));
        var queue = new VmMusicFmEventQueue(new Device(), 48000);
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.SetTempo(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.SetTimeDivision(0));
        queue.BufferCompleted = () => Assert.Throws<InvalidOperationException>(() => queue.PlayTick());
        queue.Submit([0, 0, 0x02000000]);
        Assert.Equal(100u, queue.PlayTick());
    }

    private sealed class Device : IVmMusicFmMidiDevice
    {
        public List<int> Notes { get; } = [];
        public void NoteOn(int channel, int note, int velocity) => Notes.Add(note);
        public void NoteOff(int channel, int note) { }
        public void NoteAfterTouch(int channel, int note, int pressure) { }
        public void ControllerChange(int channel, int controller, int value) { }
        public void PatchChange(int channel, int program) { }
        public void ChannelAfterTouch(int channel, int pressure) { }
        public void PitchBend(int channel, int mostSignificant, int leastSignificant) { }
        public void SystemExclusive(ReadOnlySpan<byte> data) { }
    }
}
