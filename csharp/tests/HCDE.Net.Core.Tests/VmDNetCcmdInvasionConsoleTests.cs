namespace HCDE.Net.Core.Tests;

public class VmDNetCcmdInvasionConsoleTests
{
    [Fact]
    public void InvasionControlRejectedNative_WhenZero() =>
        Assert.True(VmDNetCcmdInvasionConsole.InvasionControlRejectedNative(0));

    [Fact]
    public void ClampedMapWaveLimitNative_ClampsHigh() =>
        Assert.Equal(10, VmDNetCcmdInvasionConsole.ClampedMapWaveLimitNative(99, 10));
}
