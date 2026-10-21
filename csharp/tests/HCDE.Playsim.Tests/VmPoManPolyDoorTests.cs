namespace HCDE.Playsim.Tests;

public class VmPoManPolyDoorTests
{
    [Fact]
    public void PolyDoorTick_SlideClosesAndDestroys()
    {
        var destroyed = false;
        var ctx = new VmPoManPolyDoor.PolyDoorTickContext
        {
            DoorType = VmPoManPolyDoor.PodoorSlide,
            Dist = 1,
            TotalDist = 10,
            SpeedV = 2,
            Close = true,
            MovePolyobj = () => true,
            Destroy = () => destroyed = true,
        };
        VmPoManPolyDoor.PolyDoorTick(ctx);
        Assert.True(destroyed);
    }

    [Fact]
    public void PolyDoorTick_WaitsOnTics()
    {
        var ctx = new VmPoManPolyDoor.PolyDoorTickContext { Tics = 2 };
        VmPoManPolyDoor.PolyDoorTick(ctx);
        Assert.Equal(1, ctx.Tics);
    }

    [Fact]
    public void OpenPolyDoor_StartsOnFirstFreeMirror()
    {
        var started = new List<int>();
        var ctx = new VmPoManPolyDoor.OpenPolyDoorContext
        {
            DoorType = VmPoManPolyDoor.PodoorSlide,
            ResolveMirror = id => id == 1 ? new VmPoManPolyDoor.PolyMirrorState { PolyId = 1, Blocked = false, MirrorPolyId = 0 } : null,
            StartDoorOnPoly = (m, _) => started.Add(m.PolyId),
        };
        Assert.True(VmPoManPolyDoor.OpenPolyDoor(ctx, 1));
        Assert.Equal(new[] { 1 }, started);
    }

    [Fact]
    public void OpenPolyDoor_WalksMirrorChainWithoutCycling()
    {
        var started = new List<int>();
        var ctx = new VmPoManPolyDoor.OpenPolyDoorContext
        {
            DoorType = VmPoManPolyDoor.PodoorSlide,
            ResolveMirror = id => id switch
            {
                1 => new VmPoManPolyDoor.PolyMirrorState { PolyId = 1, MirrorPolyId = 2 },
                2 => new VmPoManPolyDoor.PolyMirrorState { PolyId = 2, MirrorPolyId = 1 },
                _ => null,
            },
            StartDoorOnPoly = (m, _) => started.Add(m.PolyId),
        };
        Assert.True(VmPoManPolyDoor.OpenPolyDoor(ctx, 1));
        Assert.Equal(new[] { 1, 2 }, started);
    }

    [Fact]
    public void OpenPolyDoor_AbortsWhenPolyBlocked()
    {
        var ctx = new VmPoManPolyDoor.OpenPolyDoorContext
        {
            ResolveMirror = _ => new VmPoManPolyDoor.PolyMirrorState { PolyId = 1, Blocked = true },
        };
        Assert.False(VmPoManPolyDoor.OpenPolyDoor(ctx, 1));
    }

    [Fact]
    public void StopPoly_InvokesStopWhenSpecialData()
    {
        var stopped = false;
        Assert.True(VmPoManPolyDoor.StopPoly(true, true, () => stopped = true));
        Assert.True(stopped);
    }

    [Fact]
    public void StopPoly_ReturnsFalseWhenPolyMissing()
    {
        Assert.False(VmPoManPolyDoor.StopPoly(false, false, null));
    }
}
