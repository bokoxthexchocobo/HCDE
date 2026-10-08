namespace HCDE.Client.Tests;

public class AudioChannelContinuationTests
{
    [Fact]
    public void PartialBuffersContinueAtNextSample()
    {
        var mixer = new AudioMixer();
        mixer.Play(new short[] { 1, 2, 3, 4, 5 });
        Assert.Equal(new short[] { 1, 2 }, mixer.Mix(2));
        Assert.Equal(new short[] { 3, 4 }, mixer.Mix(2));
        Assert.Equal(new short[] { 5, 0 }, mixer.Mix(2));
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
    }

    [Fact]
    public void ZeroLengthMixDoesNotConsumeChannels()
    {
        var mixer = new AudioMixer(); mixer.Play(new short[] { 7 });
        Assert.Empty(mixer.Mix(0)); Assert.Equal(new short[] { 7 }, mixer.Mix(1));
    }

    [Fact]
    public void MutedBuffersAdvanceTimeWithoutDiscardingRemainingSound()
    {
        var mixer = new AudioMixer { Muted = true };
        mixer.Play(new short[] { 1, 2, 3, 4 });
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
        mixer.Muted = false; Assert.Equal(new short[] { 3, 4 }, mixer.Mix(2));
    }

    [Fact]
    public void NewChannelsBeginAtCurrentBufferWhileOlderChannelsContinue()
    {
        var mixer = new AudioMixer(); mixer.Play(new short[] { 1, 2, 3 });
        Assert.Equal(new short[] { 1 }, mixer.Mix(1));
        mixer.Play(new short[] { 10 });
        Assert.Equal(new short[] { 12, 3 }, mixer.Mix(2));
        Assert.Equal(new short[] { 0 }, mixer.Mix(1));
    }

    [Fact]
    public void InvalidMixDoesNotAdvanceChannels()
    {
        var mixer = new AudioMixer(); mixer.Play(new short[] { 9 });
        Assert.Throws<ArgumentOutOfRangeException>(() => mixer.Mix(-1));
        Assert.Equal(new short[] { 9 }, mixer.Mix(1));
    }
}
