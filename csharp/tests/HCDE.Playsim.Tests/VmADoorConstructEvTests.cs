namespace HCDE.Playsim.Tests;

public class VmADoorConstructEvTests
{
    [Fact]
    public void Construct_DoorRaise_SetsDirectionAndTopDist()
    {
        var sec = new VmADoorTickSound.DoorSector
        {
            CeilingPlaneD = 100,
        };
        var door = new VmADoorTickSound.DoorThinker { Sector = sec };
        var sounded = false;
        VmADoorConstructEv.Construct(door, sec, VmADoorConstructEv.DoorRaise, 8, 30, 5, 0, new VmADoorConstructEv.DoorConstructContext
        {
            FindLowestCeilingSurrounding = (_, _) => 200,
            FindHighestFloorPoint = (_, _) => 0,
            PointToDist = (_, _, z) => z + 10,
            Tick = new VmADoorTickSound.DoorTickContext
            {
                StartSequenceSound = (_, _, _, _) => sounded = true,
            },
        });
        Assert.Equal(1, door.Direction);
        Assert.Equal(206, door.TopDist);
        Assert.True(sounded);
    }

    [Fact]
    public void Construct_DoorClose_ClearsLightTagWithCompat()
    {
        var sec = new VmADoorTickSound.DoorSector();
        var door = new VmADoorTickSound.DoorThinker { Sector = sec };
        VmADoorConstructEv.Construct(door, sec, VmADoorConstructEv.DoorClose, 4, 0, 9, 0, new VmADoorConstructEv.DoorConstructContext
        {
            CompatFlags = VmADoorConstructEv.CompatNoDoorLight,
            FindLowestCeilingSurrounding = (_, _) => 64,
            FindHighestFloorPoint = (_, _) => 0,
            PointToDist = (_, _, z) => z,
        });
        Assert.Equal(-1, door.Direction);
        Assert.Equal(0, door.LightTag);
    }

    [Fact]
    public void Construct_DoorOpen_SkipsSoundWhenAlreadyAtTop()
    {
        var sec = new VmADoorTickSound.DoorSector { CeilingPlaneD = 50 };
        var door = new VmADoorTickSound.DoorThinker { Sector = sec };
        var sounded = false;
        VmADoorConstructEv.Construct(door, sec, VmADoorConstructEv.DoorOpen, 4, 0, 0, 0, new VmADoorConstructEv.DoorConstructContext
        {
            FindLowestCeilingSurrounding = (_, _) => 54,
            FindHighestFloorPoint = (_, _) => 0,
            PointToDist = (_, _, _) => 50,
            Tick = new VmADoorTickSound.DoorTickContext
            {
                StartSequenceSound = (_, _, _, _) => sounded = true,
            },
        });
        Assert.False(sounded);
    }

    [Fact]
    public void EvDoDoor_LockFailsWithoutKeys()
    {
        var ok = VmADoorConstructEv.EvDoDoor(new VmADoorConstructEv.EvDoDoorContext
        {
            CheckKeys = (_, _, _) => false,
        }, VmADoorConstructEv.DoorOpen, new VmADoorConstructEv.EvLine(), null, 1, 4, 0, 5, 0, false, 0);
        Assert.False(ok);
    }

    [Fact]
    public void EvDoDoor_ManualTag_CreatesThinker()
    {
        var sec = new VmADoorTickSound.DoorSector { SectorNum = 2 };
        var line = new VmADoorConstructEv.EvLine
        {
            Side1 = new VmADoorConstructEv.EvSide { Sector = sec },
        };
        var created = false;
        var ok = VmADoorConstructEv.EvDoDoor(new VmADoorConstructEv.EvDoDoorContext
        {
            PlaneMovingCeiling = _ => false,
            CreateDoorThinker = (s, t, sp, d, lt, tc) =>
            {
                created = ReferenceEquals(s, sec) && t == VmADoorConstructEv.DoorOpen;
                return true;
            },
        }, VmADoorConstructEv.DoorOpen, line, null, 0, 4, 0, 0, 0, false, 0);
        Assert.True(ok);
        Assert.True(created);
    }

    [Fact]
    public void EvDoDoor_RaiseDoorReversal_ChangesDirection()
    {
        var sec = new VmADoorTickSound.DoorSector();
        var door = new VmADoorTickSound.DoorThinker
        {
            Sector = sec,
            Type = VmADoorConstructEv.DoorRaise,
            Direction = -1,
        };
        var line = new VmADoorConstructEv.EvLine { Side1 = new VmADoorConstructEv.EvSide { Sector = sec } };
        var ok = VmADoorConstructEv.EvDoDoor(new VmADoorConstructEv.EvDoDoorContext
        {
            PlaneMovingCeiling = _ => true,
            GetCeilingDoor = _ => door,
        }, VmADoorConstructEv.DoorRaise, line, new VmADoorConstructEv.EvThing { Player = new VmADoorConstructEv.EvPlayer() },
            0, 4, 0, 0, 0, false, 0);
        Assert.False(ok);
        Assert.Equal(1, door.Direction);
    }
}
