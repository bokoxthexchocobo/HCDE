namespace HCDE.Playsim.Tests;

public class VmPLnspecGenericDoorFloorPillarTests
{
    [Fact]
    public void GenericDoor_InvalidKindReturnsFalse()
    {
        Assert.False(VmPLnspecGenericDoorFloorPillar.GenericDoor(1, 8, 99, 0, 0, new()));
    }

    [Fact]
    public void GenericDoor_RaiseInvokesEvDoDoor()
    {
        var kind = (VmPLnspecGenericDoorFloorPillar.DoorKind)(-1);
        VmPLnspecGenericDoorFloorPillar.GenericDoor(5, 16, 0, 0, 0, new VmPLnspecGenericDoorFloorPillar.LineContext
        {
            EvDoDoor = (k, tag, _, _, _, _, boom) => { kind = k; return tag == 5; },
        });
        Assert.Equal(VmPLnspecGenericDoorFloorPillar.DoorKind.Raise, kind);
    }

    [Fact]
    public void GenericFloor_RaiseByValueTarget()
    {
        var called = false;
        VmPLnspecGenericDoorFloorPillar.GenericFloor(1, 8, 4, 0, 8, new VmPLnspecGenericDoorFloorPillar.LineContext
        {
            EvDoFloor = (_, tag, _, _, _, _, _) => { called = tag == 1; return true; },
        });
        Assert.True(called);
    }

    [Fact]
    public void PillarOpen_InvokesEvDoPillar()
    {
        Assert.True(VmPLnspecGenericDoorFloorPillar.PillarOpen(2, 8, 10, 20, new VmPLnspecGenericDoorFloorPillar.LineContext
        {
            EvDoPillar = (kind, tag, _, fh, ch, _, _) => kind == 1 && tag == 2 && fh == 10 && ch == 20,
        }));
    }
}
