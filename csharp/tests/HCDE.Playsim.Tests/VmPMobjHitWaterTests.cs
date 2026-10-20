namespace HCDE.Playsim.Tests;

public class VmPMobjHitWaterTests
{
    [Fact]
    public void FloorType_UsesActorTerrain() =>
        Assert.True(VmPMobjHitWater.UseActorFloorTerrain(2));

    [Fact]
    public void HitWater_SkipsPredictingPlayer() =>
        Assert.True(VmPMobjHitWater.SkipWhenPredicting(true, VmPMobjHitWater.CfPredicting));

    [Fact]
    public void HitWater_3dFloorSplashable() =>
        Assert.True(VmPMobjHitWater.Splashable3dTop(
            VmPMobjHitWater.FfExists | VmPMobjHitWater.FfSolid, 0));

    [Fact]
    public void HitWater_NoSplashWhenUndefined() =>
        Assert.True(VmPMobjHitWater.NoSplashDefinition(-1));

    [Fact]
    public void HitWater_SmallMassSplash() =>
        Assert.True(VmPMobjHitWater.SmallSplashMass(0, 5));

    [Fact]
    public void HitWater_AllowsWhenNotDontSplash() =>
        Assert.True(VmPMobjHitWater.AllowSplash(0));
}
