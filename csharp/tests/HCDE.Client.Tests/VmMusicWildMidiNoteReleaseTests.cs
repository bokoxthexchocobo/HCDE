namespace HCDE.Client.Tests;

public class VmMusicWildMidiNoteReleaseTests
{
    [Theory]
    [InlineData(32, 3)]
    [InlineData(128, 5)]
    [InlineData(0, 4)]
    [InlineData(160, 3)]
    public void EnvelopeReleaseSelectsNativeStageAndDirection(int modes, int stage)
    {
        var note = new VmMusicWildMidiNoteReleaseState { Modes = modes | 64, EnvelopeLevel = 20, IsOff = true };
        VmMusicWildMidiNoteRelease.Release(note, [0, 0, 0, 10, 10, 10], [0, 0, 0, 3, 4, 5]);
        Assert.Equal(stage, note.Envelope);
        Assert.Equal(-stage, note.EnvelopeIncrement);
        Assert.False(note.IsOff);
    }

    [Fact]
    public void HeldEnvelopeDefersReleaseAndNonEnvelopeClearsOnlyLoop()
    {
        var held = new VmMusicWildMidiNoteReleaseState { Modes = 64, Hold = 1, EnvelopeIncrement = 7 };
        VmMusicWildMidiNoteRelease.Release(held, [], []);
        Assert.Equal(3, held.Hold);
        Assert.Equal(7, held.EnvelopeIncrement);
        var plain = new VmMusicWildMidiNoteReleaseState { Modes = 132, EnvelopeIncrement = 7 };
        VmMusicWildMidiNoteRelease.Release(plain, [], []);
        Assert.Equal(128, plain.Modes);
        Assert.Equal(0, plain.EnvelopeIncrement);
    }

    [Fact]
    public void EqualityUsesPositiveRateAndAlreadyReleasedStageDoesNotRegress()
    {
        var note = new VmMusicWildMidiNoteReleaseState { Modes = 64, EnvelopeLevel = 10 };
        VmMusicWildMidiNoteRelease.Release(note, [0, 0, 0, 0, 10], [0, 0, 0, 0, 4]);
        Assert.Equal(4, note.EnvelopeIncrement);
        note.Envelope = 6;
        VmMusicWildMidiNoteRelease.Release(note, [], []);
        Assert.Equal(6, note.Envelope);
        Assert.Equal(4, note.EnvelopeIncrement);
    }
}
