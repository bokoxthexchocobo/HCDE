namespace HCDE.Client.Tests;

public class VmMusicWildMidiNoteOffTests
{
    [Fact]
    public void SoundOffOnlyDeactivatesMatchingLinkedVoicesAndDetachesReplay()
    {
        var replay = new VmMusicWildMidiNoteReleaseState { Active = true };
        var matching = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Active = true, Replay = replay, Hold = 3 };
        var other = new VmMusicWildMidiNoteReleaseState { NoteId = 0x23c, Active = true, Replay = replay };
        VmMusicWildMidiNoteRelease.SoundOff([matching, other], 1);
        Assert.False(matching.Active); Assert.Null(matching.Replay); Assert.Equal(3, matching.Hold);
        Assert.True(other.Active); Assert.Same(replay, other.Replay);
        Assert.True(replay.Active);
    }

    [Fact]
    public void InvalidSoundOffChannelIsRejectedBeforeVoiceMutation()
    {
        var voice = new VmMusicWildMidiNoteReleaseState { Active = true };
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiNoteRelease.SoundOff([voice], 16));
        Assert.True(voice.Active);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ActivePrimaryWinsOtherwiseSecondaryIsReleased(bool primaryActive)
    {
        var primary = new VmMusicWildMidiNoteReleaseState { Active = primaryActive, Modes = 4, EnvelopeIncrement = 8 };
        var secondary = new VmMusicWildMidiNoteReleaseState { Active = true, Modes = 4, EnvelopeIncrement = 9 };
        Assert.True(VmMusicWildMidiNoteRelease.NoteOff(primary, secondary, false));
        Assert.Equal(primaryActive ? 0 : 8, primary.EnvelopeIncrement);
        Assert.Equal(primaryActive ? 9 : 0, secondary.EnvelopeIncrement);
        Assert.Equal(primaryActive ? 0 : 4, primary.Modes);
        Assert.Equal(primaryActive ? 4 : 0, secondary.Modes);
    }

    [Fact]
    public void InactiveAndNonLoopingPercussionAreIgnored()
    {
        var primary = new VmMusicWildMidiNoteReleaseState { Modes = 64 };
        var secondary = new VmMusicWildMidiNoteReleaseState();
        Assert.False(VmMusicWildMidiNoteRelease.NoteOff(primary, secondary, false));
        primary.Active = true;
        Assert.False(VmMusicWildMidiNoteRelease.NoteOff(primary, secondary, true));
        Assert.False(primary.IsOff);
        primary.Modes |= 4;
        Assert.True(VmMusicWildMidiNoteRelease.NoteOff(primary, secondary, true));
        Assert.True(primary.IsOff);
    }

    [Fact]
    public void InitialEnvelopeDefersReleaseThenUsesSelectedVoiceSampleData()
    {
        var note = new VmMusicWildMidiNoteReleaseState
        {
            Active = true, Modes = 64, EnvelopeLevel = 20, EnvelopeIncrement = 7,
            EnvelopeTargets = [0, 0, 0, 0, 10], EnvelopeRates = [0, 0, 0, 0, 3]
        };
        var other = new VmMusicWildMidiNoteReleaseState();
        Assert.True(VmMusicWildMidiNoteRelease.NoteOff(note, other, false));
        Assert.True(note.IsOff);
        Assert.Equal(0, note.Envelope);
        Assert.Equal(7, note.EnvelopeIncrement);
        note.Envelope = 1;
        Assert.True(VmMusicWildMidiNoteRelease.NoteOff(note, other, false));
        Assert.False(note.IsOff);
        Assert.Equal(4, note.Envelope);
        Assert.Equal(-3, note.EnvelopeIncrement);
    }
}
