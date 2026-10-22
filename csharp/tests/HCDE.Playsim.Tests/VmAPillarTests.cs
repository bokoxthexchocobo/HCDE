using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAPillarTests
{
    [Fact]
    public void OnDestroy_DelRefsInterpolators()
    {
        var floor = new object();
        var ceiling = new object();
        var floorDel = false;
        var ceilingDel = false;
        var pillar = new VmAPillar.PillarThinker
        {
            InterpFloor = floor,
            InterpCeiling = ceiling,
            DelRefFloor = o => { if (o == floor) floorDel = true; },
            DelRefCeiling = o => { if (o == ceiling) ceilingDel = true; },
        };
        VmAPillar.OnDestroy(pillar);
        Assert.True(floorDel);
        Assert.True(ceilingDel);
        Assert.Null(pillar.InterpFloor);
        Assert.Null(pillar.InterpCeiling);
    }

    [Fact]
    public void Tick_BothPastDest_StopsAndDestroys()
    {
        var stopped = false;
        var destroyed = false;
        var sec = new VmAPillar.PillarSector();
        var pillar = new VmAPillar.PillarThinker
        {
            Sector = sec,
            Type = VmAPillar.PillarBuild,
            MoveFloor = (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.PastDest,
            MoveCeiling = (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.PastDest,
            StopSequence = _ => stopped = true,
            DestroySelf = () => destroyed = true,
        };
        VmAPillar.Tick(pillar);
        Assert.True(stopped);
        Assert.True(destroyed);
    }

    [Fact]
    public void Construct_BuildType_SplitsSpeedByDistance()
    {
        var sec = new VmAPillar.PillarSector
        {
            CenterFloor = () => 0,
            CenterCeiling = () => 100,
            FloorPointToDist = (_, h) => h,
            CeilingPointToDist = (_, h) => -h,
        };
        var pillar = new VmAPillar.PillarThinker();
        VmAPillar.Construct(pillar, sec, VmAPillar.PillarBuild, 8, 40, 0, 0, false,
            _ => (null, 0), _ => (null, 100), (_, _, _, _) => { });
        Assert.Equal(8 * 40.0 / 60.0, pillar.FloorSpeed, 3);
        Assert.Equal(8, pillar.CeilingSpeed);
    }

    [Fact]
    public void EvDoPillar_SkipsWhenPlaneMoving()
    {
        var created = 0;
        var sec = new VmAPillar.PillarSector { PlaneMovingFloor = () => true };
        var ok = VmAPillar.EvDoPillar(
            VmAPillar.PillarBuild,
            new[] { 0 },
            _ => sec,
            1, 0, 0, 0, false,
            (_, _, _, _, _, _, _) => created++);
        Assert.False(ok);
        Assert.Equal(0, created);
    }

    [Fact]
    public void EvDoPillar_BuildSkipsEqualFloorCeiling()
    {
        var created = 0;
        var sec = new VmAPillar.PillarSector { CenterFloor = () => 64, CenterCeiling = () => 64 };
        VmAPillar.EvDoPillar(VmAPillar.PillarBuild, new[] { 1 }, _ => sec, 1, 0, 0, 0, false,
            (_, _, _, _, _, _, _) => created++);
        Assert.Equal(0, created);
    }
}
