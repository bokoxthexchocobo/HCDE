namespace HCDE.Playsim.Tests;

public class VmDSectorEffectTests
{
    [Fact]
    public void SectorEffectOnDestroy_ClearsSectorPointers()
    {
        var effect = new object();
        var sector = new VmDSectorEffect.SectorEffectLinks
        {
            FloorData = effect,
            CeilingData = effect,
            LightingData = effect,
        };
        var called = false;
        VmDSectorEffect.SectorEffectOnDestroy(sector, effect, _ => called = true);
        Assert.Null(sector.FloorData);
        Assert.Null(sector.CeilingData);
        Assert.Null(sector.LightingData);
        Assert.True(called);
    }

    [Fact]
    public void MoveAttached_RollsBackOnLinkedSectorFailure()
    {
        var crushed = false;
        var ok = VmDSectorEffect.MoveAttached(
            0,
            4,
            0,
            true,
            false,
            ref crushed,
            (_, _, _, _, _) => true,
            (_, _, _, _, _) => false);
        Assert.False(ok);
        Assert.True(crushed);
    }

    [Fact]
    public void MoveFloor_DownPastDest_ReturnsPastDest()
    {
        double floorD = 100;
        var planes = new VmDSectorEffect.SectorPlanes
        {
            FloorD = floorD,
            FloorGetChangedHeight = h => floorD + h,
            FloorHeightDiff = (_, d) => d - floorD,
            SetFloorD = d => floorD = d,
        };
        planes.FloorD = floorD;

        var res = VmDSectorEffect.MoveFloor(
            planes,
            8,
            80,
            -1,
            -1,
            false,
            false,
            (_, _, _, _, _, s) => { s.Crushed = false; return true; },
            (_, _, _, _, _) => false,
            _ => { },
            () => { });
        Assert.Equal(VmADoorTickSound.MoveResult.PastDest, res);
        Assert.Equal(80, floorD);
    }

    [Fact]
    public void MoveFloor_DownCrush_ReturnsCrushedWhenBlocked()
    {
        double floorD = 100;
        var planes = new VmDSectorEffect.SectorPlanes
        {
            FloorD = floorD,
            FloorGetChangedHeight = h => floorD + h,
            SetFloorD = d => floorD = d,
        };
        var res = VmDSectorEffect.MoveFloor(
            planes,
            8,
            50,
            10,
            -1,
            false,
            false,
            (_, _, _, _, _, s) => { s.Crushed = false; return true; },
            (_, _, _, _, _) => true,
            _ => { },
            () => { });
        Assert.Equal(VmADoorTickSound.MoveResult.Crushed, res);
    }

    [Fact]
    public void MoveCeiling_UpPastDest_ReturnsPastDest()
    {
        double ceilingD = 200;
        var planes = new VmDSectorEffect.SectorPlanes
        {
            CeilingD = ceilingD,
            CeilingGetChangedHeight = h => ceilingD + h,
            CeilingHeightDiff = (_, d) => d - ceilingD,
            SetCeilingD = d => ceilingD = d,
        };
        planes.CeilingD = ceilingD;

        var res = VmDSectorEffect.MoveCeiling(
            planes,
            8,
            240,
            -1,
            1,
            false,
            (_, _, _, _, _, s) => { s.Crushed = false; return true; },
            (_, _, _, _, _) => false,
            _ => { });
        Assert.Equal(VmADoorTickSound.MoveResult.PastDest, res);
        Assert.Equal(240, ceilingD);
    }

    [Fact]
    public void MoveCeiling_DownCrush_ReturnsCrushedWhenBlocked()
    {
        double ceilingD = 200;
        var planes = new VmDSectorEffect.SectorPlanes
        {
            CeilingD = ceilingD,
            CeilingGetChangedHeight = h => ceilingD + h,
            SetCeilingD = d => ceilingD = d,
        };
        var res = VmDSectorEffect.MoveCeiling(
            planes,
            8,
            0,
            10,
            -1,
            false,
            (_, _, _, _, _, s) => { s.Crushed = false; return true; },
            (_, _, _, _, _) => true,
            _ => { });
        Assert.Equal(VmADoorTickSound.MoveResult.Crushed, res);
    }
}
