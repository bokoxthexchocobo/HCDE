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
    public void Blood_PickStateWhenNoSpray() =>
        Assert.True(VmPMobjSpawnBlood.ShouldPickStateAfterSpray(false));
}
