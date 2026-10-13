using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiReverbCoefficientsTests
{
    [Fact]
    public void SurfacePatternsAndFeedbackNumeratorsFollowNativeLayout()
    {
        var coefficients = VmMusicWildMidiReverbCoefficients.Create(44100, new double[8]);
        for (var band = 0; band < 6; band++)
            for (var component = 0; component < 5; component++)
            {
                Assert.Equal(coefficients[0, band, component], coefficients[7, band, component]);
                Assert.Equal(coefficients[1, band, component], coefficients[6, band, component]);
            }
        Assert.NotEqual(coefficients[0, 0, 0], coefficients[1, 0, 0]);
        Assert.Equal(coefficients[0, 0, 1], coefficients[0, 0, 3]);
        var filter = new VmMusicWildMidiReverbFilter(coefficients);
        Assert.Equal(new VmMusicWildMidiMixVolume(0, 0), filter.Process(0, 0));
    }

    [Fact]
    public void AirAbsorptionAndSampleRateChangeCoefficientResponse()
    {
        var near = VmMusicWildMidiReverbCoefficients.Create(44100, new double[8]);
        var far = VmMusicWildMidiReverbCoefficients.Create(44100, Enumerable.Repeat(100.0, 8).ToArray());
        var lowRate = VmMusicWildMidiReverbCoefficients.Create(11025, new double[8]);
        Assert.True(far[0, 5, 0] < near[0, 5, 0]);
        Assert.NotEqual(near[0, 5, 0], lowRate[0, 5, 0]);
    }

    [Fact]
    public void InvalidDistanceAndRateInputsFailExplicitly()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiReverbCoefficients.Create(0, new double[8]));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiReverbCoefficients.Create(44100, new double[7]));
        foreach (var value in new[] { -1.0, double.NaN, double.PositiveInfinity })
            Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiReverbCoefficients.Create(44100, Enumerable.Repeat(value, 8).ToArray()));
    }
}
