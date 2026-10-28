namespace HCDE.Playsim.Tests;

public class VmPLnspecLineSpecialsTableTests
{
    [Fact]
    public void IsNopSlot_Index0() =>
        Assert.True(VmPLnspecLineSpecialsTable.IsNopSlot(0));

    [Fact]
    public void IsPolyobjSlot_RotateLeft() =>
        Assert.True(VmPLnspecLineSpecialsTable.IsPolyobjSlot(2));

    [Fact]
    public void IsTeleportSlot_ZombieChanger() =>
        Assert.True(VmPLnspecLineSpecialsTable.IsTeleportSlot(39));

    [Fact]
    public void MapsToCallable_DoorOpen() =>
        Assert.True(VmPLnspecLineSpecialsTable.MapsToCallable(11));

    [Fact]
    public void SlotInRange_Max149() =>
        Assert.True(VmPLnspecLineSpecialsTable.SlotInRange(149));
}
