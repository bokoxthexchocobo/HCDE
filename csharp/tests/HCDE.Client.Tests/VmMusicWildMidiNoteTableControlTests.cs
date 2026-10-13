using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiNoteTableControlTests
{
    private static VmMusicWildMidiInitializedVoice? Prepare(int channel, byte note, byte velocity) => new(new()
    {
        NoteId = (ushort)(channel << 8 | note), Velocity = velocity, Modes = 64, IncrementDivisor = 100,
        EnvelopeTargets = new int[7], EnvelopeRates = [1, 1, 1, 1, 1, 1, 10]
    }, new(new short[3], new(1024, 0, 0, 0)));

    [Fact]
    public void PitchBendUpdatesMatchingLinkedVoiceButNotQueuedReplay()
    {
        var table = new VmMusicWildMidiNoteTable();
        table.NoteOn(0, 60, 100, false, Prepare);
        table.NoteOn(1, 60, 100, false, Prepare);
        var primary = table.Voices[0];
        primary.Envelope = 4;
        table.NoteOn(0, 60, 100, false, Prepare);
        var state = new VmMusicWildMidiChannelState { PitchRange = 200 };
        table.PitchBend(state, 0, 16383, 44100);
        Assert.Equal(200, state.PitchAdjust);
        Assert.Equal(VmMusicWildMidiSampleIncrement.Compute(60, 0, 200, 44100, 100), primary.SampleIncrement);
        Assert.Equal(0UL, primary.Replay!.SampleIncrement);
        Assert.Equal(0UL, table.Voices[1].SampleIncrement);
    }

    [Fact]
    public void VolumeControllerUpdatesCurrentAndReplayWithTheirVelocities()
    {
        var table = new VmMusicWildMidiNoteTable();
        table.NoteOn(0, 60, 100, false, Prepare);
        var primary = Assert.Single(table.Voices);
        primary.Envelope = 4;
        table.NoteOn(0, 60, 50, false, Prepare);
        var state = new VmMusicWildMidiChannelState { Expression = 127, Pan = 64, Balance = 64 };
        Assert.True(table.Control(state, 0, 7, 80, 1024, false));
        Assert.Equal(VmMusicWildMidiNoteVolume.Compute(state, 100, 1024, false), primary.MixVolume);
        Assert.Equal(VmMusicWildMidiNoteVolume.Compute(state, 50, 1024, false), primary.Replay!.MixVolume);
    }

    [Fact]
    public void PedalControllerUsesPersistentVoicesAndInvalidEventsFailBeforeMutation()
    {
        var table = new VmMusicWildMidiNoteTable();
        table.NoteOn(0, 60, 100, false, Prepare);
        var voice = Assert.Single(table.Voices);
        voice.Hold = 3;
        var state = new VmMusicWildMidiChannelState { Hold = 1 };
        Assert.True(table.Control(state, 0, 64, 0, 1024, false));
        Assert.Equal(4, voice.Envelope);
        Assert.Equal(0, voice.Hold);
        Assert.Throws<ArgumentOutOfRangeException>(() => table.PitchBend(state, 0, 16384, 44100));
        Assert.Equal(0, state.Pitch);
    }
}
