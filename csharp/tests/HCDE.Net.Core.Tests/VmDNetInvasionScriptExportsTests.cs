namespace HCDE.Net.Core.Tests;

public class VmDNetInvasionScriptExportsTests
{
    [Fact]
    public void InvasionGetStateExportNative_Passthrough() =>
        Assert.Equal(3, VmDNetInvasionScriptExports.InvasionGetStateExportNative(3));

    [Fact]
    public void InvasionBossWaveAsIntNative_ReturnsOne() =>
        Assert.Equal(1, VmDNetInvasionScriptExports.InvasionBossWaveAsIntNative(true));

    [Fact]
    public void InvasionGetWaveExportNative_ClampsNegative() =>
        Assert.Equal(0, VmDNetInvasionScriptExports.InvasionGetWaveExportNative(-1));

    [Fact]
    public void InvasionControlExportNative_Passthrough() =>
        Assert.Equal(1, VmDNetInvasionScriptExports.InvasionControlExportNative(1));
}
