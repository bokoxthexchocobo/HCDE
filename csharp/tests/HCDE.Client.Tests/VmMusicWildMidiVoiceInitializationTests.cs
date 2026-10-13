using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiVoiceInitializationTests
{
    private static VmMusicWildMidiGusPatchSample Sample() => new(
        new(4, 0, 0, 22050, 0, 0, 440000, 20432, 64, 0),
        VmMusicWildMidiPcmDecoder.Decode([1, 2, 3, 4], 0, 0, 64),
        new byte[] { 63, 63, 63, 63, 63, 63 }, new byte[] { 255, 255, 255, 0, 0, 0 });

    private static VmMusicWildMidiChannelState Channel() => new()
    {
        Volume = 100, Expression = 127, Pan = 64, Balance = 64, Hold = 1
    };

    [Fact]
    public void CreatesNativeInitialEnvelopePitchAndVolumeState()
    {
        var channel = Channel();
        var result = VmMusicWildMidiVoiceInitialization.Create(Sample(), channel, 3, 60, 100, 0, 44100, 1024, false);
        var voice = result.Voice;
        Assert.Equal(0x33c, voice.NoteId);
        Assert.Equal(100, voice.Velocity);
        Assert.Equal(0U, voice.SamplePosition);
        Assert.Equal(0, voice.Envelope);
        Assert.Equal(0, voice.EnvelopeLevel);
        Assert.Equal(65322, voice.EnvelopeIncrement);
        Assert.Equal(64, voice.Modes);
        Assert.Equal(1, voice.Hold);
        Assert.False(voice.Active);
        Assert.False(voice.IsOff);
        Assert.Null(voice.Replay);
        Assert.Equal(VmMusicWildMidiNoteVolume.Compute(channel, 100, 1024, false), voice.MixVolume);
        Assert.Equal(VmMusicWildMidiSampleIncrement.Compute(0x33c, 0, 0, 44100, 20432), voice.SampleIncrement);
        Assert.Equal(4096U, result.Sample.Bounds.DataLength);
    }

    [Fact]
    public void PatchPitchAndChannelBendFlowIntoIncrement()
    {
        var channel = Channel();
        channel.PitchAdjust = 100;
        var result = VmMusicWildMidiVoiceInitialization.Create(Sample(), channel, 9, 35, 127, 60, 44100, 1024, true);
        Assert.Equal(VmMusicWildMidiSampleIncrement.Compute(0x923, 60, 100, 44100, 20432), result.Voice.SampleIncrement);
        Assert.Equal(60, result.Voice.PatchNote);
    }

    [Fact]
    public void PreparedVoiceCanEnterMixerWithIndependentEnvelopeStorage()
    {
        var sample = Sample();
        var first = VmMusicWildMidiVoiceInitialization.Create(sample, Channel(), 0, 60, 100, 0, 44100, 1024, false);
        var second = VmMusicWildMidiVoiceInitialization.Create(sample, Channel(), 0, 61, 100, 0, 44100, 1024, false);
        first.Voice.EnvelopeTargets[0] = 123;
        Assert.Equal(4194240, second.Voice.EnvelopeTargets[0]);
        var output = new int[2];
        VmMusicWildMidiLinearMixer.Mix(new List<VmMusicWildMidiNoteReleaseState> { second.Voice }, output, _ => second.Sample);
        Assert.Equal(new[] { 0, 0 }, output);
        Assert.True(second.Voice.SamplePosition > 0);
        Assert.Equal(65322, second.Voice.EnvelopeLevel);
    }

    [Fact]
    public void ZeroVelocityAndInvalidMidiRangesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiVoiceInitialization.Create(Sample(), Channel(), 0, 60, 0, 0, 44100, 1024, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiVoiceInitialization.Create(Sample(), Channel(), 16, 60, 100, 0, 44100, 1024, false));
    }
}
