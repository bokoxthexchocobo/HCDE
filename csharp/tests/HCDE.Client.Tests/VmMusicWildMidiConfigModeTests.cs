using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigModeTests
{
    private static byte[] Bytes(byte modes)
    {
        var bytes = new byte[339];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 4; bytes[255] = 2;
        bytes[259] = 1; bytes[269] = 1; bytes[294] = modes;
        return bytes;
    }

    [Fact]
    public void DrumDefaultsStripLoopAndEnvelopeWhileMelodicKeepsThem()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes(228));
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano\ndrumset 0\n35 drum");
        foreach (var id in new ushort[] { 0, 163 }) catalog.LoadSamples(catalog.FindExact(id)!);
        Assert.Equal(228, catalog.FindExact(0)!.Instrument.Samples[0].Pcm.Modes);
        Assert.Equal(160, catalog.FindExact(163)!.Instrument.Samples[0].Pcm.Modes);
    }

    [Fact]
    public void ConfiguredKeepAndRemoveFlagsReachHeaderAndPcmModes()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes(228));
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("drumset 0\n35 drum KEEP=loop keep=env remove=sustain REMOVE=clamped");
        var patch = catalog.FindExact(163)!;
        catalog.LoadSamples(patch);
        Assert.Equal(68, patch.Keep);
        Assert.Equal(160, patch.Remove);
        var sample = patch.Instrument.Samples[0];
        Assert.Equal(68, sample.Pcm.Modes);
        Assert.Equal(68, sample.Header.Modes);
    }

    [Fact]
    public void KeepEnvelopeForcesFlagAndReplacementResetsOptionsForReload()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes(0));
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.Read("0 piano keep=env");
        var patch = catalog.FindExact(0)!;
        catalog.LoadSamples(patch);
        Assert.Equal(64, patch.Instrument.Samples[0].Pcm.Modes);
        reader.ApplyLine("0 replacement");
        Assert.Equal(0, patch.Keep);
        Assert.Equal(64, patch.Instrument.Samples[0].Pcm.Modes);
        patch.FreeSamples();
        catalog.LoadSamples(patch);
        Assert.Equal(0, patch.Instrument.Samples[0].Pcm.Modes);
    }
}
