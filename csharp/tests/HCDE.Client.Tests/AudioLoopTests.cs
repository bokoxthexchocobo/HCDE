namespace HCDE.Client.Tests;

public class AudioLoopTests
{
    [Fact]
    public void LoopWrapsAcrossAndWithinBuffersUntilStopped()
    {
        var mixer = new AudioMixer(); var channel = mixer.PlayChannel(new short[] { 1, 2, 3 }, loop: true);
        Assert.Equal(new short[] { 1, 2, 3, 1 }, mixer.Mix(4));
        Assert.Equal(new short[] { 2, 3, 1, 2, 3 }, mixer.Mix(5));
        Assert.True(mixer.IsPlaying(channel)); Assert.True(mixer.StopChannel(channel));
        Assert.Equal(new short[] { 0 }, mixer.Mix(1));
    }

    [Fact]
    public void MutedLoopAdvancesAndZeroBufferDoesNot()
    {
        var mixer = new AudioMixer { Muted = true }; mixer.PlayChannel(new short[] { 1, 2, 3 }, true);
        Assert.Equal(new short[] { 0, 0, 0, 0 }, mixer.Mix(4));
        Assert.Empty(mixer.Mix(0)); mixer.Muted = false;
        Assert.Equal(new short[] { 2, 3 }, mixer.Mix(2));
    }

    [Fact]
    public void OneShotCompletesWhileLoopContinues()
    {
        var mixer = new AudioMixer(); var loop = mixer.PlayChannel(new short[] { 10 }, true);
        var once = mixer.PlayChannel(new short[] { 1, 2 });
        Assert.Equal(new short[] { 11, 12, 10 }, mixer.Mix(3));
        Assert.True(mixer.IsPlaying(loop)); Assert.False(mixer.IsPlaying(once));
        mixer.StopAllChannels(); Assert.False(mixer.IsPlaying(loop));
    }

    [Fact]
    public void EmptyLoopDoesNotCreateChannel()
    {
        var mixer = new AudioMixer(); Assert.Equal(Guid.Empty, mixer.PlayChannel([], true));
        Assert.Equal(new short[] { 0 }, mixer.Mix(1));
    }
}
