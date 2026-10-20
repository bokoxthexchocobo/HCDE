namespace HCDE.Playsim.Tests;

public class VmPSpecActorSpecialFinishTests
{
    [Fact]
    public void NegativeDamageHealTick() =>
        Assert.True(VmPSpecActorSpecialFinish.NegativeDamageHealTick(-5, 32, 32));

    [Fact]
    public void PlayerTouchesSecret() =>
        Assert.True(VmPSpecActorSpecialFinish.PlayerTouchesSecret(true, true));
}
