namespace HCDE.Playsim.Tests;

public class VmAFloorTickElevatorTests
{
    [Fact]
    public void Tick_PastDest_ClearsFloorDataAndDestroys()
    {
        var sec = new VmAFloorTickElevator.FloorSector();
        var floor = new VmAFloorTickElevator.FloorThinker
        {
            Sector = sec,
            Type = VmAFloorTickElevator.GenFloorChg,
            Direction = 1,
            FloorDestDist = 64,
            Speed = 4,
        };
        var destroyed = false;
        VmAFloorTickElevator.Tick(floor, new VmAFloorTickElevator.FloorTickContext
        {
            MoveFloor = (_, _, _, _, _, _, _) => VmADoorTickSound.MoveResult.PastDest,
            Destroy = _ => destroyed = true,
        });
        Assert.True(destroyed);
        Assert.Null(sec.FloorData);
    }

    [Fact]
    public void SetFloorChangeType_ModelZero_SetsGenFloorChg0()
    {
        var sec = new VmAFloorTickElevator.FloorSector { FloorTexture = "FLOOR1" };
        var floor = new VmAFloorTickElevator.FloorThinker { Sector = sec };
        VmAFloorTickElevator.SetFloorChangeType(floor, sec, 1);
        Assert.Equal(VmAFloorTickElevator.GenFloorChg0, floor.Type);
        Assert.Equal("FLOOR1", floor.Texture);
    }

    [Fact]
    public void PlayFloorSound_SilentMove_SkipsSequence()
    {
        var sec = new VmAFloorTickElevator.FloorSector { Flags = VmAFloorTickElevator.SecfSilentmove };
        var called = false;
        VmAFloorTickElevator.PlayFloorSound(sec, new VmAFloorTickElevator.SoundContext
        {
            StartSequenceSound = (_, _, _, _) => called = true,
        });
        Assert.False(called);
    }

    [Fact]
    public void EvDoElevator_CurrentWithoutLine_ReturnsFalse()
    {
        var ok = VmAFloorTickElevator.EvDoElevator(
            new VmAFloorTickElevator.EvDoElevatorContext(),
            null,
            VmAFloorTickElevator.ElevateCurrent,
            4,
            0,
            1);
        Assert.False(ok);
    }

    [Fact]
    public void EvDoElevator_Raise_SetsDestinations()
    {
        var sec = new VmAFloorTickElevator.FloorSector
        {
            CenterFloor = 0,
            CenterCeiling = 128,
            PointToDist = (_, _, z) => z,
            CeilingPointToDist = (_, _, z) => z + 1000,
        };
        VmAFloorTickElevator.ElevatorThinker? created = null;
        VmAFloorTickElevator.EvDoElevator(
            new VmAFloorTickElevator.EvDoElevatorContext
            {
                SectorTagIterator = _ => new[] { 0 },
                SectorByIndex = _ => sec,
                CreateElevator = s =>
                {
                    created = new VmAFloorTickElevator.ElevatorThinker { Sector = s };
                    return created;
                },
            },
            new VmAFloorTickElevator.FloorLine(),
            VmAFloorTickElevator.ElevateRaise,
            4,
            16,
            1);
        Assert.NotNull(created);
        Assert.Equal(16, created!.FloorDestDist);
        Assert.Equal(1144, created.CeilingDestDist);
        Assert.Equal(1, created.Direction);
    }

    [Fact]
    public void EvDoElevator_SkipsSectorWhenFloorAlreadyMoving()
    {
        var sec = new VmAFloorTickElevator.FloorSector();
        var ok = VmAFloorTickElevator.EvDoElevator(
            new VmAFloorTickElevator.EvDoElevatorContext
            {
                SectorTagIterator = _ => new[] { 0 },
                SectorByIndex = _ => sec,
                PlaneMovingFloor = _ => true,
            },
            new VmAFloorTickElevator.FloorLine(),
            VmAFloorTickElevator.ElevateRaise,
            4,
            16,
            1);
        Assert.False(ok);
    }
}
