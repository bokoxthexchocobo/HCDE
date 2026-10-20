namespace HCDE.Playsim.Tests;

public class VmPMapPitCheckThingTests
{
    [Fact]
    public void SkipSelf_WhenSame()
    {
        Assert.True(VmPMapPitCheckThing.SkipSelf(new AActorRef(1), new AActorRef(1)));
        Assert.False(VmPMapPitCheckThing.SkipSelf(new AActorRef(1), new AActorRef(2)));
    }

    [Fact]
    public void TouchyDetonate_WhenAligned() =>
        Assert.True(VmPMapPitCheckThing.TouchyDetonate(true, true, 1, true, true, true));

    [Fact]
    public void RipperSkipsCorpse() =>
        Assert.True(VmPMapPitCheckThing.RipperSkipsCorpse(true, true, false));

    [Fact]
    public void ProjectileClipHeight_UsesPassHeight() =>
        Assert.Equal(24, VmPMapPitCheckThing.ProjectileClipHeight(24, false, 40));

    [Fact]
    public void SolidBlock_MirrorBlocksPlayer() =>
        Assert.True(VmPMapPitCheckThing.SolidBlock(false, true, false, true));

    [Fact]
    public void EarlyGates_SkipWhenNonInteractive()
    {
        var ctx = new VmPMapPitCheckThing.PitEarlyGateContext(
            false, false, false, false, false, false, true, false, false);
        Assert.Equal(VmPMapPitCheckThing.PitTraversalOutcome.KeepGoing,
            VmPMapPitCheckThing.EvaluateEarlyGates(ctx));
    }

    [Fact]
    public void MissilePath_RipCorpsePass()
    {
        var ctx = new VmPMapPitCheckThing.PitMissilePathContext(
            true, 0, true, false, false, false, false, false, true,
            false, 0, 40, 0, 32, 0, false, false, false, 2, true, true, false, false,
            false, false, false, true, false, false, false, false, false, false);
        Assert.Equal(VmPMapPitCheckThing.PitTraversalOutcome.StopPass,
            VmPMapPitCheckThing.EvaluateMissilePath(ctx));
    }

    [Fact]
    public void SolidPickup_UnblockPasses()
    {
        var ctx = new VmPMapPitCheckThing.PitSolidPickupContext(
            false, true, false, true, false, false, false, 0, 0, true);
        Assert.Equal(VmPMapPitCheckThing.PitTraversalOutcome.StopPass,
            VmPMapPitCheckThing.EvaluateSolidAndPickup(ctx));
    }
}
