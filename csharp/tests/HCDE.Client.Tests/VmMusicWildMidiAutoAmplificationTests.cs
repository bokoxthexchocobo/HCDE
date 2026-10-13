using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiAutoAmplificationTests
{
    private static byte[] Bytes(byte first, byte second)
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1;
        bytes[335] = first; bytes[336] = second;
        return bytes;
    }

    [Fact]
    public void PositiveTieAndNegativePeaksFollowNativeIntegerFactors()
    {
        foreach (var pair in new[] { (A: (byte)64, B: (byte)0, Factor: (32767 << 10) / 16384),
            (A: (byte)64, B: (byte)192, Factor: (32767 << 10) / 16384),
            (A: (byte)64, B: (byte)128, Factor: 1024) })
        {
            var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes(pair.A, pair.B));
            new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano amp=80\nAUTO_AMP");
            var patch = catalog.FindExact(0)!;
            catalog.LoadSamples(patch);
            Assert.Equal(pair.Factor, patch.Amplification);
            Assert.Equal((short)(unchecked((sbyte)pair.A) << 8), patch.Instrument.Samples[0].Pcm.Data[0]);
        }
    }

    [Fact]
    public void IncludeEnablesSharedStickyCombinationAndLoadedPatchIsNotRecomputed()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes(64, 0));
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true, _ => "auto_amp_with_amp");
        reader.Read("0 piano amp=80\nsource child.cfg\nauto_amp");
        Assert.True(catalog.AutomaticAmplification);
        Assert.True(catalog.CombineAmplification);
        var patch = catalog.FindExact(0)!;
        catalog.LoadSamples(patch);
        Assert.Equal((819 * 2047) >> 10, patch.Amplification);
        var original = patch.Amplification;
        catalog.LoadSamples(patch);
        Assert.Equal(original, patch.Amplification);
    }

    [Fact]
    public void SilentAndOverflowingCombinationFailWithoutPublishingSamples()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes(0, 0));
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano\nauto_amp");
        var patch = catalog.FindExact(0)!;
        Assert.Throws<InvalidDataException>(() => catalog.LoadSamples(patch));
        Assert.False(patch.Loaded);
        Assert.False(patch.HasSamples);
        Assert.Equal(1024, patch.Amplification);
        catalog = new(_ => Bytes(1, 0));
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano amp=2097151\nauto_amp_with_amp");
        Assert.Throws<InvalidDataException>(() => catalog.LoadSamples(catalog.FindExact(0)!));
    }
}
