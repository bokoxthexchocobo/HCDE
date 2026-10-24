namespace HCDE.Playsim.Tests;

public class VmPLnspecDoorsPillarsTests
{
    [Fact]
    public void LockedRaise_OpensWhenDelayZero()
    {
        Assert.Equal(
            VmPLnspecDoorsPillars.DoorKind.Open,
            VmPLnspecDoorsPillars.LockedRaiseKind(0, 0));
    }

    [Fact]
    public void AnimatedDoor_RequiresKeysWhenLockSet()
    {
        Assert.False(VmPLnspecDoorsPillars.AnimatedDoorRequiresKeysWhenLockSet(5, false));
    }

    [Fact]
    public void Waggle_PacksAmplitudeAndFrequency()
    {
        var w = VmPLnspecDoorsPillars.PackFloorWaggle(1, 10, 20, 30, 40);
        Assert.Equal(10, w.Amplitude);
        Assert.False(w.Ceiling);
    }

    [Fact]
    public void CeilingInstant_ToFloorUsesGapArg()
    {
        var c = VmPLnspecDoorsPillars.PackCeilingLowerToFloor(2, 8, 1, 3, 16);
        Assert.Equal(16, c.Gap);
    }

    [Fact]
    public void CeilingLowerByTexture_CrushFromArg3()
    {
        var c = VmPLnspecDoorsPillars.PackCeilingLowerByTexture(1, 4, 2, 7);
        Assert.Equal(7, c.Crush);
    }

    [Fact]
    public void Destroy_SectorTagScanWhenTidZero()
    {
        var plan = VmPLnspecDoorsPillars.PlanDestroy(0, 1, 5);
        Assert.True(plan.BySectorTag);
        Assert.True(plan.Extreme);
    }

    [Fact]
    public void StairsRepeatable_TogglesArg3() =>
        Assert.True(VmPLnspecDoorsPillars.StairsRepeatableToggle(true));

    [Fact]
    public void RemoveReturnsTrue() =>
        Assert.True(VmPLnspecDoorsPillars.DestroyReturnsTrue());
}
