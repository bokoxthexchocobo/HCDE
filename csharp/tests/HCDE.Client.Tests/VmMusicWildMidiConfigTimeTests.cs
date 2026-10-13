using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigTimeTests
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
    public void TimesOverrideConvertedRatesAfterPatch47AndReachVoices()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes());
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("47 string ENV_TIME3=100tail env_time3=2e2 env_time4=1.47 env_time5=45000 env_level3=0.5");
        var patch = catalog.FindExact(47)!;
        catalog.LoadSamples(patch);
        var sample = patch.Instrument.Samples[0];
        foreach (var rate in new[] { 11025, 44100, 65535 })
        {
            var envelope = sample.ConvertEnvelope(rate);
            Assert.Equal((int)(4194303.0 / (rate * (200f / 1000.0))), envelope.Rates[3]);
            Assert.Equal((int)(4194303.0 / (rate * (1.47f / 1000.0))), envelope.Rates[4]);
            Assert.Equal((int)(4194303.0 / (rate * (45000f / 1000.0))), envelope.Rates[5]);
            Assert.Equal(16448 * 127, envelope.Targets[3]);
            var original = VmMusicWildMidiGusEnvelopes.Convert(sample.EnvelopeRates.Span, sample.EnvelopeLevels.Span, 64, rate);
            Assert.Equal(original.Rates[6], envelope.Rates[6]);
        }
        var voice = VmMusicWildMidiVoiceInitialization.Create(sample, new(), 0, 60, 100, 0, 44100, 1024, false);
        Assert.Equal(475, voice.Voice.EnvelopeRates[3]);
    }

    [Fact]
    public void InvalidTimesPreserveDeclarationAndReloadResetsOverrides()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes());
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.ApplyLine("0 piano env_time0=200");
        foreach (var option in new[] { "env_time6=200", "env_time0=", "env_time0=-200", "env_time0=1.46", "env_time0=45001", "env_time0=1e999" })
            Assert.Throws<InvalidDataException>(() => reader.ApplyLine("0 replacement " + option));
        var patch = catalog.FindExact(0)!;
        catalog.LoadSamples(patch);
        var sample = patch.Instrument.Samples[0];
        Assert.Equal(475, sample.ConvertEnvelope(44100).Rates[0]);
        reader.ApplyLine("0 replacement");
        Assert.Equal(475, sample.ConvertEnvelope(44100).Rates[0]);
        patch.FreeSamples();
        catalog.LoadSamples(patch);
        Assert.NotEqual(475, patch.Instrument.Samples[0].ConvertEnvelope(44100).Rates[0]);
    }
}
