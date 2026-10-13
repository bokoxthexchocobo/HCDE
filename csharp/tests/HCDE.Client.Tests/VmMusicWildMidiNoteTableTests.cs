using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiNoteTableTests
{
    private static VmMusicWildMidiInitializedVoice? Prepare(int channel, byte note, byte velocity) => new(new()
    {
        NoteId = (ushort)(channel << 8 | note), Velocity = velocity, Modes = 64,
        EnvelopeTargets = new int[7], EnvelopeRates = [1, 1, 1, 1, 1, 1, 10]
    }, new(new short[3], new(1024, 0, 0, 0)));

    [Fact]
    public void ZeroVelocityDispatchesReleaseWithoutPreparingSample()
    {
        var table = new VmMusicWildMidiNoteTable();
        Assert.True(table.NoteOn(0, 60, 100, false, Prepare));
        Assert.True(table.NoteOn(0, 60, 0, false, (_, _, _) => throw new InvalidOperationException()));
        Assert.True(Assert.Single(table.Voices).IsOff);
    }

    [Fact]
    public void RepeatedNotesReuseBothSlotsAcrossReplayActivation()
    {
        var table = new VmMusicWildMidiNoteTable();
        table.NoteOn(0, 60, 100, false, Prepare);
        var primary = Assert.Single(table.Voices);
        primary.Envelope = 4;
        Assert.True(table.NoteOn(0, 60, 101, false, Prepare));
        var secondary = primary.Replay;
        VmMusicWildMidiVoiceLifecycle.Complete(table.Voices, 0);
        Assert.Same(secondary, Assert.Single(table.Voices));
        Assert.True(table.NoteOn(0, 60, 102, false, Prepare));
        Assert.Same(primary, secondary!.Replay);
        Assert.Equal(102, primary.Velocity);
    }

    [Fact]
    public void ChannelsAreIndependentAndMissingSamplesDoNotInsertVoices()
    {
        var table = new VmMusicWildMidiNoteTable();
        Assert.False(table.NoteOn(0, 60, 100, false, (_, _, _) => null));
        Assert.Empty(table.Voices);
        table.NoteOn(0, 60, 100, false, Prepare);
        table.NoteOn(1, 60, 100, false, Prepare);
        Assert.Equal(2, table.Voices.Count);
        table.NoteOff(1, 60, false);
        Assert.False(table.Voices[0].IsOff);
        Assert.True(table.Voices[1].IsOff);
    }
}
