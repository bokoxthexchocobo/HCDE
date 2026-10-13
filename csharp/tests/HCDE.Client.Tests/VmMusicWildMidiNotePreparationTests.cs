using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiNotePreparationTests
{
    private static VmMusicWildMidiGusPatchSample Sample(uint low, uint high, byte value) => new(
        new(2, 0, 0, 22050, low, high, 440000, 20432, 0, 0),
        VmMusicWildMidiPcmDecoder.Decode(new byte[] { value, value }, 0, 0, 0), new byte[6], new byte[6]);

    [Fact]
    public void MelodicSelectionUsesEventPitchAndIncrementUsesPatchPitch()
    {
        var instrument = new VmMusicWildMidiInstrument(72, new[] { Sample(0, 400000, 1), Sample(400000, 900000, 2) });
        var state = new VmMusicWildMidiChannelState { Volume = 100, Expression = 127, Pan = 64, Balance = 64 };
        var prepared = VmMusicWildMidiNotePreparation.Prepare(state, instrument, _ => throw new InvalidOperationException(), 0, 60, 100, 44100, 1024, false);
        Assert.Equal(256, prepared!.Value.Sample.Data.Span[0]);
        Assert.Equal(72, prepared.Value.Voice.PatchNote);
    }

    [Fact]
    public void PercussionLookupFallsBackAndSelectsUsingPatchPitch()
    {
        var instrument = new VmMusicWildMidiInstrument(72, new[] { Sample(0, 400000, 1), Sample(400000, 900000, 2) });
        var calls = new List<ushort>();
        var state = new VmMusicWildMidiChannelState { IsDrum = true, Bank = 2 };
        var prepared = VmMusicWildMidiNotePreparation.Prepare(state, null, id => { calls.Add(id); return id == 0xbc ? instrument : null; }, 9, 60, 100, 44100, 1024, false);
        Assert.Equal(new ushort[] { 0x2bc, 0xbc }, calls);
        Assert.Equal(512, prepared!.Value.Sample.Data.Span[0]);
    }
}
