using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicSynthStreamPlaybackTests
{
    [Fact]
    public void ClosedDeviceStatisticsUseNativeMessageBeforeHostCleanup()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(120, 0x3c80)]));
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var device = new VmMusicWildMidiSessionDevice(renderer);
        using var playback = VmMusicSynthStreamPlayback.Start(source, VmMusicMidiDeviceKind.WildMidi,
            -6, false, _ => device, new(600000), (_, fill) => new Output(fill));
        device.Close();
        Assert.Equal("No MIDI device in use.", playback.GetStatistics());
        playback.Update();
        Assert.Throws<ObjectDisposedException>(() => playback.GetStatistics());
    }

    [Theory]
    [InlineData(false, float.NaN)]
    [InlineData(true, float.NaN)]
    [InlineData(false, -1f)]
    [InlineData(true, -1f)]
    public void InvalidResolvedGainFailsBeforeDeviceAllocation(bool explicitVolume, float gain)
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, []));
        var allocated = false;
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicSynthStreamPlayback.StartConfigured(source,
            new(6, "bank", 0, explicitVolume ? gain : null), -6, false,
            (_, _) => { allocated = true; throw new InvalidOperationException(); }, new(600000),
            (_, _) => null, (_, _) => gain));
        Assert.False(allocated);
    }

    [Fact]
    public void ReplayGainLookupFailurePropagatesBeforeDeviceAllocation()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, []));
        var allocated = false;
        var failure = new InvalidDataException("replay gain");
        Assert.Same(failure, Assert.Throws<InvalidDataException>(() => VmMusicSynthStreamPlayback.StartConfigured(source,
            new(6, "bank", 0, null), -6, false,
            (_, _) => { allocated = true; throw new InvalidOperationException(); }, new(600000),
            (_, _) => null, (_, _) => throw failure)));
        Assert.False(allocated);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(-1f)]
    public void InvalidInitialGainFailsBeforeAllocatingDevice(float gain)
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, []));
        var allocated = false;
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicSynthStreamPlayback.Start(source,
            VmMusicMidiDeviceKind.WildMidi, -6, false,
            _ => { allocated = true; throw new InvalidOperationException(); }, new(600000), (_, _) => null, gain));
        Assert.False(allocated);
    }

    private sealed class Output(VmMusicFloatFill fill) : IVmMusicOutputStream
    {
        public int Disposals;
        public bool Paused;
        public bool FailStop;
        public bool Fill(float[] samples) => fill(samples);
        public void Play(bool loop, float volume) => Assert.True(fill(new float[2]));
        public void SetPaused(bool paused) => Paused = paused;
        public void Stop() { if (FailStop) throw new InvalidOperationException("stop"); }
        public void Dispose() => Disposals++;
    }

    [Fact]
    public void OutputCallbackDrainsSessionAndHostUpdateReleasesOwnedRenderer()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(120, 0x3c80)]));
        var renderer = new VmMusicWildMidiManagedRenderer(22050, _ => null, _ => 0);
        var router = VmMusicSynthDeviceFactory.CreateRouter(null, () => renderer);
        Output? output = null;
        using var playback = VmMusicSynthStreamPlayback.Start(source, VmMusicMidiDeviceKind.WildMidi, -6, false,
            kind => Assert.IsAssignableFrom<IVmMusicAudioSessionDevice>(router(kind)), new(600000),
            (format, fill) => { Assert.Equal(22050, format.SampleRate); return output = new(fill); });
        playback.Pause(true);
        Assert.True(playback.ChangeInteger("wildmidi.reverb", 1));
        Assert.False(playback.ChangeInteger("unknown", 1));
        Assert.False(playback.ChangeNumber("libadl.gain", 0.5));
        Assert.False(playback.ChangeString("libadl.custombank", "bank"));
        playback.OutputVolume = 0.5f;
        Assert.Throws<ArgumentOutOfRangeException>(() => playback.OutputVolume = float.NaN);
        Assert.True(output!.Paused);
        var samples = Enumerable.Repeat(1f, 2048).ToArray();
        Assert.True(output.Fill(samples));
        Assert.All(samples, value => Assert.Equal(0f, value));
        playback.Pause(false);
        for (var i = 0; i < 100 && output.Fill(samples); i++) { }
        Assert.False(output.Fill(samples));
        playback.Update();
        Assert.Equal(1, output.Disposals);
        Assert.Throws<ObjectDisposedException>(() => renderer.GetVoiceCount());
        Assert.Throws<ObjectDisposedException>(() => playback.ChangeInteger("wildmidi.reverb", 0));
        Assert.Throws<ObjectDisposedException>(() => playback.OutputVolume = 1);
    }

    [Fact]
    public void MissingOutputBackendReleasesStartedSession()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, []));
        var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var router = VmMusicSynthDeviceFactory.CreateRouter(null, () => renderer);
        Assert.Throws<VmMusicMidiDeviceUnavailableException>(() => VmMusicSynthStreamPlayback.Start(source,
            VmMusicMidiDeviceKind.WildMidi, -6, false,
            kind => Assert.IsAssignableFrom<IVmMusicAudioSessionDevice>(router(kind)), new(600000), (_, _) => null));
        Assert.Throws<ObjectDisposedException>(() => renderer.GetVoiceCount());
    }

    [Fact]
    public void FailedCallbackClearsOutputAndCannotResumeBeforeHostCleanup()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(120, 0x3c80)]));
        var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var router = VmMusicSynthDeviceFactory.CreateRouter(null, () => renderer);
        Output? output = null;
        using var playback = VmMusicSynthStreamPlayback.Start(source, VmMusicMidiDeviceKind.WildMidi, -6, false,
            kind => Assert.IsAssignableFrom<IVmMusicAudioSessionDevice>(router(kind)), new(600000),
            (_, fill) => output = new(fill));
        float[] malformed = [1, 2, 3];
        Assert.Throws<ArgumentException>(() => output!.Fill(malformed));
        Assert.Equal(new float[3], malformed);
        float[] retry = [1, 1];
        Assert.False(output!.Fill(retry));
        Assert.Equal(new float[2], retry);
        playback.Update();
        Assert.Equal(1, output.Disposals);
        Assert.Throws<ObjectDisposedException>(() => renderer.GetVoiceCount());
    }

    [Fact]
    public void OutputStopFailureStillDisposesStreamAndClearsOwnership()
    {
        var stream = new Output(_ => true) { FailStop = true };
        var output = new VmMusicOutputStream();
        output.Create(true, () => new(8, 44100, 2, true), _ => stream);
        Assert.Throws<InvalidOperationException>(() => output.Stop());
        Assert.Null(output.Stream);
        Assert.Equal(1, stream.Disposals);
        output.Stop();
        Assert.Equal(1, stream.Disposals);
    }
}
