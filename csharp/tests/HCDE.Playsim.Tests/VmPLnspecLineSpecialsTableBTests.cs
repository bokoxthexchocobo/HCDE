namespace HCDE.Playsim.Tests;

public class VmPLnspecLineSpecialsTableBTests
{
    [Fact]
    public void MapsToCallable_LineSetHealth() =>
        Assert.True(VmPLnspecLineSpecialsTableB.MapsToCallable(150));

    [Fact]
    public void IsNopSlot_TeamScore() =>
        Assert.True(VmPLnspecLineSpecialsTableB.IsNopSlot(152));

    [Fact]
    public void TeleportAlias_Recognized() =>
        Assert.True(VmPLnspecLineSpecialsTableB.TeleportSpecialAlias("TeleportSpecial"));

    [Fact]
    public void IsGenericMoverSlot_CrusherDist() =>
        Assert.True(VmPLnspecLineSpecialsTableB.IsGenericMoverSlot(284));

    [Fact]
    public void BinarySearchStep_Positive() =>
        Assert.Equal(1, VmPLnspecLineSpecialsTableB.BinarySearchStep(5));

    [Fact]
    public void SlotHandlerKind_GenericMover() =>
        Assert.Equal(2, VmPLnspecLineSpecialsTableB.SlotHandlerKind(205));
}
