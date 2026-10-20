namespace HCDE.Playsim.Tests;

public class VmPLnspecDoorsPillarsTests
{
    [Fact]
    public void GenericDoor_LocalLightTagUsesArg0()
    {
        var plan = VmPLnspecDoorsPillars.PlanGenericDoor(7, 32, 128 | 1, 0, 0);
        Assert.Equal(0, plan.Tag);
        Assert.Equal(7, plan.LightTag);
        Assert.Equal(VmPLnspecDoorsPillars.DoorKind.Open, plan.Kind);
    }

    [Fact]
    public void GenericDoor_BoomGenWhenBit64()
    {
        var plan = VmPLnspecDoorsPillars.PlanGenericDoor(1, 2, 64, 0, 0);
        Assert.True(plan.BoomGen);
    }

    [Fact]
    public void LockedRaise_OpensWhenDelayZero()
    {
        Assert.Equal(
            VmPLnspecDoorsPillars.DoorKind.Open,
            VmPLnspecDoorsPillars.LockedRaiseKind(0, 0));
    }

    [Fact]
    public void TextureFloor_LowerUsesNoCrush()
    {
        var call = VmPLnspecDoorsPillars.PackLowerByTexture(1, 8, 2, 3);
        Assert.Equal(-1, call.Crush);
    }

    [Fact]
    public void PillarOpen_PacksFloorAndCeilHeights()
    {
        var call = VmPLnspecDoorsPillars.PackPillarOpen(4, 16, 32, 64);
        Assert.Equal(VmPLnspecDoorsPillars.PillarKind.Open, call.Kind);
        Assert.Equal(32, call.Height);
        Assert.Equal(64, call.Crush);
    }

    [Fact]
    public void Destroy_SectorTagScanWhenTidZero()
    {
        var plan = VmPLnspecDoorsPillars.PlanDestroy(0, 1, 5);
        Assert.True(plan.BySectorTag);
        Assert.True(plan.Extreme);
    }

    [Fact]
    public void ProjectileGravity_SetsFlag()
    {
        var call = VmPLnspecDoorsPillars.PackProjectile(0, 1, 128, 10, 5, true);
        Assert.True(VmPLnspecDoorsPillars.ProjectileUsesGravity(call));
    }
}
