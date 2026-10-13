using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiReleaseFixConfigTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    public void KeepEnvelopeOnOriginallyUnenvelopedPatchUsesDecodedDefaults(int rate)
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1;
        for (var i = 0; i < 6; i++) { bytes[276 + i] = (byte)rate; bytes[282 + i] = 10; }
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => bytes);
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano keep=env env_level2=0.5 env_time3=100");
        var patch = catalog.FindExact(0)!;
        catalog.LoadSamples(patch);
        var sample = patch.Instrument.Samples[0];
        Assert.Equal(64, sample.Header.Modes & 64);
        var envelope = sample.ConvertEnvelope(44100);
        var defaults = VmMusicWildMidiGusEnvelopes.Convert(new byte[6], new byte[6], 0, 44100);
        for (var stage = 0; stage < 6; stage++)
        {
            Assert.Equal(stage == 2 ? 16448 * 127 : defaults.Targets[stage], envelope.Targets[stage]);
            Assert.Equal(stage == 3 ? (int)(4194303.0 / (44100f * (100f / 1000.0))) : defaults.Rates[stage], envelope.Rates[stage]);
        }
        Assert.Equal(defaults.Rates[6], envelope.Rates[6]);
    }

    private const string Directive = "guspat_editor_author_cant_read_so_fix_release_time_for_me";

    [Fact]
    public void IncludedDirectiveSetsSharedStickyStateWithoutChangingParentBank()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        Assert.False(catalog.FixRelease);
        var reader = new VmMusicWildMidiConfigReader(catalog, "main.cfg", true, _ => Directive.ToUpperInvariant());
        reader.Read("bank 2\nsource child.cfg\n3 piano");
        Assert.True(catalog.FixRelease);
        Assert.NotNull(catalog.FindExact(0x203));
        reader.ApplyLine(Directive + " ignored");
        Assert.True(catalog.FixRelease);
    }

    [Fact]
    public void DirectiveDoesNotGateAutomaticRepairOrChangeConvertedEnvelope()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1; bytes[294] = 64;
        for (var i = 0; i < 6; i++) { bytes[276 + i] = 63; bytes[282 + i] = 200; }
        bytes[279] = 63; bytes[280] = 62; bytes[281] = 61;
        VmMusicWildMidiGusEnvelope Load(bool enabled)
        {
            var catalog = new VmMusicWildMidiInstrumentCatalog(_ => bytes);
            var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
            reader.Read((enabled ? Directive + "\n" : "") + "0 piano");
            var patch = catalog.FindExact(0)!;
            catalog.LoadSamples(patch);
            return patch.Instrument.Samples[0].ConvertEnvelope(44100);
        }
        var automatic = Load(false);
        var configured = Load(true);
        Assert.Equal(automatic.Targets, configured.Targets);
        Assert.Equal(automatic.Rates, configured.Rates);
        Assert.True(automatic.Rates[3] < automatic.Rates[4]);
        Assert.True(automatic.Rates[4] < automatic.Rates[5]);
    }
}
