using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigNoteTests
{
    [Fact]
    public void NotesUseCaseInsensitiveDecimalPrefixesAndLastOptionWins()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.Read("0 piano NoTe=60tail note=72\ndrumset 1\n35 kick note=36");
        Assert.Equal(72, catalog.FindExact(0)!.Instrument.Note);
        Assert.Equal(36, catalog.FindExact(0x1a3)!.Instrument.Note);
        reader.ApplyLine("bank 0");
        reader.ApplyLine("0 piano note=0");
        Assert.Equal(0, catalog.FindExact(0)!.Instrument.Note);
        reader.ApplyLine("0 piano note=328");
        Assert.Equal(72, catalog.FindExact(0)!.Instrument.Note);
    }

    [Fact]
    public void InvalidOptionsDoNotReplaceExistingConfiguration()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.ApplyLine("0 piano note=60");
        foreach (var value in new[] { "", "-1", "+1", "x", "128", "2147483648" })
            Assert.Throws<InvalidDataException>(() => reader.ApplyLine("0 replacement note=" + value));
        Assert.Throws<NotSupportedException>(() => reader.ApplyLine("0 replacement note=72 unconverted=80"));
        Assert.Equal(60, catalog.FindExact(0)!.Instrument.Note);
    }

    [Fact]
    public void LoadedNoteOverrideReachesPreparedVoicePitch()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1;
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => bytes);
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano note=72");
        var patch = catalog.FindExact(0)!;
        catalog.LoadSamples(patch);
        var state = new VmMusicWildMidiChannelState { Patch = patch };
        var prepared = VmMusicWildMidiNotePreparation.Prepare(state, catalog.FindExact, 0, 60, 100, 44100, 1024, false);
        Assert.NotNull(prepared);
        Assert.Equal(72, prepared.Value.Voice.PatchNote);
        Assert.Equal(VmMusicWildMidiSampleIncrement.Compute(60, 72, 0, 44100, prepared.Value.Voice.IncrementDivisor),
            prepared.Value.Voice.SampleIncrement);
    }
}
