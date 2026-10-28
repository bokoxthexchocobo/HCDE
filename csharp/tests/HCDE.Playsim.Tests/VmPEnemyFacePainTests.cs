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
    public void Wander_SkipsConversation() =>
        Assert.True(VmPEnemyFacePain.WanderSkipsConversation(VmPEnemyFacePain.Mf5Inconversation));

    [Fact]
    public void Wander_Standstill() =>
        Assert.True(VmPEnemyFacePain.WanderStandstill(VmPEnemyFacePain.Mf4Standstill));
}
