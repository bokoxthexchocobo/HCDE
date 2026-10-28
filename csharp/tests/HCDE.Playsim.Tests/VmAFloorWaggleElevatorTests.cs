namespace HCDE.Playsim.Tests;

public class VmAFloorWaggleElevatorTests
{
    [Fact]
    public void EvStartWaggle_Floor_SetsExpandState()
    {
        VmAFloorWaggleElevator.WaggleThinker? created = null;
        var sec = new VmAFloorWaggleElevator.FloorSector { FloorPlaneD = 128 };
        var ok = VmAFloorWaggleElevator.EvStartWaggle(
            new VmAFloorWaggleElevator.StartWaggleContext
            {
                SectorTagIterator = _ => new[] { 0 },
                SectorByIndex = _ => sec,
                CreateFloorWaggle = s =>
                {
                    created = new VmAFloorWaggleElevator.WaggleThinker { Sector = s };
                    VmAFloorWaggleElevator.FloorWaggleConstruct(created, new object());
                    return created;
                },
            },
            1,
            64,
            32,
            0,
            0,
            false);
        Assert.True(ok);
        Assert.NotNull(created);
        Assert.Equal(VmAFloorWaggleElevator.WglstateExpand, created!.State);
        Assert.Equal(128, created.OriginalDist);
        Assert.Equal(sec.FloorData, created);
    }

    [Fact]
    public void EvStartWaggle_SkipsWhenPlaneMoving()
    {
        Assert.False(VmAFloorWaggleElevator.EvStartWaggle(
            new VmAFloorWaggleElevator.StartWaggleContext
            {
                SectorTagIterator = _ => new[] { 0 },
                SectorByIndex = _ => new VmAFloorWaggleElevator.FloorSector { PlaneMovingFloor = () => true },
            },
            1,
            64,
            32,
            0,
            0,
            false));
    }

    [Fact]
    public void DoWaggle_ExpandReachesStable()
    {
        var w = new VmAFloorWaggleElevator.WaggleThinker
        {
            Sector = new VmAFloorWaggleElevator.FloorSector(),
            TargetScale = 4,
            ScaleDelta = 2,
            State = VmAFloorWaggleElevator.WglstateExpand,
        };
        var tick = new VmAFloorWaggleElevator.WaggleTickContext
        {
            BobSin = _ => 0,
            GetPlaneD = (_, _) => 0,
            SetPlaneD = (_, _) => { },
            HeightDiff = (_, _) => 0,
            ChangePlaneTexZ = (_, _) => { },
            Scroll3dMidtex = (_, _, _, _) => { },
            MoveLinkedSectors = (_, _, _, _) => { },
            ChangeSector = (_, _, _, _, _) => { },
        };
        VmAFloorWaggleElevator.DoWaggle(w, false, tick);
        VmAFloorWaggleElevator.DoWaggle(w, false, tick);
        Assert.Equal(VmAFloorWaggleElevator.WglstateStable, w.State);
        Assert.Equal(4, w.Scale);
    }

    [Fact]
    public void WaggleBaseSerialize_WritesFields()
    {
        var keys = new List<string>();
        var w = new VmAFloorWaggleElevator.WaggleThinker
        {
            Sector = new VmAFloorWaggleElevator.FloorSector(),
            State = VmAFloorWaggleElevator.WglstateExpand,
        };
        VmAFloorWaggleElevator.WaggleBaseSerialize(w, (k, _) => keys.Add(k));
        Assert.Contains("state", keys);
        Assert.Contains("originaldist", keys);
    }

    [Fact]
    public void ElevatorSerialize_WritesFields()
    {
        var keys = new List<string>();
        var elev = new VmAFloorWaggleElevator.ElevatorThinker
        {
            Sector = new VmAFloorWaggleElevator.FloorSector(),
            Type = 1,
            Speed = 8,
        };
        VmAFloorWaggleElevator.ElevatorSerialize(elev, (k, _) => keys.Add(k));
        Assert.Contains("speed", keys);
        Assert.Contains("interp_floor", keys);
    }

    [Fact]
    public void DoWaggle_ReduceDestroysThinker()
    {
        var destroyed = false;
        var w = new VmAFloorWaggleElevator.WaggleThinker
        {
            Sector = new VmAFloorWaggleElevator.FloorSector(),
            State = VmAFloorWaggleElevator.WglstateReduce,
            Scale = 0.5,
            ScaleDelta = 1,
            OriginalDist = 64,
        };
        VmAFloorWaggleElevator.DoWaggle(w, false, new VmAFloorWaggleElevator.WaggleTickContext
        {
            GetPlaneD = (_, _) => 64,
            SetPlaneD = (_, _) => { },
            HeightDiff = (_, _) => 0,
            ChangePlaneTexZ = (_, _) => { },
            Scroll3dMidtex = (_, _, _, _) => { },
            MoveLinkedSectors = (_, _, _, _) => { },
            ChangeSector = (_, _, _, _, _) => { },
            Destroy = _ => destroyed = true,
        });
        Assert.True(destroyed);
        Assert.True(w.Destroyed);
    }

    [Fact]
    public void ElevatorOnDestroy_DropsInterpRefs()
    {
        var floor = new object();
        var ceiling = new object();
        var elev = new VmAFloorWaggleElevator.ElevatorThinker
        {
            Sector = new VmAFloorWaggleElevator.FloorSector(),
            InterpFloor = floor,
            InterpCeiling = ceiling,
        };
        var refs = new List<object>();
        VmAFloorWaggleElevator.ElevatorOnDestroy(elev, new VmAFloorWaggleElevator.ElevatorLifecycleContext
        {
            DelRef = o => { if (o != null) refs.Add(o); },
        });
        Assert.Equal(2, refs.Count);
        Assert.Null(elev.InterpFloor);
        Assert.Null(elev.InterpCeiling);
    }
}
