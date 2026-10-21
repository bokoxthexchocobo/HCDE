namespace HCDE.Playsim.Tests;

public class VmActorDoDamageTests
{
    [Fact]
    public void CheckClass_ExcludeInverts() =>
        Assert.False(VmActorDoDamage.CheckClass("A", "A", exclude: true));

    [Fact]
    public void PassesFilterSpecies_EitherMode() =>
        Assert.True(VmActorDoDamage.PassesFilterSpecies(false, true, VmActorDoDamage.DmssFlags.Either));

    [Fact]
    public void AssembleDamageFlags_KillAddsNoFactor() =>
        Assert.NotEqual(0, VmActorDoDamage.AssembleDamageFlags(VmActorDoDamage.DmssFlags.Kill));

    [Fact]
    public void AdjustKillAmount_AddsHealth() =>
        Assert.Equal(15, VmActorDoDamage.AdjustKillAmount(5, 10, VmActorDoDamage.DmssFlags.Kill));

    [Fact]
    public void PlanDoDamage_HealWhenNegative() =>
        Assert.Equal(VmActorDoDamage.DamagePlan.Heal, VmActorDoDamage.PlanDoDamage(-5, passesFilter: true));

    [Fact]
    public void SetTicsOnActor_WhenFromActor() =>
        Assert.True(VmActorDoDamage.SetTicsOnActor(fromActor: true));

    [Fact]
    public void DamageMasterTarget_RequiresMaster() =>
        Assert.False(VmActorDoDamage.DamageMasterTarget(hasMaster: false));

    [Fact]
    public void ShouldDamageChild_WhenMasterMatches() =>
        Assert.True(VmActorDoDamage.ShouldDamageChildActor(masterMatches: true));
}
