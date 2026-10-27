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

    [Fact]
    public void CreateFloor_ModelChange_InvokesSetFloorChangeType()
    {
        var sec = new VmAFloorCreate.FloorSector
        {
            FloorPlaneD = 0,
            CenterFloor = (_, _) => 0,
            PointToDist = (_, _, z) => z,
        };
        var model = new VmAFloorCreate.FloorSector { FloorTexture = "M1" };
        VmAFloorCreate.FloorThinker? floor = null;
        VmAFloorCreate.CreateFloor(
            new VmAFloorCreate.CreateFloorContext
            {
                CreateThinker = s =>
                {
                    floor = new VmAFloorCreate.FloorThinker { Sector = s };
                    return floor;
                },
                FindModelFloorSector = (_, _) => model,
                SetFloorChangeType = VmAFloorDfloorMethods918Slice.SetFloorChangeType,
            },
            sec,
            VmAFloorCreate.FloorRaiseByValue,
            null,
            4,
            8,
            0,
            7,
            false,
            false);
        Assert.Equal(VmAFloorDfloorMethods918Slice.GenFloorChgT, floor!.Type);
        Assert.Equal("M1", floor.Texture);
    }

    [Fact]
    public void CreateFloor_LowerAndChange_UsesModelSectorTexture()
    {
        var sec = new VmAFloorCreate.FloorSector
        {
            FloorPlaneD = 0,
            FloorTexture = "OLD",
            Special = new VmAFloorTickElevator.SectorSpecial { Tag = 1 },
            PointToDist = (_, _, z) => z,
        };
        var model = new VmAFloorCreate.FloorSector { FloorTexture = "NEW", Special = new VmAFloorTickElevator.SectorSpecial { Tag = 9 } };
        VmAFloorCreate.FloorThinker? floor = null;
        VmAFloorCreate.CreateFloor(
            new VmAFloorCreate.CreateFloorContext
            {
                CreateThinker = s =>
                {
                    floor = new VmAFloorCreate.FloorThinker { Sector = s };
                    return floor;
                },
                FindLowestFloorSurrounding = (_, _) => 32,
                FindModelFloorSector = (_, _) => model,
            },
            sec,
            VmAFloorCreate.FloorLowerAndChange,
            null,
            4,
            0,
            0,
            0,
            false,
            false);
        Assert.Equal("NEW", floor!.Texture);
        Assert.Equal(9, floor.NewSpecial.Tag);
        Assert.Equal(-1, floor.Direction);
    }

    [Fact]
    public void CreateFloor_InstantMove_SetsSilentWhenCompatAllows()
    {
        var sec = new VmAFloorCreate.FloorSector
        {
            FloorPlaneD = 0,
            CenterFloor = (_, _) => 0,
            PointToDist = (_, _, z) => z,
        };
        var sounded = false;
        VmAFloorCreate.CreateFloor(
            new VmAFloorCreate.CreateFloorContext
            {
                CreateThinker = s => new VmAFloorCreate.FloorThinker { Sector = s },
                StopInterpolation = (_, _) => { },
                StartFloorSound = _ => sounded = true,
            },
            sec,
            VmAFloorCreate.FloorRaiseByValue,
            null,
            100,
            8,
            0,
            0,
            false,
            false);
        Assert.False(sounded);
        Assert.True(VmAFloorCreate.CreateFloor(
            new VmAFloorCreate.CreateFloorContext
            {
                CompatFlags = VmAFloorCreate.CompatfSilentInstantFloors,
                CreateThinker = s => new VmAFloorCreate.FloorThinker { Sector = s },
                StopInterpolation = (_, _) => { },
                StartFloorSound = _ => sounded = true,
            },
            sec,
            VmAFloorCreate.FloorRaiseByValue,
            null,
            100,
            8,
            0,
            0,
            false,
            false));
        Assert.True(sounded);
    }
}

