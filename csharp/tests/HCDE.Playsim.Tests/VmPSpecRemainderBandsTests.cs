namespace HCDE.Playsim.Tests;

public class VmPSpecRemainderBandsTests
{
    [Fact]
    public void NegativeDamageHealTick() =>
        Assert.True(VmPSpecRemainderBands.NegativeDamageHealTick(-3, 32, 32));

    [Fact]
    public void ShowSecretSectorCvar() =>
        Assert.False(VmPSpecRemainderBands.ShowSecretSectorCvar(false));

    [Fact]
    public void ClShowSecretMessageCvar() =>
        Assert.True(VmPSpecRemainderBands.ClShowSecretMessageCvar(true));

    [Fact]
    public void HealBodyAmount() =>
        Assert.Equal(5, VmPSpecRemainderBands.HealBodyAmount(-5));

    [Fact]
    public void GiveSecretActionParams() =>
        Assert.True(VmPSpecRemainderBands.GiveSecretActionParams(true, false));

    [Fact]
    public void ActivateLineStub() =>
        Assert.True(VmPSpecRemainderBands.ActivateLineStub(0));

    [Fact]
    public void RemoteActivateLineStub() =>
        Assert.True(VmPSpecRemainderBands.RemoteActivateLineStub(1));

    [Fact]
    public void DoSectorDamageComment() =>
        Assert.True(VmPSpecRemainderBands.DoSectorDamageComment());

    [Fact]
    public void ActorOnSpecialFlatComment() =>
        Assert.True(VmPSpecRemainderBands.ActorOnSpecialFlatComment());

    [Fact]
    public void TimelimitExternDeclared() =>
        Assert.True(VmPSpecRemainderBands.TimelimitExternDeclared());
}
