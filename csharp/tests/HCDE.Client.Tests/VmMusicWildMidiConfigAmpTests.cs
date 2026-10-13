using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigAmpTests
{
    [Fact]
    public void AmplificationUsesNativeIntegerScalingAndLastOptionWins()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.Read("0 piano AMP=80tail\n1 flute amp=100 amp=33\n2 silent amp=0");
        Assert.Equal(819, catalog.FindExact(0)!.Amplification);
        Assert.Equal(337, catalog.FindExact(1)!.Amplification);
        Assert.Equal(0, catalog.FindExact(2)!.Amplification);
        reader.ApplyLine("0 replacement");
        Assert.Equal(1024, catalog.FindExact(0)!.Amplification);
    }

    [Fact]
    public void InvalidAmplificationFailsWithoutReplacingMetadata()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.ApplyLine("0 piano amp=80 note=60");
        foreach (var value in new[] { "", "-1", "+1", "x", "2097152", "2147483648" })
            Assert.Throws<InvalidDataException>(() => reader.ApplyLine("0 replacement amp=" + value));
        Assert.Equal(819, catalog.FindExact(0)!.Amplification);
        Assert.Equal(60, catalog.FindExact(0)!.Instrument.Note);
        reader.ApplyLine("0 maximum amp=2097151");
        Assert.Equal(21474826, catalog.FindExact(0)!.Amplification);
    }

    [Fact]
    public void NativeAmplificationMetadataDoesNotAlterDecodedPcm()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1; bytes[335] = 10;
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => bytes);
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano amp=0");
        var patch = catalog.FindExact(0)!;
        catalog.LoadSamples(patch);
        Assert.Equal(2560, patch.Instrument.Samples[0].Pcm.Data[0]);
        Assert.Equal(0, patch.Amplification);
    }
}
