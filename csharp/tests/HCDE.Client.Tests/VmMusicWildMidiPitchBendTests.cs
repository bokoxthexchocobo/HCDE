namespace HCDE.Client.Tests;

public class VmMusicWildMidiPitchBendTests
{
    [Theory]
    [InlineData(0, -8192, -200)]
    [InlineData(8192, 0, 0)]
    [InlineData(16383, 8191, 200)]
    [InlineData(8193, 1, 0)]
    [InlineData(8191, -1, 0)]
    public void BendUsesAsymmetricDivisorsAndUpdatesMatchingVoicesOnly(int data, short pitch, long adjustment)
    {
        var channel = new VmMusicWildMidiChannelState { PitchRange = 200 };
        var replay = new VmMusicWildMidiNoteReleaseState { SampleIncrement = 9 };
        var matching = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Replay = replay };
        var other = new VmMusicWildMidiNoteReleaseState { NoteId = 0x23c, SampleIncrement = 8 };
        VmMusicWildMidiPitchBend.Apply(channel, [matching, other], 1, data, voice =>
        {
            Assert.Same(matching, voice); Assert.Equal(pitch, channel.Pitch);
            Assert.Equal(adjustment, channel.PitchAdjust); return 123;
        });
        Assert.Equal(123UL, matching.SampleIncrement);
        Assert.Equal(8UL, other.SampleIncrement);
        Assert.Equal(9UL, replay.SampleIncrement);
        Assert.False(matching.Active);
    }

    [Fact]
    public void IncrementFailurePreservesPreviousIncrementAndStopsLaterUpdates()
    {
        var channel = new VmMusicWildMidiChannelState { PitchRange = 200 };
        var voice = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, SampleIncrement = 8 };
        var error = new IOException("increment failure");
        Assert.Same(error, Assert.Throws<IOException>(() =>
            VmMusicWildMidiPitchBend.Apply(channel, [voice], 1, 16383, _ => throw error)));
        Assert.Equal(200, channel.PitchAdjust);
        Assert.Equal(8UL, voice.SampleIncrement);
    }
}
