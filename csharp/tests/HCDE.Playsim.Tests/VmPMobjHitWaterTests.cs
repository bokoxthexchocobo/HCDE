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

    [Fact]
    public void HitWater_WalkFloorPortalDown() =>
        Assert.True(VmPMobjHitWater.WalkDownThroughFloorPortal(
            new VmPMobjHitWater.PortalFloorWalkContext(-1, 0, 1, false)));

    [Fact]
    public void HitWater_Try3dTopSplash() =>
        Assert.True(VmPMobjHitWater.TrySplashAt3dTop(
            new VmPMobjHitWater.HitWater3dProbeContext(
                VmPMobjHitWater.FfExists | VmPMobjHitWater.FfSolid, 0, 10, 10, 0, 0)));

    [Fact]
    public void HitWater_SpawnSmallSplashPath() =>
        Assert.True(VmPMobjHitWater.SpawnSmallSplash(
            new VmPMobjHitWater.SplashSpawnContext(true, true, true, false, true)));

    [Fact]
    public void FloorType_FallsBackToSector() =>
        Assert.Equal(7, VmPMobjHitWater.ResolveThingFloorType(-1, () => 7));

    [Fact]
    public void HitWater_CheckAboveMissileCompareZ() =>
        Assert.True(VmPMobjHitWater.CheckAboveCenter(
            true, 10, VmPMobjHitWater.CompareZForCheckAbove(12, 5, isMissile: true)));

    [Fact]
    public void HitWater_SelectHeightSecTerrain() =>
        Assert.Equal(3, VmPMobjHitWater.SelectFloorTerrain(
            force: false, hasHeightSec: true, moreFlags: 0, () => 1, () => 3));

    [Fact]
    public void HitWater_RejectBelowBottomBlocks() =>
        Assert.True(VmPMobjHitWater.RejectBelowBottom(
            new VmPMobjHitWater.HitWater3dProbeContext(0, 0, 5, 0, 2, 0)));

    [Fact]
    public void HitWater_3dFloorLoopPicksTerrainOnTopPlane()
    {
        var floors = new[]
        {
            new VmPMobjHitWater.F3dFloorTerrainProbe(
                VmPMobjHitWater.FfExists | VmPMobjHitWater.FfSolid, 0, 10, 0, 42),
        };
        var found = VmPMobjHitWater.SelectTerrainFrom3dFloors(10, 0, floors, out var terrain);
        Assert.True(found);
        Assert.Equal(42, terrain);
    }
}
