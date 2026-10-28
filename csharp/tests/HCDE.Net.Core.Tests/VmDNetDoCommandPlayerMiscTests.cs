namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandPlayerMiscTests
{
    [Fact]
    public void SelectCrouchDirectionNative_FlipsSign() =>
        Assert.Equal(1, VmDNetDoCommandPlayerMisc.SelectCrouchDirectionNative(-1));

    [Fact]
    public void CrouchBlockedByJumpNative_WhenJumpHeld() =>
        Assert.True(VmDNetDoCommandPlayerMisc.CrouchBlockedByJumpNative(1, 1));

    [Fact]
    public void KillClassRequiresResolvedActorNative() =>
        Assert.False(VmDNetDoCommandPlayerMisc.KillClassRequiresResolvedActorNative(null));
}
