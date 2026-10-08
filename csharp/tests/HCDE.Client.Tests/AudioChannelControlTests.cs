namespace HCDE.Client.Tests;

public class AudioChannelControlTests
{
    [Fact]
    public void StoppingOneChannelRetainsOtherSoundAndCursor()
    {
        var mixer = new AudioMixer();
        var first = mixer.PlayChannel(new short[] { 1, 2, 3 });
        var second = mixer.PlayChannel(new short[] { 10, 20, 30 });
        Assert.Equal(new short[] { 11 }, mixer.Mix(1));
        Assert.True(mixer.StopChannel(first)); Assert.False(mixer.IsPlaying(first));
        Assert.True(mixer.IsPlaying(second)); Assert.Equal(new short[] { 20, 30 }, mixer.Mix(2));
        Assert.False(mixer.IsPlaying(second));
    }

    [Fact]
    public void CompletedAndStoppedHandlesCannotStopNewChannels()
    {
        var mixer = new AudioMixer(); var old = mixer.PlayChannel(new short[] { 1 });
        mixer.Mix(1);
        var current = mixer.PlayChannel(new short[] { 2 });
        Assert.NotEqual(old, current); Assert.False(mixer.StopChannel(old));
        Assert.True(mixer.IsPlaying(current)); Assert.True(mixer.StopChannel(current));
        Assert.False(mixer.StopChannel(current));
    }

    [Fact]
    public void StopAllClearsMutedAndPartiallyPlayedChannels()
    {
        var mixer = new AudioMixer { Muted = true };
        var channel = mixer.PlayChannel(new short[] { 1, 2, 3 }); mixer.Mix(1);
        mixer.StopAllChannels(); Assert.False(mixer.IsPlaying(channel));
        mixer.Muted = false; Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
    }

    [Fact]
    public void EmptyPcmHasNoChannelAndForeignHandleDoesNotStopAudio()
    {
        var mixer = new AudioMixer(); Assert.Equal(Guid.Empty, mixer.PlayChannel([]));
        Assert.False(mixer.IsPlaying(Guid.Empty)); Assert.False(mixer.StopChannel(Guid.Empty));
        var other = new AudioMixer(); var foreign = other.PlayChannel(new short[] { 1 });
        var own = mixer.PlayChannel(new short[] { 2 });
        Assert.False(mixer.StopChannel(foreign)); Assert.True(mixer.IsPlaying(own));
    }
}
