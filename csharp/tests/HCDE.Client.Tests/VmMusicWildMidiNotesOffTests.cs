namespace HCDE.Client.Tests;

public class VmMusicWildMidiNotesOffTests
{
    [Theory]
    [InlineData(20, -3)]
    [InlineData(10, 3)]
    public void NotesOffForcesStageFiveEvenWithSustainAndDoesNotRequireActive(int level, int increment)
    {
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            NoteId = 0x13c, Modes = 96, EnvelopeLevel = level, IsOff = true,
            EnvelopeTargets = [0, 0, 0, 0, 0, 10], EnvelopeRates = [0, 0, 0, 0, 0, 3]
        };
        VmMusicWildMidiNoteRelease.NotesOff([voice], 1, false);
        Assert.Equal(5, voice.Envelope); Assert.Equal(increment, voice.EnvelopeIncrement);
        Assert.False(voice.Active); Assert.True(voice.IsOff); Assert.Equal(96, voice.Modes);
    }

    [Fact]
    public void HeldVoiceIsMarkedWhileNonEnvelopeAndOtherChannelRemainUnchanged()
    {
        var held = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Hold = 1 };
        var plain = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13d, Modes = 4, EnvelopeIncrement = 8 };
        var other = new VmMusicWildMidiNoteReleaseState { NoteId = 0x23c, Hold = 1 };
        VmMusicWildMidiNoteRelease.NotesOff([held, plain, other], 1, false);
        Assert.Equal(3, held.Hold); Assert.Equal(1, other.Hold);
        Assert.Equal(4, plain.Modes); Assert.Equal(8, plain.EnvelopeIncrement);
    }

    [Fact]
    public void PercussionAndCompletedStagesDoNotAccessEnvelopeArrays()
    {
        var voice = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Modes = 64 };
        VmMusicWildMidiNoteRelease.NotesOff([voice], 1, true);
        Assert.Equal(0, voice.Envelope);
        voice.Envelope = 6;
        VmMusicWildMidiNoteRelease.NotesOff([voice], 1, false);
        Assert.Equal(6, voice.Envelope);
    }
}
