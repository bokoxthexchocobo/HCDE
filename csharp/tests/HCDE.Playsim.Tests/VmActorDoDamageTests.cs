namespace HCDE.Playsim.Tests;

public class VmActorDoDamageTests
{
    [Fact]
    public void CheckClass_ExcludeInverts() =>
        Assert.False(VmActorDoDamage.CheckClass("A", "A", exclude: true));

    [Fact]
    public void CheckSpecies_ExcludeInverts() =>
        Assert.False(VmActorDoDamage.CheckSpecies("imp", "imp", exclude: true));

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
    public void HealMagnitude_Positive() =>
        Assert.Equal(5, VmActorDoDamage.HealMagnitude(-5));

    [Fact]
    public void UseInflictorDamageType_WhenFlagged() =>
        Assert.True(VmActorDoDamage.UseInflictorDamageType(true, VmActorDoDamage.DmssFlags.InflictorDmgType));

    [Fact]
    public void SetTicsOnPsprite_WhenFound() =>
        Assert.True(VmActorDoDamage.SetTicsOnPsprite(fromPsprite: true, pspriteFound: true));

    [Fact]
    public void SetTicsOnActor_WhenFromActor() =>
        Assert.True(VmActorDoDamage.ShouldSetActorTics(fromActor: true));

    [Fact]
    public void ShouldDamageWhenPresent_Target() =>
        Assert.True(VmActorDoDamage.ShouldDamageWhenPresent(new object()));

    [Fact]
    public void ShouldDamageWhenPresent_TracerAbsent() =>
        Assert.False(VmActorDoDamage.ShouldDamageWhenPresent(null));

    [Fact]
    public void PassesDoDamageFilter_BothRequired() =>
        Assert.False(VmActorDoDamage.PassesDoDamageFilter(false, true, 0));
}
