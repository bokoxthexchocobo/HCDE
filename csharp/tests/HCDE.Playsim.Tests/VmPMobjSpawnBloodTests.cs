namespace HCDE.Playsim.Tests;

public class VmPMobjSpawnBloodTests
{
    [Fact]
    public void Blood_DisablesParticlesWhenFlagClear() =>
        Assert.True(VmPMobjSpawnBlood.ForceBloodTypeOff(0));

    [Fact]
    public void Blood_StrifeSprayWhenHighDamage() =>
        Assert.True(VmPMobjSpawnBlood.StrifeSprayState(VmPMobjSpawnBlood.GameStrife, 20, true));

    [Fact]
    public void Blood_DamageAdvanceMedium() =>
        Assert.Equal(1, VmPMobjSpawnBlood.DamageStateAdvance(10));

    [Fact]
    public void Blood_HidesForModernBloodType() =>
        Assert.True(VmPMobjSpawnBlood.HideActorForBloodType(2));

    [Fact]
    public void Splatter_OppositeAngle() =>
        Assert.Equal(0, VmPMobjSpawnBlood.OppositeHitAngle(180));

    [Fact]
    public void Ripper_HereticFloats() =>
        Assert.True(VmPMobjSpawnBlood.RipperHereticNoGravity(VmPMobjSpawnBlood.GameHeretic));

    [Fact]
    public void StateWalk_NeedsOwnedFrames() =>
        Assert.True(VmPMobjSpawnBlood.TryAdvanceOwnedState(
            new VmPMobjSpawnBlood.BloodStateWalkContext(1, true, true)));

    [Fact]
    public void Ripper_SplashAngleAdds180() =>
        Assert.Equal(200, VmPMobjSpawnBlood.RipperSplashAngle(
            new VmPMobjSpawnBlood.RipperSplashContext(1, 20)));

    [Fact]
    public void PlanBloodDamageState_UsesSprayWhenFound()
    {
        var pick = VmPMobjSpawnBlood.PlanBloodDamageState(20, VmPMobjSpawnBlood.GameStrife, true);
        Assert.True(pick.UseSprayState);
    }

    [Fact]
    public void Blood_SelectAdvanceSkipsSpray() =>
        Assert.Equal(-1, VmPMobjSpawnBlood.SelectBloodStateAdvance(
            20, VmPMobjSpawnBlood.GameStrife, true));

    [Fact]
    public void Blood_ContinueWalkWhileAdvanceLeft() =>
        Assert.True(VmPMobjSpawnBlood.ContinueParentWalk(
            new VmPMobjSpawnBlood.ParentStateWalkStep(false, 2, true, true)));

    [Fact]
    public void WalkParentChain_FindsOwnedAdvance()
    {
        var chain = new[]
        {
            new VmPMobjSpawnBlood.BloodClassStateNode
            {
                OwnsSpawnState = true,
                OwnsSpawnOffset = off => off == 2,
            },
        };
        Assert.Equal(2, VmPMobjSpawnBlood.WalkParentChainForBloodAdvance(chain, 2));
    }

    [Fact]
    public void PlanBloodSplatter_OppositeSplashAngle()
    {
        var plan = VmPMobjSpawnBlood.PlanBloodSplatter(2, 0, 0, 64, -64, 90, 0);
        Assert.Equal(1, plan.VelX);
        Assert.Equal(-1, plan.VelY);
        Assert.Equal(-90, plan.SplashAngleDeg);
    }

    [Fact]
    public void ResolveBloodTypeCvar_ClearsWhenParticlesDisabled() =>
        Assert.Equal(0, VmPMobjSpawnBlood.ResolveBloodTypeCvar(2, 0));
}
