namespace HCDE.Client.Tests;

public class VmMusicFmSessionIntegrationTests
{
    private static VmMusicSmfPlaybackSource Source() => new(VmMusicSmf.Serialize(120, 500000,
        Enumerable.Range(0, 300).Select(i => new VmMusicSmfEvent(1, (uint)(0x640090 | ((60 + i % 12) << 8))))));

    [Fact]
    public void SmfSessionDrivesRendererThroughCompletionAndCleanup()
    {
        using var source = Source();
        var renderer = new Renderer();
        var device = new VmMusicFmSessionDevice(renderer, 1000, 2);
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        Assert.Equal(1, renderer.Resets);
        var output = new float[100];
        for (var i = 0; i < 100 && device.IsOpen; i++)
        {
            device.ServiceStream(output);
            session.Update();
        }
        Assert.False(device.IsOpen);
        Assert.False(session.IsPlaying());
        Assert.Equal(300, renderer.Notes.Count);
        Assert.Equal(Enumerable.Range(0, 300).Select(i => 60 + i % 12), renderer.Notes);
        Assert.True(renderer.GeneratedFrames > 0);
        Assert.True(renderer.SystemMessages >= 3);
        Assert.Equal(0, session.LastServiceError);
        source.Restart();
        Assert.False(source.CheckDone());
    }

    [Fact]
    public void RepeatingSessionCanPauseAndResumeWithoutConsumingEvents()
    {
        using var source = Source();
        var renderer = new Renderer();
        var device = new VmMusicFmSessionDevice(renderer, 1000, 1);
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, repeatSong: true));
        var output = new float[100];
        for (var i = 0; i < 40; i++) { device.ServiceStream(output); session.Update(); }
        Assert.True(renderer.Notes.Count > 300);
        Assert.True(session.IsPlaying());
        session.Startup.Lifecycle.Pause();
        var count = renderer.Notes.Count;
        var frames = renderer.GeneratedFrames;
        Assert.True(device.ServiceStream(output));
        Assert.All(output, sample => Assert.Equal(0, sample));
        Assert.Equal(count, renderer.Notes.Count);
        Assert.Equal(frames, renderer.GeneratedFrames);
        session.Startup.Lifecycle.Resume();
        Assert.True(device.ServiceStream(output));
        Assert.True(renderer.Notes.Count > count);
        session.Dispose();
        Assert.False(device.IsOpen);
    }

    [Fact]
    public void RendererResetFailureLeavesDeviceClosedAndCanBeRetried()
    {
        var renderer = new Renderer();
        var failure = new InvalidOperationException("reset failed");
        renderer.ResetFailure = failure;
        var device = new VmMusicFmSessionDevice(renderer, 1000, 1);
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => device.Open()));
        Assert.False(device.IsOpen);
        renderer.ResetFailure = null;
        Assert.Equal(0, device.Open());
        Assert.Equal(2, renderer.Resets);
        device.Close();
        device.Open();
        Assert.Equal(3, renderer.Resets);
        Assert.Throws<ArgumentException>(() => device.ServiceStream(new float[1]));
        device.Close();
    }

    private sealed class Renderer : IVmMusicFmRenderer
    {
        public List<int> Notes { get; } = [];
        public int Resets { get; private set; }
        public int GeneratedFrames { get; private set; }
        public int SystemMessages { get; private set; }
        public Exception? ResetFailure { get; set; }
        public void ResetState() { Resets++; if (ResetFailure is not null) throw ResetFailure; }
        public int Generate(Span<float> samples) { GeneratedFrames += samples.Length / 2; samples.Fill(1); return samples.Length; }
        public void NoteOn(int channel, int note, int velocity) => Notes.Add(note);
        public void NoteOff(int channel, int note) { }
        public void NoteAfterTouch(int channel, int note, int pressure) { }
        public void ControllerChange(int channel, int controller, int value) { }
        public void PatchChange(int channel, int program) { }
        public void ChannelAfterTouch(int channel, int pressure) { }
        public void PitchBend(int channel, int mostSignificant, int leastSignificant) { }
        public void SystemExclusive(ReadOnlySpan<byte> data) => SystemMessages++;
    }
}
