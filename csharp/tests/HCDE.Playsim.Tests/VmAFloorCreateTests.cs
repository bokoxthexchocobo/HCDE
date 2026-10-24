namespace HCDE.Playsim.Tests;

public class VmAFloorCreateTests
{
    [Fact]
    public void CreateFloor_RaiseByValue_SetsUpDirection()
    {
        var sec = new VmAFloorCreate.FloorSector
        {
            FloorPlaneD = 0,
            CenterFloor = (_, _) => 0,
            PointToDist = (_, _, z) => z,
        };
        VmAFloorCreate.FloorThinker? floor = null;
        var ok = VmAFloorCreate.CreateFloor(
            new VmAFloorCreate.CreateFloorContext
            {
                CreateThinker = s =>
                {
                    floor = new VmAFloorCreate.FloorThinker { Sector = s };
                    return floor;
                },
            },
            sec,
            VmAFloorCreate.FloorRaiseByValue,
            null,
            4,
            32,
            0,
            0,
            false,
            false);
        Assert.True(ok);
        Assert.Equal(1, floor!.Direction);
        Assert.Equal(32, floor.FloorDestDist);
    }

    [Fact]
    public void CreateFloor_LowerByValue_SetsDownDirection()
    {
        var sec = new VmAFloorCreate.FloorSector
        {
            FloorPlaneD = 64,
            CenterFloor = (_, _) => 64,
            PointToDist = (_, _, z) => z,
        };
        VmAFloorCreate.FloorThinker? floor = null;
        VmAFloorCreate.CreateFloor(
            new VmAFloorCreate.CreateFloorContext
            {
                CreateThinker = s =>
                {
                    floor = new VmAFloorCreate.FloorThinker { Sector = s };
                    return floor;
                },
            },
            sec,
            VmAFloorCreate.FloorLowerByValue,
            null,
            4,
            16,
            0,
            0,
            false,
            false);
        Assert.Equal(-1, floor!.Direction);
        Assert.Equal(48, floor.FloorDestDist);
    }

    [Fact]
    public void CreateFloor_SkipsWhenPlaneMoving()
    {
        var ok = VmAFloorCreate.CreateFloor(
            new VmAFloorCreate.CreateFloorContext { PlaneMovingFloor = _ => true },
            new VmAFloorCreate.FloorSector(),
            VmAFloorCreate.FloorRaiseByValue,
            null,
            4,
            8,
            0,
            0,
            false,
            false);
        Assert.False(ok);
    }
}
