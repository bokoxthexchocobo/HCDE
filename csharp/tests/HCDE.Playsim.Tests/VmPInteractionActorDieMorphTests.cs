namespace HCDE.Playsim.Tests;

public class VmPInteractionActorDieMorphTests
{
    [Fact]
    public void Morph_KillDummyWhenAltPlayerNull() =>
        Assert.True(VmPInteractionActorDieMorph.KillDummyMorph(true, true, true, false));

    [Fact]
    public void Missile_Explodes() =>
        Assert.True(VmPInteractionActorDieMorph.MissileExplodesOnDeath(VmPInteractionActorDieMorph.MfMissile));

    [Fact]
    public void Corpse_WhenMonster() =>
        Assert.True(VmPInteractionActorDieMorph.ApplyCorpseFlag(true, false, false, false));

    [Fact]
    public void Fraglimit_Hits() =>
        Assert.True(VmPInteractionActorDieMorph.FraglimitReached(10, 10));

    [Fact]
    public void Frags_TeammateDecrements() =>
        Assert.Equal(0, VmPInteractionActorDieMorph.AdjustFragCount(
            new VmPInteractionActorDieMorph.FragSpreeContext(true, false, true, 0, 0, 0, 0, 35), 1));

    [Fact]
    public void Multikill_ResetsAfterGap() =>
        Assert.True(VmPInteractionActorDieMorph.ResetMultikillOnGap(
            new VmPInteractionActorDieMorph.FragSpreeContext(true, false, false, 0, 0, 10, 200, 35)));
}
