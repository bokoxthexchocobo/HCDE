namespace HCDE.Playsim.Tests;

public class VmActorDoKillTests
{
    [Fact]
    public void PassesKillFilter_EitherMode() =>
        Assert.True(VmActorDoKill.PassesKillFilter(false, true, VmActorDoKill.KilsFlags.Either));

    [Fact]
    public void MissileKillAllowed_RespectsFoilInvul() =>
        Assert.True(VmActorDoKill.MissileKillAllowed(true, false, false, VmActorDoKill.KilsFlags.FoilInvul));

    [Fact]
    public void DamageSiblingsSameMaster_RequiresDistinctActor() =>
        Assert.True(VmActorDoKill.DamageSiblingsSameMaster(true, true, true));

    [Fact]
    public void KillSiblingsLoop_MatchesNativeGuard() =>
        Assert.False(VmActorDoKill.KillSiblingsLoop(false, true, true));

    [Fact]
    public void ShouldKillWhenPresent_Target() =>
        Assert.True(VmActorDoKill.ShouldKillWhenPresent(new object()));

    [Fact]
    public void IsKillChildOfMaster_WhenMasterMatches() {
        var self = new object();
        Assert.True(VmActorDoKill.IsKillChildOfMaster(self, self));
    }

    [Fact]
    public void ShouldKillMaster_WhenSet() =>
        Assert.True(VmActorDoKill.ShouldKillMaster(new object()));

    [Fact]
    public void CombineKillBaseFlags_IncludesFoilInvul() =>
        Assert.Equal(13, VmActorDoKill.CombineKillBaseFlags(VmActorDoKill.KilsFlags.FoilInvul));

    [Fact]
    public void ExplodePathApplies_WhenMissileAllowed() =>
        Assert.True(VmActorDoKill.ExplodePathApplies(true, VmActorDoKill.KilsFlags.KillMissiles, true));

    [Fact]
    public void PlanDoKill_ExplodeAndDamage_WhenMissileAndMonstersAllowed()
    {
        var plan = VmActorDoKill.PlanDoKill(
            passesFilter: true,
            isMissile: true,
            invulnerable: false,
            buddha: false,
            noDamage: false,
            VmActorDoKill.KilsFlags.KillMissiles);
        Assert.Equal(VmActorDoKill.KillPlan.ExplodeAndDamage, plan);
    }
}
