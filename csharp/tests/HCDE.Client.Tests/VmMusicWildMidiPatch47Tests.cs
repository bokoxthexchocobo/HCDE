using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiPatch47Tests
{
    private static byte[] Bytes(byte modes)
    {
        var bytes = new byte[339];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 4; bytes[255] = 2;
        bytes[259] = 1; bytes[269] = 1; bytes[294] = modes;
        for (var i = 0; i < 6; i++) { bytes[276 + i] = (byte)(63 - i); bytes[282 + i] = (byte)(200 - i * 20); }
        return bytes;
    }

    [Fact]
    public void NonLoopingPatch47CopiesConvertedStageTwoWithoutChangingStageSix()
    {
        var patch = new VmMusicWildMidiInstrumentPatch(47);
        patch.Load(Bytes(64));
        var sample = patch.Instrument.Samples[0];
        Assert.True(sample.CopyStageTwoToRelease);
        var original = VmMusicWildMidiGusEnvelopes.Convert(sample.EnvelopeRates.Span, sample.EnvelopeLevels.Span, 64, 44100);
        var envelope = sample.ConvertEnvelope(44100);
        for (var stage = 3; stage < 6; stage++)
        {
            Assert.Equal(original.Targets[2], envelope.Targets[stage]);
            Assert.Equal(original.Rates[2], envelope.Rates[stage]);
        }
        Assert.Equal(original.Targets[6], envelope.Targets[6]);
        Assert.Equal(original.Rates[6], envelope.Rates[6]);
        var voice = VmMusicWildMidiVoiceInitialization.Create(sample, new(), 0, 60, 100, 0, 44100, 1024, false);
        Assert.Equal(envelope.Targets, voice.Voice.EnvelopeTargets);
        Assert.Equal(envelope.Rates, voice.Voice.EnvelopeRates);
    }

    [Fact]
    public void LoopingBankedAndPercussionVariantsDoNotReceiveSpecialAdjustment()
    {
        foreach (var pair in new[] { (Id: (ushort)47, Modes: (byte)68), (Id: (ushort)303, Modes: (byte)64), (Id: (ushort)175, Modes: (byte)64) })
        {
            var patch = new VmMusicWildMidiInstrumentPatch(pair.Id);
            patch.Load(Bytes(pair.Modes));
            var sample = patch.Instrument.Samples[0];
            Assert.False(sample.CopyStageTwoToRelease);
            var original = VmMusicWildMidiGusEnvelopes.Convert(sample.EnvelopeRates.Span, sample.EnvelopeLevels.Span, sample.Header.Modes, 44100);
            var envelope = sample.ConvertEnvelope(44100);
            Assert.Equal(original.Targets, envelope.Targets);
            Assert.Equal(original.Rates, envelope.Rates);
        }
    }
}
