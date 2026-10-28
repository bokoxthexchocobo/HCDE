namespace HCDE.Playsim.Tests;

public class VmPLnspecDoorFloorLineSpecialsTests
{
    [Fact]
    public void DoorLockedRaise_OpensWhenDelayZero()
    {
        var op = (VmPLnspecDoorFloorLineSpecials.DoorOp)(-1);
        VmPLnspecDoorFloorLineSpecials.DoorLockedRaise(1, 8, 0, 0, 0, new VmPLnspecDoorFloorLineSpecials.LineContext
        {
            EvDoDoor = (kind, _, _, _, _, _, _, _) => { op = kind; return true; },
        });
        Assert.Equal(VmPLnspecDoorFloorLineSpecials.DoorOp.Open, op);
    }

    [Fact]
    public void GenericDoor_InvalidKindReturnsFalse()
    {
        Assert.False(VmPLnspecDoorFloorLineSpecials.GenericDoor(1, 8, 99, 0, 0, new()));
    }

    [Fact]
    public void GenericDoor_LocalLightTagSwapsTag()
    {
        var lightTag = -1;
        VmPLnspecDoorFloorLineSpecials.GenericDoor(7, 8, 128, 0, 0, new VmPLnspecDoorFloorLineSpecials.LineContext
        {
            EvDoDoor = (_, tag, _, _, _, lt, _, _) => { lightTag = lt; return tag == 0; },
        });
        Assert.Equal(7, lightTag);
    }

    [Fact]
    public void DoorAnimated_BlocksWithoutKeys()
    {
        Assert.False(VmPLnspecDoorFloorLineSpecials.DoorAnimated(0, 8, 0, 5, false, new()));
    }

    [Fact]
    public void FloorMoveToValue_NegativeHeight()
    {
        var height = 0;
        VmPLnspecDoorFloorLineSpecials.FloorMoveToValue(1, 8, 4, true, 0, new VmPLnspecDoorFloorLineSpecials.LineContext
        {
            EvDoFloor = (_, _, _, h, _, _, _) => { height = h; return true; },
        });
        Assert.Equal(-4, height);
    }

    [Fact]
    public void FloorLowerToHighest_AdjustMinus128()
    {
        var height = 0;
        VmPLnspecDoorFloorLineSpecials.FloorLowerToHighest(1, 8, 200, false, new VmPLnspecDoorFloorLineSpecials.LineContext
        {
            EvDoFloor = (_, _, _, h, _, _, _) => { height = h; return true; },
        });
        Assert.Equal(72, height);
    }
}
