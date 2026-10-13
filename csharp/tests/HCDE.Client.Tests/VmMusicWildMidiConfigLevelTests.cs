using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigLevelTests
{
    private static byte[] Bytes()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1; bytes[294] = 64;
        for (var i = 0; i < 6; i++) { bytes[276 + i] = 63; bytes[282 + i] = 200; }
        return bytes;
    }

    [Fact]
    public void LevelOverridesFollowFloatRoundingAndApplyAfterPatch47Adjustment()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes());
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("47 string ENV_LEVEL3=0.5tail env_level4=1e0 env_level5=0");
        var patch = catalog.FindExact(47)!;
        catalog.LoadSamples(patch);
        var sample = patch.Instrument.Samples[0];
        var envelope = sample.ConvertEnvelope(44100);
        Assert.Equal(16448 * 127, envelope.Targets[3]);
        Assert.Equal(16448 * 255, envelope.Targets[4]);
        Assert.Equal(0, envelope.Targets[5]);
        Assert.Equal(envelope.Rates[2], envelope.Rates[3]);
        Assert.Equal(0, envelope.Targets[6]);
        var voice = VmMusicWildMidiVoiceInitialization.Create(sample, new(), 0, 60, 100, 0, 44100, 1024, false);
        Assert.Equal(envelope.Targets, voice.Voice.EnvelopeTargets);
    }

    [Fact]
    public void MalformedLevelsPreservePatchAndReplacementOnlyAffectsReload()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes());
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.ApplyLine("0 piano env_level0=0.5 env_level0=0.25");
        foreach (var option in new[] { "env_level6=0", "env_level0=", "env_level0=-1", "env_level0=.5", "env_level0=2", "env_level0=1e999" })
            Assert.Throws<InvalidDataException>(() => reader.ApplyLine("0 replacement " + option));
        var patch = catalog.FindExact(0)!;
        catalog.LoadSamples(patch);
        var sample = patch.Instrument.Samples[0];
        Assert.Equal(16448 * 63, sample.ConvertEnvelope(44100).Targets[0]);
        reader.ApplyLine("0 replacement");
        Assert.Equal(16448 * 63, sample.ConvertEnvelope(44100).Targets[0]);
        patch.FreeSamples();
        catalog.LoadSamples(patch);
        Assert.Equal(16448 * 200, patch.Instrument.Samples[0].ConvertEnvelope(44100).Targets[0]);
    }
}
