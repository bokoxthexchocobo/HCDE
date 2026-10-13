using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiReverbFilterTests
{
    [Fact]
    public void ParallelBandsUseReflectionInputAndTruncateEachContribution()
    {
        var coefficients = new int[8, 6, 5];
        for (var reflection = 0; reflection < 8; reflection++)
            for (var band = 0; band < 6; band++) coefficients[reflection, band, 0] = 1024;
        var filter = new VmMusicWildMidiReverbFilter(coefficients);
        Assert.Equal(new VmMusicWildMidiMixVolume(96, -96), filter.Process(17, -17));
        Assert.Equal(new VmMusicWildMidiMixVolume(0, 0), filter.Process(0, 0));
    }

    [Fact]
    public void StereoHistoriesFeedbackAndResetFollowNativeRecurrence()
    {
        var coefficients = new int[8, 6, 5];
        coefficients[0, 0, 0] = 1024;
        coefficients[0, 0, 1] = 512;
        coefficients[0, 0, 2] = 256;
        coefficients[0, 0, 3] = 256;
        coefficients[0, 0, 4] = 128;
        var filter = new VmMusicWildMidiReverbFilter(coefficients);
        coefficients[0, 0, 0] = 0;
        Assert.Equal(new VmMusicWildMidiMixVolume(128, 0), filter.Process(1024, 0));
        Assert.Equal(new VmMusicWildMidiMixVolume(32, 128), filter.Process(0, 1024));
        Assert.Equal(new VmMusicWildMidiMixVolume(8, 32), filter.Process(0, 0));
        filter.Reset();
        Assert.Equal(new VmMusicWildMidiMixVolume(0, 0), filter.Process(0, 0));
        Assert.Equal(new VmMusicWildMidiMixVolume(128, 0), filter.Process(1024, 0));
    }

    [Fact]
    public void InvalidCoefficientDimensionsFailAtConstruction()
    {
        Assert.Throws<ArgumentException>(() => new VmMusicWildMidiReverbFilter(new int[8, 5, 5]));
        Assert.Throws<ArgumentNullException>(() => new VmMusicWildMidiReverbFilter(null!));
    }
}
