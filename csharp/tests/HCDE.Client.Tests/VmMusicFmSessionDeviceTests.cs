namespace HCDE.Client.Tests;

public class VmMusicFmSessionDeviceTests
{
    [Fact]
    public void ThrownCompletionStopsPlaybackWithoutReplacingException()
    {
        var renderer = new Renderer();
        var device = new VmMusicFmSessionDevice(renderer, renderer, 1000, 1);
        device.Open();
        device.Prepare(0, new uint[] { 0, 0, 0x02000000 });
        device.Prepare(1, new uint[] { 1, 0, 0x003c90 });
        device.StreamOut(0);
        device.StreamOut(1);
        var failure = new InvalidOperationException("completion threw");
        device.SetCompletionCallback(() => throw failure);
        device.StartStream();
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => device.ServiceStream(new float[2])));
        Assert.False(device.Update());
        float[] output = [99, 99];
        Assert.False(device.ServiceStream(output));
        Assert.Equal(new float[] { 0, 0 }, output);
        device.Close();
    }

    [Fact]
    public void ByteAudioEntryPointUsesReportedFloatStereoFormatAndBoundedBuffer()
    {
        var renderer = new Renderer();
        var device = new VmMusicFmSessionDevice(renderer, renderer, 44100, 2.5f);
        Assert.Equal(new VmMusicStreamFormat(176400, 44100, 2, true), device.GetStreamInfo());
        device.Open();
        device.Prepare(0, new uint[] { 100, 0, 0x003c90 });
        device.StreamOut(0);
        device.StartStream();
        byte[] buffer = Enumerable.Repeat((byte)99, 24).ToArray();
        Assert.True(device.ServiceStreamBytes(buffer.AsSpan(0, 16)));
        Assert.Equal(new float[] { 2.5f, 2.5f, 2.5f, 2.5f },
            System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(buffer.AsSpan(0, 16)).ToArray());
        Assert.All(buffer.Skip(16), value => Assert.Equal((byte)99, value));
        Assert.Throws<ArgumentException>(() => device.ServiceStreamBytes(new byte[9]));
        device.Close();
    }

    [Fact]
    public void OutputGainChangesApplyToFollowingAudioRequests()
    {
        var renderer = new Renderer();
        var device = new VmMusicFmSessionDevice(renderer, renderer, 1000, 1);
        device.Open();
        device.Prepare(0, new uint[] { 100, 0, 0x003c90 });
        device.StreamOut(0);
        device.StartStream();
        var samples = new float[2];
        Assert.True(device.ServiceStream(samples));
        Assert.Equal(new float[] { 1, 1 }, samples);
        device.OutputGain = 2.5f;
        Assert.True(device.ServiceStream(samples));
        Assert.Equal(new float[] { 2.5f, 2.5f }, samples);
        device.Close();
    }

    [Fact]
    public void PreparedBuffersCompleteInOrderAndCanBeRefilledInline()
    {
        var renderer = new Renderer();
        var device = new VmMusicFmSessionDevice(renderer, renderer, 1000, 2);
        device.Open();
        device.SetTimeDivision(500);
        device.Prepare(0, new uint[] { 1, 0, 0x003c90 });
        device.StreamOutSync(0);
        var completions = 0;
        device.SetCompletionCallback(() =>
        {
            device.Unprepare(0);
            if (++completions == 1)
            {
                device.Prepare(0, new uint[] { 0, 0, 0x003d90 });
                device.StreamOut(0);
            }
            return 0;
        });
        device.StartStream();
        float[] samples = [99, 99, 99, 99];
        Assert.False(device.ServiceStream(samples));
        Assert.Equal(new[] { 60, 61 }, renderer.Notes);
        Assert.Equal(2, completions);
        Assert.Equal(new float[] { 2, 2, 0, 0 }, samples);
        Assert.False(device.Update());
    }

    [Fact]
    public void PausePreservesTimingAndStopReleasesQueuedBuffers()
    {
        var renderer = new Renderer();
        var device = new VmMusicFmSessionDevice(renderer, renderer, 1000, 1);
        device.Open();
        device.Prepare(0, new uint[] { 1, 0, 0x003c90 });
        device.StreamOut(0);
        Assert.Throws<InvalidOperationException>(() => device.Unprepare(0));
        device.StartStream();
        device.Pause(true);
        var samples = new float[10];
        Assert.True(device.ServiceStream(samples));
        Assert.Empty(renderer.Notes);
        device.Pause(false);
        Assert.False(device.ServiceStream(samples));
        Assert.Equal(60, Assert.Single(renderer.Notes));
        device.Stop();
        device.Unprepare(0);
        device.Close();
        Assert.False(device.IsOpen);
        Assert.Throws<InvalidOperationException>(() => device.StartStream());
    }

    [Fact]
    public void CompletionErrorStopsPlaybackAndSurfacesError()
    {
        var renderer = new Renderer();
        var device = new VmMusicFmSessionDevice(renderer, renderer, 1000, 1);
        device.Open();
        device.Prepare(0, new uint[] { 0, 0, 0x02000000 });
        device.StreamOut(0);
        device.SetCompletionCallback(() => 42);
        device.StartStream();
        Assert.Contains("42", Assert.Throws<InvalidOperationException>(() => device.ServiceStream(new float[2])).Message);
        Assert.False(device.Update());
        device.Unprepare(0);
        device.Close();
    }

    private sealed class Renderer : IVmMusicFmAudioDevice, IVmMusicFmMidiDevice
    {
        public List<int> Notes { get; } = [];
        public int Generate(Span<float> samples) { samples.Fill(1); return samples.Length; }
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
