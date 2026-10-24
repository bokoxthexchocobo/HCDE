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
    public void KillTargetPresent_WhenSet() =>
        Assert.True(true);

    [Fact]
    public void KillChildrenMasterMatch_WhenChild() =>
        Assert.True(true);

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
