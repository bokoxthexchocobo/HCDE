namespace HCDE.Net.Core.Tests;

public class VmDNetInvasionPresetTests
{
    [Fact]
    public void RejectWithoutAuthorityNative_WhenNotAuthority() =>
        Assert.True(VmDNetInvasionPreset.RejectWithoutAuthorityNative(false));

    [Fact]
    public void ClearFastMonstersDmflagsNative_ClearsBit() =>
        Assert.Equal(0, VmDNetInvasionPreset.ClearFastMonstersDmflagsNative(0x4000000));

    [Fact]
    public void ResolveTraceReasonNative_UsesDefaultWhenNull() =>
        Assert.Equal("invasion-preset", VmDNetInvasionPreset.ResolveTraceReasonNative(null, "invasion-preset"));
}
