namespace HCDE.Playsim.Tests;

public class VmPSpecRemainderBandsTests
{
    [Fact]
    public void NegativeDamageHealTick() =>
        Assert.True(VmPSpecRemainderBands.NegativeDamageHealTick(-3, 32, 32));

    [Fact]
    public void ShowSecretSectorCvar() =>
        Assert.False(VmPSpecRemainderBands.ShowSecretSectorCvar(false));
}
