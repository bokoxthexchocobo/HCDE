using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiDeviceResourcesTests
{
    private static VmMusicWildMidiInitializedVoice? Prepare(int channel, byte note, byte velocity) => new(new()
    {
        NoteId = (ushort)(channel << 8 | note), Velocity = velocity, Modes = 64,
        EnvelopeTargets = new int[7], EnvelopeRates = [1, 1, 1, 1, 1, 1, 10]
    }, new(new short[3], new(1024, 0, 0, 0)));

    [Fact]
    public void ClearDropsActiveAndQueuedBindingsAndAllowsFreshNotes()
    {
        var table = new VmMusicWildMidiNoteTable();
        table.NoteOn(0, 60, 100, false, Prepare);
        var primary = Assert.Single(table.Voices);
        primary.Envelope = 4;
        table.NoteOn(0, 60, 100, false, Prepare);
        var replay = primary.Replay!;
        table.Clear();
        Assert.Empty(table.Voices);
        Assert.False(primary.Active);
        Assert.False(replay.Active);
        Assert.Null(primary.Replay);
        Assert.Null(primary.Sample);
        Assert.Null(replay.Sample);
        Assert.Empty(primary.EnvelopeRates);
        Assert.True(table.NoteOn(0, 60, 100, false, Prepare));
        Assert.Same(primary, Assert.Single(table.Voices));
    }

    [Fact]
    public void SharedPatchSurvivesUntilLastDeviceDisposal()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1;
        bytes[247] = 2;
        bytes[259] = 1;
        bytes[269] = 1;
        var patch = new VmMusicWildMidiInstrumentPatch(0);
        patch.Load(bytes);
        using var first = new VmMusicWildMidiDeviceResources();
        using var second = new VmMusicWildMidiDeviceResources();
        first.Load(0, _ => patch, _ => throw new InvalidOperationException());
        second.Load(0, _ => patch, _ => throw new InvalidOperationException());
        Assert.Equal(2, patch.UseCount);
        first.Dispose();
        Assert.Equal(1, patch.UseCount);
        Assert.True(patch.HasSamples);
        second.Dispose();
        Assert.Equal(0, patch.UseCount);
        Assert.False(patch.HasSamples);
    }

    [Fact]
    public void DisposalIsIdempotentAndBlocksFurtherPatchLoading()
    {
        var resources = new VmMusicWildMidiDeviceResources();
        resources.Notes.NoteOn(0, 60, 100, false, Prepare);
        resources.Dispose();
        resources.Dispose();
        Assert.Empty(resources.Notes.Voices);
        Assert.Throws<ObjectDisposedException>(() => resources.Load(0, _ => null, _ => 0));
    }
}
