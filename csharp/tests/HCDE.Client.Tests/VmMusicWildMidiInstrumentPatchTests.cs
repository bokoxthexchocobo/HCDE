using System.Buffers.Binary;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiInstrumentPatchTests
{
    private static byte[] Bytes()
    {
        var data = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(data);
        data[198] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(247), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(259), 22050);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(269), 440000);
        data[335] = 1; data[336] = 3;
        return data;
    }

    [Fact]
    public void ProgramChangeBindsLoadedPatchThroughNoteOnAndMixer()
    {
        var patch = new VmMusicWildMidiInstrumentPatch(7);
        patch.Load(Bytes());
        var state = new VmMusicWildMidiChannelState { Volume = 100, Expression = 127, Pan = 64, Balance = 64 };
        VmMusicWildMidiPatchLookup.ChangeProgram(state, 7, id => id == 7 ? patch : null);
        var table = new VmMusicWildMidiNoteTable();
        Assert.True(table.NoteOn(0, 60, 100, false, (channel, note, velocity) =>
            VmMusicWildMidiNotePreparation.Prepare(state, _ => patch, channel, note, velocity, 44100, 1024, false)));
        Assert.True(patch.Loaded);
        Assert.True(patch.HasSamples);
        var output = new int[2];
        VmMusicWildMidiLinearMixer.Mix(table.Voices, output);
        Assert.True(Assert.Single(table.Voices).SamplePosition > 0);
    }

    [Fact]
    public void FailedReloadRetainsExistingInstrument()
    {
        var patch = new VmMusicWildMidiInstrumentPatch(0);
        patch.Load(Bytes());
        var previous = patch.Instrument;
        Assert.Throws<ArgumentException>(() => patch.Load(new byte[1]));
        Assert.Same(previous, patch.Instrument);
        Assert.True(patch.Loaded);
    }

    [Fact]
    public void ReferenceReleaseClearsSamplesAfterLastUser()
    {
        var patch = new VmMusicWildMidiInstrumentPatch(0);
        var users = new List<VmMusicWildMidiInstrumentPatch>();
        Assert.Equal(VmMusicWildMidiPatchLoadResult.Added,
            VmMusicWildMidiPatchLoading.Load(0, users, _ => patch, item => { item.Load(Bytes()); return 0; }));
        Assert.Equal(1, patch.UseCount);
        VmMusicWildMidiPatchLoading.Release(users, item => item.FreeSamples());
        Assert.False(patch.HasSamples);
        Assert.False(patch.Loaded);
        Assert.Equal(0, patch.UseCount);
    }
}
