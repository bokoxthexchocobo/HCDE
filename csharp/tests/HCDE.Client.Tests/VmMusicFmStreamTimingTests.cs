namespace HCDE.Client.Tests;

public class VmMusicFmStreamTimingTests
{
    [Fact]
    public void FractionalTimingSurvivesSplitAudioBuffers()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 1000);
        queue.SetTimeDivision(200); // 2.5 samples per tick.
        queue.Submit([1, 0, 0x003c90, 1, 0, 0x003d90, 1, 0, 0x003e90]);
        Assert.True(queue.ServiceStream(device, new float[2], 2));
        Assert.Empty(device.Notes);
        Assert.Equal(1.5, queue.NextTickInSamples);
        Assert.True(queue.ServiceStream(device, new float[2], 2));
        Assert.Equal(new[] { 60 }, device.Notes);
        Assert.Equal(3, queue.NextTickInSamples);
        Assert.True(queue.ServiceStream(device, new float[4], 2));
        Assert.Single(device.Notes);
        float[] output = [99, 99, 99, 99, 99, 99];
        Assert.False(queue.ServiceStream(device, output, 2));
        Assert.Equal(new[] { 60, 61, 62 }, device.Notes);
        Assert.Equal(new float[] { 2, 2, 2, 2, 2, 2 }, output);
        Assert.Equal(new[] { 1, 1, 2, 1, 2 }, device.RenderFrames);
    }

    [Fact]
    public void ExhaustionLeavesUnrenderedTailSilent()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 1000);
        queue.SetTimeDivision(500);
        queue.Submit([1, 0, 0x003c90]);
        float[] output = [99, 99, 99, 99, 99, 99];
        Assert.False(queue.ServiceStream(device, output, 3));
        Assert.Equal(new float[] { 3, 3, 0, 0, 0, 0 }, output);
        Assert.Equal(60, Assert.Single(device.Notes));
    }

    [Fact]
    public void TempoEventChangesFollowingSampleInterval()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 1000);
        queue.SetTimeDivision(1000);
        queue.Submit([0, 0, 0x010f4240, 3, 0, 0x003c90]);
        Assert.True(queue.ServiceStream(device, new float[4], 1));
        Assert.Empty(device.Notes);
        Assert.Equal(1, queue.NextTickInSamples);
        Assert.False(queue.ServiceStream(device, new float[2], 1));
        Assert.Equal(60, Assert.Single(device.Notes));
    }

    [Fact]
    public void CallbackRefillContinuesRenderingInSameRequest()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 1000);
        queue.SetTimeDivision(500);
        var completions = 0;
        queue.BufferCompleted = () =>
        {
            if (++completions == 1) queue.Submit([0, 0, 0x003d90, 2, 0, 0x003e90]);
        };
        queue.Submit([0, 0, 0x003c90]);
        float[] output = [99, 99, 99, 99, 99, 99];
        Assert.False(queue.ServiceStream(device, output, 1));
        Assert.Equal(new[] { 60, 61, 62 }, device.Notes);
        Assert.Equal(new float[] { 1, 1, 1, 1, 0, 0 }, output);
        Assert.Equal(2, completions);
    }

    [Fact]
    public void EmptyAndZeroFrameRequestsDoNotDispatchEvents()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 1000);
        float[] output = [99, 99];
        Assert.False(queue.ServiceStream(device, output, 1));
        Assert.Equal(new float[] { 0, 0 }, output);
        queue.Submit([0, 0, 0x003c90]);
        Assert.True(queue.ServiceStream(device, [], 1));
        Assert.Empty(device.Notes);
        Assert.Empty(device.RenderFrames);
    }

    [Fact]
    public void RenderFailurePreservesCountdownAndAllowsExplicitRetry()
    {
        var device = new Device();
        var queue = new VmMusicFmEventQueue(device, 1000);
        queue.Submit([1, 0, 0x003c90]);
        var failure = new InvalidOperationException("generate failed");
        device.Failure = failure;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => queue.ServiceStream(device, new float[2], 1)));
        Assert.Equal(5, queue.NextTickInSamples);
        device.Failure = null;
        Assert.True(queue.ServiceStream(device, new float[2], 1));
        Assert.Equal(4, queue.NextTickInSamples);
        Assert.Throws<ArgumentException>(() => queue.ServiceStream(device, new float[3], 1));
    }

    private sealed class Device : IVmMusicFmMidiDevice, IVmMusicFmAudioDevice
    {
        public List<int> Notes { get; } = [];
        public List<int> RenderFrames { get; } = [];
        public Exception? Failure { get; set; }
        public int Generate(Span<float> stereoSamples)
        {
            if (Failure is not null) throw Failure;
            RenderFrames.Add(stereoSamples.Length / 2);
            stereoSamples.Fill(1);
            return stereoSamples.Length;
        }
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
