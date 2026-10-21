namespace HCDE.Playsim.Tests;

public class VmActorWeaveWolfTests
{
    [Fact]
    public void AdvanceWeaveIndex_WrapsAt64() =>
        Assert.Equal(0, VmActorWeaveWolf.AdvanceWeaveIndex(63, 1));

    [Fact]
    public void LineEffectSetsDone_WhenNonRepeating() =>
        Assert.True(VmActorWeaveWolf.LineEffectSetsDone(true, false));

    [Fact]
    public void WolfHitChance_ReducesWithDistance() =>
        Assert.Equal(248, VmActorWeaveWolf.WolfHitChance(256, 1, false));

    [Fact]
    public void WolfDamageAtRange_HalvesTwice() =>
        Assert.Equal(10, VmActorWeaveWolf.WolfDamageAtRange(40, 100, 50, 80));
}
