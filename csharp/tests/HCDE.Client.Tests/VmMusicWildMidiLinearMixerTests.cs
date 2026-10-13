using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiLinearMixerTests
{
    private static VmMusicWildMidiNoteReleaseState Voice(uint position = 0) => new()
    {
        Active = true, SamplePosition = position, SampleIncrement = 1024,
        EnvelopeLevel = 1024 << 12, MixVolume = new(1024, 512)
    };

    [Theory]
    [InlineData(0, 100, 50)]
    [InlineData(512, 200, 100)]
    [InlineData(1023, 299, 149)]
    public void FractionalInterpolationAndStereoScaling(uint position, int left, int right)
    {
        Assert.Equal(new(left, right), VmMusicWildMidiLinearMixer.Interpolate([100, 300], Voice(position)));
    }

    [Fact]
    public void NegativeInterpolationTruncatesTowardZero()
    {
        Assert.Equal(new(-101, -50), VmMusicWildMidiLinearMixer.Interpolate([-101, -100], Voice(512)));
    }

    [Fact]
    public void ExhaustionMixesCurrentVoiceAndReplayInSameFrame()
    {
        var old = Voice();
        var replay = Voice();
        old.Replay = replay;
        var voices = new List<VmMusicWildMidiNoteReleaseState> { old };
        var output = new int[4];
        VmMusicWildMidiLinearMixer.Mix(voices, output, voice => ReferenceEquals(voice, old)
            ? new(new short[] { 100, 100 }, new(1024, 0, 0, 0))
            : new(new short[] { 200, 300, 0 }, new(2048, 0, 0, 0)));
        Assert.Equal(new[] { 300, 150, 300, 150 }, output);
        Assert.False(old.Active);
        Assert.False(replay.Active);
        Assert.Empty(voices);
    }

    [Fact]
    public void LoopingVoiceUsesWrappedPositionOnFollowingFrame()
    {
        var voice = Voice(1024);
        voice.Modes = VmMusicWildMidiNoteRelease.Loop;
        var output = new int[6];
        VmMusicWildMidiLinearMixer.Mix(new List<VmMusicWildMidiNoteReleaseState> { voice }, output,
            _ => new(new short[] { 100, 200, 300, 0 }, new(3072, 1024, 2048, 1024)));
        Assert.Equal(new[] { 200, 100, 300, 150, 200, 100 }, output);
    }

    [Fact]
    public void EmptyVoiceListWritesSilence()
    {
        var output = new[] { 1, 2, 3, 4 };
        VmMusicWildMidiLinearMixer.Mix(new List<VmMusicWildMidiNoteReleaseState>(), output,
            _ => throw new InvalidOperationException());
        Assert.Equal(new int[4], output);
    }

    [Fact]
    public void MissingInterpolationGuardSampleAndPartialFramesFailExplicitly()
    {
        var voice = Voice();
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiLinearMixer.Interpolate(new short[] { 100 }, voice));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiLinearMixer.Mix(
            new List<VmMusicWildMidiNoteReleaseState> { voice }, new int[1], _ => default));
        Assert.Equal(0U, voice.SamplePosition);
    }
}
