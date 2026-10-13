namespace HCDE.Client.Tests;

public class VmMusicWildMidiAftertouchTests
{
    [Fact]
    public void ChannelPressureUpdatesMatchingVoicesAndReplayEvenWhenInactive()
    {
        var replay = new VmMusicWildMidiNoteReleaseState();
        var first = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Replay = replay };
        var other = new VmMusicWildMidiNoteReleaseState { NoteId = 0x23c, Velocity = 1 };
        var last = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13d, Active = true };
        var calls = new List<VmMusicWildMidiNoteReleaseState>();
        VmMusicWildMidiAftertouch.ApplyChannelPressure([first, other, last], 1, 300, note =>
            { Assert.Equal(44, note.Velocity); calls.Add(note); });
        Assert.Equal(new[] { first, replay, last }, calls);
        Assert.Equal(1, other.Velocity);
        Assert.False(first.Active);
    }

    [Fact]
    public void ChannelPressureFailureStopsBeforeLaterVoiceMutation()
    {
        var first = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c };
        var last = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13d, Velocity = 1 };
        var error = new IOException("adjust failed");
        Assert.Same(error, Assert.Throws<IOException>(() => VmMusicWildMidiAftertouch.ApplyChannelPressure(
            [first, last], 1, 100, _ => throw error)));
        Assert.Equal(100, first.Velocity); Assert.Equal(1, last.Velocity);
    }

    [Fact]
    public void InvalidChannelPressureChannelIsRejectedBeforeMutation()
    {
        var voice = new VmMusicWildMidiNoteReleaseState { Velocity = 1 };
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiAftertouch.ApplyChannelPressure(
            [voice], -1, 100, _ => Assert.Fail("unexpected adjustment")));
        Assert.Equal(1, voice.Velocity);
    }

    [Theory]
    [InlineData(true, 300, 44)]
    [InlineData(false, -1, 255)]
    public void SelectedActiveVoiceAndReplayReceiveBytePressureInOrder(bool primaryActive, int pressure, byte expected)
    {
        var replay = new VmMusicWildMidiNoteReleaseState();
        var primary = new VmMusicWildMidiNoteReleaseState { Active = primaryActive, Velocity = 1 };
        var secondary = new VmMusicWildMidiNoteReleaseState { Active = true, Velocity = 2 };
        var selected = primaryActive ? primary : secondary;
        selected.Replay = replay;
        var calls = new List<VmMusicWildMidiNoteReleaseState>();
        Assert.True(VmMusicWildMidiAftertouch.Apply(primary, secondary, pressure, note =>
        { Assert.Equal(expected, note.Velocity); calls.Add(note); }));
        Assert.Equal(new[] { selected, replay }, calls);
        Assert.Equal(primaryActive ? 2 : 1, (primaryActive ? secondary : primary).Velocity);
        Assert.False(replay.Active);
    }

    [Fact]
    public void InactiveVoicesAreUnchangedAndDoNotAdjustVolume()
    {
        var primary = new VmMusicWildMidiNoteReleaseState { Velocity = 1 };
        var secondary = new VmMusicWildMidiNoteReleaseState { Velocity = 2 };
        Assert.False(VmMusicWildMidiAftertouch.Apply(primary, secondary, 127, _ => Assert.Fail("unexpected adjustment")));
        Assert.Equal(1, primary.Velocity);
        Assert.Equal(2, secondary.Velocity);
    }

    [Fact]
    public void VolumeAdjustmentFailurePropagatesBeforeReplayMutation()
    {
        var replay = new VmMusicWildMidiNoteReleaseState { Velocity = 2 };
        var primary = new VmMusicWildMidiNoteReleaseState { Active = true, Replay = replay };
        var error = new IOException("volume adjustment failed");
        Assert.Same(error, Assert.Throws<IOException>(() => VmMusicWildMidiAftertouch.Apply(primary,
            new(), 100, _ => throw error)));
        Assert.Equal(100, primary.Velocity);
        Assert.Equal(2, replay.Velocity);
    }
}
