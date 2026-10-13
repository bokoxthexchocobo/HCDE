namespace HCDE.Client.Tests;

public class VmMusicStreamPlaybackTests
{
    private sealed class Output(VmMusicFloatFill fill) : IVmMusicOutputStream
    {
        public float[] Samples { get; } = new float[2];
        public void Play(bool loop, float volume) => Assert.True(fill(Samples));
        public void SetPaused(bool paused) { }
        public void Stop() { }
        public void Dispose() { }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreatedOutputCanImmediatelyFillUsingInputFormatAndCurrentGain(bool inputFloat)
    {
        var playback = new VmMusicStreamPlayback { MusicVolume = .5f };
        VmMusicFloatFill? callback = null;
        Output? stream = null;
        playback.Create(true, () => new(8, 44100, 2, inputFloat), (format, fill) =>
        {
            Assert.True(format.IsFloat);
            Assert.Equal(inputFloat ? 8 : 16, format.BufferBytes);
            callback = fill;
            return stream = new Output(fill);
        }, samples =>
        {
            Assert.True(inputFloat);
            samples.Fill(1);
            return true;
        }, samples =>
        {
            Assert.False(inputFloat);
            samples.Fill(16384);
            return true;
        });
        Assert.All(stream!.Samples, value => Assert.Equal(inputFloat ? .5f : .25f, value));
        playback.MusicVolume = 2;
        Assert.True(callback!(stream.Samples));
        Assert.All(stream.Samples, value => Assert.Equal(inputFloat ? 2 : 1, value));
        playback.Stop();
        Assert.Null(playback.Stream);
    }

    [Fact]
    public void RegisteredCallbackClearsOutputWhenBackendEnds()
    {
        var playback = new VmMusicStreamPlayback();
        VmMusicFloatFill? callback = null;
        playback.Create(true, () => new(8, 44100, 2, true), (_, fill) => { callback = fill; return null; },
            samples => { samples[0] = 3; return false; }, _ => throw new InvalidOperationException());
        float[] destination = [9, 9];
        Assert.False(callback!(destination));
        Assert.Equal(new float[2], destination);
    }
}
