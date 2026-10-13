using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiNoteTablePressureTests
{
    private static VmMusicWildMidiInitializedVoice? Prepare(int channel, byte note, byte velocity) => new(new()
    {
        NoteId = (ushort)(channel << 8 | note), Velocity = velocity, Modes = 64,
        EnvelopeTargets = new int[7], EnvelopeRates = [1, 1, 1, 1, 1, 1, 10]
    }, new(new short[3], new(1024, 0, 0, 0)));
    private static VmMusicWildMidiChannelState State() => new() { Volume = 100, Expression = 127, Pan = 64, Balance = 64, Pressure = 12 };

    [Fact]
    public void PolyPressureUpdatesActiveVoiceAndQueuedReplayVolume()
    {
        var table = new VmMusicWildMidiNoteTable();
        table.NoteOn(0, 60, 100, false, Prepare);
        var primary = Assert.Single(table.Voices);
        primary.Envelope = 4;
        table.NoteOn(0, 60, 101, false, Prepare);
        var state = State();
        Assert.True(table.Aftertouch(state, 0, 60, 50, 1024, false));
        Assert.Equal(50, primary.Velocity);
        Assert.Equal(50, primary.Replay!.Velocity);
        Assert.Equal(VmMusicWildMidiNoteVolume.Compute(state, 50, 1024, false), primary.MixVolume);
        Assert.Equal(primary.MixVolume, primary.Replay.MixVolume);
    }

    [Fact]
    public void ChannelPressureFiltersVoicesAndDoesNotChangeStoredChannelPressure()
    {
        var table = new VmMusicWildMidiNoteTable();
        table.NoteOn(0, 60, 100, false, Prepare);
        table.NoteOn(1, 60, 100, false, Prepare);
        var state = State();
        table.ChannelPressure(state, 1, 40, 1024, true);
        Assert.Equal(100, table.Voices[0].Velocity);
        Assert.Equal(40, table.Voices[1].Velocity);
        Assert.Equal(12, state.Pressure);
        Assert.Equal(VmMusicWildMidiNoteVolume.Compute(state, 40, 1024, true), table.Voices[1].MixVolume);
    }

    [Fact]
    public void InactiveNotesAndInvalidPressureDoNotMutateVoices()
    {
        var table = new VmMusicWildMidiNoteTable();
        Assert.False(table.Aftertouch(State(), 0, 60, 50, 1024, false));
        table.NoteOn(0, 60, 100, false, Prepare);
        Assert.Throws<ArgumentOutOfRangeException>(() => table.Aftertouch(State(), 0, 60, 128, 1024, false));
        Assert.Equal(100, Assert.Single(table.Voices).Velocity);
    }
}
