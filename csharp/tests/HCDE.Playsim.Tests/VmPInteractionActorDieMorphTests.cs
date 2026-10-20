namespace HCDE.Playsim.Tests;

public class VmPInteractionActorDieMorphTests
{
    [Fact]
    public void Morph_KillDummyWhenAltPlayerNull() =>
        Assert.True(VmPInteractionActorDieMorph.KillDummyMorph(false, true, true, false));

    [Fact]
    public void Missile_Explodes() =>
        Assert.True(VmPInteractionActorDieMorph.MissileExplodesOnDeath(VmPInteractionActorDieMorph.MfMissile));

    [Fact]
    public void Corpse_WhenMonster() =>
        Assert.True(VmPInteractionActorDieMorph.ApplyCorpseFlag(true, false, false, false));

    [Fact]
    public void Fraglimit_Hits() =>
        Assert.True(VmPInteractionActorDieMorph.FraglimitReached(10, 10));
}
