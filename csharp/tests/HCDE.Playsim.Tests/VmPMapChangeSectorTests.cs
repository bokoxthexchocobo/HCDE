namespace HCDE.Playsim.Tests;

public class VmPMapChangeSectorTests
{
    [Fact]
    public void Change_AbsMove() =>
        Assert.Equal(3, VmPMapChangeSector.AbsMoveAmount(-3));

    [Fact]
    public void Change_MoveNoblockmapUnlessFlag() =>
        Assert.True(VmPMapChangeSector.ShouldMoveThing(
            VmPMapChangeSector.MfNoblockmap, VmPMapChangeSector.Mf5Movewithsector));

    [Fact]
    public void Change_PickFloorDrop() =>
        Assert.True(VmPMapChangeSector.PickFloorDrop(
            new VmPMapChangeSector.IteratorPickContext(0, -1)));

    [Fact]
    public void Change_MidtexDual() =>
        Assert.True(VmPMapChangeSector.MidtexUsesBothIterators(2));

    [Fact]
    public void Change_SkipInvalidMode() =>
        Assert.True(VmPMapChangeSector.InvalidFloorOrCeil(3));

    [Fact]
    public void Change_FakeFloorUpdate() =>
        Assert.True(VmPMapChangeSector.UpdateFakeFloorSectors(false, false));
}
