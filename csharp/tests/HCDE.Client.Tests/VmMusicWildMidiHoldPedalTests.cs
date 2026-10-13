namespace HCDE.Client.Tests;

public class VmMusicWildMidiHoldPedalTests
{
    [Theory]
    [InlineData(64, 1)]
    [InlineData(63, 0)]
    public void PedalThresholdUpdatesChannelWithoutApplyingHoldToExistingVoices(int value, byte expected)
    {
        var channel = new VmMusicWildMidiChannelState();
        var voice = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c };
        VmMusicWildMidiNoteRelease.HoldPedal(channel, [voice], 1, value);
        Assert.Equal(expected, channel.Hold);
        Assert.Equal(0, voice.Hold);
    }

    [Theory]
    [InlineData(96, 4)]
    [InlineData(224, 5)]
    public void PedalReleaseUsesClampedPriorityAndIgnoresSustainStage(int modes, int stage)
    {
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            NoteId = 0x13c, Modes = modes, Hold = 3, EnvelopeLevel = 20, IsOff = true,
            EnvelopeTargets = [0, 0, 0, 0, 10, 10], EnvelopeRates = [0, 0, 0, 0, 4, 5]
        };
        VmMusicWildMidiNoteRelease.HoldPedal(new(), [voice], 1, 0);
        Assert.Equal(stage, voice.Envelope); Assert.Equal(-stage, voice.EnvelopeIncrement);
        Assert.Equal(0, voice.Hold); Assert.True(voice.IsOff);
    }

    [Fact]
    public void ReleaseClearsMatchingHoldAndOnlyStopsNonEnvelopeLoopsMarkedOff()
    {
        var pending = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Modes = 4, Hold = 3, EnvelopeIncrement = 9 };
        var held = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13d, Modes = 4, Hold = 1, EnvelopeIncrement = 8 };
        var other = new VmMusicWildMidiNoteReleaseState { NoteId = 0x23c, Hold = 3 };
        VmMusicWildMidiNoteRelease.HoldPedal(new(), [pending, held, other], 1, 0);
        Assert.Equal(0, pending.Modes); Assert.Equal(0, pending.EnvelopeIncrement); Assert.Equal(0, pending.Hold);
        Assert.Equal(4, held.Modes); Assert.Equal(8, held.EnvelopeIncrement); Assert.Equal(0, held.Hold);
        Assert.Equal(3, other.Hold);
    }
}
