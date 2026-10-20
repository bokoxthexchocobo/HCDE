namespace HCDE.Playsim.Tests;

public class VmPSpecTestActivateLineTests
{
    [Fact]
    public void RejectFirstSideOnly_WhenBackSide() =>
        Assert.True(VmPSpecTestActivateLine.RejectFirstSideOnly(1, 1));

    [Fact]
    public void PromoteMissileTeleport_WhenCrossTeleport() =>
        Assert.True(VmPSpecTestActivateLine.PromoteMissileTeleport(70, 2, 4, true));

    [Fact]
    public void PredictSpecial_AllowsTeleportLine() =>
        Assert.True(VmPSpecTestActivateLine.PredictSpecialAllowed(215));

    [Fact]
    public void DoorRaiseMonsterPath_SlowSpeed() =>
        Assert.True(VmPSpecTestActivateLine.DoorRaiseMonsterPath(12, 32));

    [Fact]
    public void ExpandAnyCross_AddsCrossBits() =>
        Assert.Equal(6, VmPSpecTestActivateLine.ExpandAnyCross(512));
}
