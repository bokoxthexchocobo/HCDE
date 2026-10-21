namespace HCDE.Playsim.Tests;

public class VmSoundEngineLimitStopTests
{
    [Fact]
    public void WithinLimitDistance_ZeroAttnAlwaysInside() =>
        Assert.True(VmSoundEngineLimitStop.WithinLimitDistance(9999, 100, 0, 1));

    [Fact]
    public void ShouldStopUnpositioned_MatchesAutoChannel() =>
        Assert.True(VmSoundEngineLimitStop.ShouldStopUnpositioned(0, -1, 3, -1, 3));
}
