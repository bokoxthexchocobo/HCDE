namespace HCDE.Playsim.Tests;

public class VmPEnemyFacePainTests
{
    [Fact]
    public void Face_SkipsNullOther() =>
        Assert.False(VmPEnemyFacePain.FaceRequiresOther(true));

    [Fact]
    public void Face_LimitsYaw() =>
        Assert.True(VmPEnemyFacePain.YawTurnLimited(10, 20));

    [Fact]
    public void Pain_LowHealthSuffix() =>
        Assert.Equal("*pain25", VmPEnemyFacePain.PainAmountSuffix(10));

    [Fact]
    public void Pain_PlayerBranch() =>
        Assert.True(VmPEnemyFacePain.PainUsesPlayerBranch(true, true));
}
