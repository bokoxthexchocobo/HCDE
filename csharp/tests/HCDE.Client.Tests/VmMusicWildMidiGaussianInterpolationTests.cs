using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiGaussianInterpolationTests
{
    private static VmMusicWildMidiNoteReleaseState Voice(uint position) => new()
    {
        SamplePosition = position, EnvelopeLevel = 1024 << 12, MixVolume = new(1024, 512)
    };

    [Theory]
    [InlineData(0, 0, 100)]
    [InlineData(0, 512, 200)]
    [InlineData(2, 512, 600)]
    [InlineData(3, 512, 800)]
    public void NewtonBoundaryWindowsReproduceLinearRamp(int index, int fraction, int expected)
    {
        var sample = new VmMusicWildMidiSampleData(new short[] { 100, 300, 500, 700, 900 }, new(4096, 0, 0, 0));
        Assert.Equal(new(expected, expected / 2), VmMusicWildMidiGaussianInterpolation.Interpolate(
            sample, Voice((uint)(index * 1024 + fraction))));
    }

    [Fact]
    public void InteriorGaussianWindowReproducesIntegerSample()
    {
        var samples = Enumerable.Range(0, 50).Select(i => (short)(i * 100)).ToArray();
        Assert.Equal(new(2500, 1250), VmMusicWildMidiGaussianInterpolation.Interpolate(
            new(samples, new(50 * 1024, 0, 0, 0)), Voice(25 * 1024)));
    }

    [Fact]
    public void EnhancedMixerUsesBoundaryInterpolationAndExhaustion()
    {
        var voice = Voice(512);
        voice.Active = true;
        voice.SampleIncrement = 2048;
        var voices = new List<VmMusicWildMidiNoteReleaseState> { voice };
        var output = new int[4];
        VmMusicWildMidiLinearMixer.Mix(voices, output,
            _ => new(new short[] { 100, 300, 0 }, new(2048, 0, 0, 0)), true);
        Assert.Equal(new[] { 200, 100, 0, 0 }, output);
        Assert.False(voice.Active);
        Assert.Empty(voices);
    }

    [Fact]
    public void MissingBoundaryGuardSampleThrows()
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGaussianInterpolation.Interpolate(
            new(new short[] { 100 }, new(1024, 0, 0, 0)), Voice(0)));
    }
}
