using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiGusEnvelopeTests
{
    [Fact]
    public void TargetsAndRatesUseNativeScaling()
    {
        var envelope = VmMusicWildMidiGusEnvelopes.Convert([63, 63, 63, 63, 63, 63], [255, 128, 0, 1, 2, 3], 64, 44100);
        Assert.Equal(new[] { 4194240, 2105344, 0, 16448, 32896, 49344, 0 }, envelope.Targets);
        Assert.All(envelope.Rates, rate => Assert.Equal(65322, rate));
    }

    [Theory]
    [InlineData(3, 2, 1)]
    [InlineData(3, 1, 2)]
    [InlineData(2, 3, 1)]
    [InlineData(2, 1, 3)]
    [InlineData(1, 3, 2)]
    [InlineData(1, 2, 3)]
    public void ReleaseRepairOrdersAllDistinctPermutations(byte a, byte b, byte c)
    {
        var envelope = VmMusicWildMidiGusEnvelopes.Convert([63, 63, 63, a, b, c], new byte[6], 64, 44100);
        Assert.True(envelope.Rates[3] < envelope.Rates[4]);
        Assert.True(envelope.Rates[4] < envelope.Rates[5]);
    }

    [Fact]
    public void NonEnvelopeModesIgnoreZeroTimeRates()
    {
        var envelope = VmMusicWildMidiGusEnvelopes.Convert(new byte[6], new byte[6], 0, 44100);
        Assert.Equal(4194303, envelope.Targets[0]);
        Assert.Equal(0, envelope.Targets[6]);
        Assert.All(envelope.Rates, rate => Assert.Equal(envelope.Rates[0], rate));
    }

    [Fact]
    public void ZeroTimeAndMalformedInputsThrow()
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusEnvelopes.Convert(new byte[6], new byte[6], 64, 44100));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusEnvelopes.Convert(new byte[5], new byte[6], 0, 44100));
    }
}
