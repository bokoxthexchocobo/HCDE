namespace HCDE.Client.Tests;

public class VmMusicWildMidiSampleIncrementTests
{
    [Fact]
    public void PitchBendCanComputeVoiceIncrementWithoutHostCallback()
    {
        var channel = new VmMusicWildMidiChannelState { PitchRange = 200 };
        var voice = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, IncrementDivisor = 100 };
        VmMusicWildMidiPitchBend.Apply(channel, [voice], 1, 16383, 44100);
        Assert.Equal(VmMusicWildMidiSampleIncrement.Compute(voice.NoteId, 0, 200, 44100, 100), voice.SampleIncrement);
    }

    [Fact]
    public void PatchOverrideChannelMaskAndOctaveScalingMatchNativeIntegerOrder()
    {
        var middle = VmMusicWildMidiSampleIncrement.Compute(60, 0, 0, 44100, 100);
        Assert.Equal(62208UL, middle);
        Assert.Equal(middle, VmMusicWildMidiSampleIncrement.Compute(0x93c, 0, 0, 44100, 100));
        Assert.Equal(middle, VmMusicWildMidiSampleIncrement.Compute(1, 60, 0, 44100, 100));
        Assert.True(VmMusicWildMidiSampleIncrement.Compute(72, 0, 0, 44100, 100) > middle);
    }

    [Fact]
    public void PitchClampsWithoutOverflowAndCentAdjustmentChangesFrequency()
    {
        Assert.Equal(VmMusicWildMidiSampleIncrement.Compute(0, 0, 0, 44100, 100),
            VmMusicWildMidiSampleIncrement.Compute(60, 0, long.MinValue, 44100, 100));
        Assert.Equal(VmMusicWildMidiSampleIncrement.Compute(127, 0, 0, 44100, 100),
            VmMusicWildMidiSampleIncrement.Compute(60, 0, long.MaxValue, 44100, 100));
        Assert.True(VmMusicWildMidiSampleIncrement.Compute(60, 0, 100, 44100, 100)
            > VmMusicWildMidiSampleIncrement.Compute(60, 0, 0, 44100, 100));
    }

    [Fact]
    public void InvalidDivisorsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiSampleIncrement.Compute(60, 0, 0, 0, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiSampleIncrement.Compute(60, 0, 0, 1, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiSampleIncrement.Compute(60, 0, 0, 44100, 0));
    }
}
