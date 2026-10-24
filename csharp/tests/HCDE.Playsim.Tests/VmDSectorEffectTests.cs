namespace HCDE.Playsim.Tests;

/// <summary>Parity tests for <c>dsectoreffect.cpp</c> plane movers (see phase-123 MoveFloor/MoveCeiling audits).</summary>
public class VmDSectorEffectTests
{
    private sealed class PlaneD
    {
        public double Value;
    }

    private static VmDSectorEffect.SectorPlanes MakeFloorPlanes(PlaneD floorD)
    {
        var planes = new VmDSectorEffect.SectorPlanes();
        planes.FloorD = floorD.Value;
        planes.FloorGetChangedHeight = h => floorD.Value + h;
        planes.FloorHeightDiff = (from, to) => to - from;
        planes.SetFloorD = d =>
        {
            floorD.Value = d;
            planes.FloorD = d;
        };
        return planes;
    }

    private static VmDSectorEffect.SectorPlanes MakeCeilingPlanes(PlaneD ceilingD)
    {
        var planes = new VmDSectorEffect.SectorPlanes();
        planes.CeilingD = ceilingD.Value;
        planes.CeilingGetChangedHeight = h => ceilingD.Value + h;
        planes.CeilingHeightDiff = (from, to) => to - from;
        planes.SetCeilingD = d =>
        {
            ceilingD.Value = d;
            planes.CeilingD = d;
        };
        return planes;
    }

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

    /// <summary>Downward destination clamp (<c>MoveFloor</c> case -1, <c>movedest &gt;= dest</c>) sets plane to dest and returns pastdest.</summary>
    [Fact]
    public void MoveFloor_DownDestinationClamp_SetsFloorToDestAndPastDest()
    {
        var floorD = new PlaneD { Value = 100 };
        var planes = MakeFloorPlanes(floorD);
        var res = VmDSectorEffect.MoveFloor(
            planes,
            8,
            80,
            10,
            -1,
            false,
            false,
            (_, _, _, _, _, _) => true,
            (_, _, _, _, _) => false,
            _ => { },
            () => { });
        Assert.Equal(VmADoorTickSound.MoveResult.PastDest, res);
        Assert.Equal(80, floorD.Value);
    }

    /// <summary>Native reports pastdest on a destination-clamped down move even when <c>P_ChangeSector</c> rolls the floor back (phase-123 stair/floor endpoint audit).</summary>
    [Fact]
    public void MoveFloor_DownDestinationClamp_PastDestAfterChangeSectorRollback()
    {
        var floorD = new PlaneD { Value = 100 };
        var planes = MakeFloorPlanes(floorD);
        var res = VmDSectorEffect.MoveFloor(
            planes,
            8,
            80,
            10,
            -1,
            false,
            false,
            (_, _, _, _, _, _) => true,
            (_, _, _, _, _) => true,
            _ => { },
            () => { });
        Assert.Equal(VmADoorTickSound.MoveResult.PastDest, res);
        Assert.Equal(100, floorD.Value);
    }

    /// <summary>Incremental down step (<c>movedest &lt; dest</c>) with sector obstruction returns crushed (not pastdest).</summary>
    [Fact]
    public void MoveFloor_IncrementalDown_ObstructionReturnsCrushed()
    {
        var floorD = new PlaneD { Value = 100 };
        var planes = MakeFloorPlanes(floorD);
        var res = VmDSectorEffect.MoveFloor(
            planes,
            8,
            120,
            10,
            -1,
            false,
            false,
            (_, _, _, _, _, _) => true,
            (_, _, _, _, _) => true,
            _ => { },
            () => { });
        Assert.Equal(VmADoorTickSound.MoveResult.Crushed, res);
    }

    /// <summary>Upward destination clamp with <c>crush &lt; 0</c> (stopMoving): blocked sector change rolls back but still pastdest.</summary>
    [Fact]
    public void MoveFloor_UpDestinationClamp_PastDestAfterStopMovingRollback()
    {
        var floorD = new PlaneD { Value = 100 };
        var planes = MakeFloorPlanes(floorD);
        planes.CeilingSlope = false;
        planes.FloorSlope = false;
        planes.PortalLinkedCeiling = false;
        planes.CompatFlags2 = 0;
        var res = VmDSectorEffect.MoveFloor(
            planes,
            4,
            108,
            -1,
            1,
            false,
            false,
            (_, _, _, _, _, _) => true,
            (_, _, _, _, _) => true,
            _ => { },
            () => { });
        Assert.Equal(VmADoorTickSound.MoveResult.PastDest, res);
        Assert.Equal(100, floorD.Value);
    }

    /// <summary>Upward destination clamp (<c>MoveCeiling</c> case 1, <c>movedest &gt;= dest</c>) sets ceiling to dest.</summary>
    [Fact]
    public void MoveCeiling_UpDestinationClamp_SetsCeilingToDestAndPastDest()
    {
        var ceilingD = new PlaneD { Value = 200 };
        var planes = MakeCeilingPlanes(ceilingD);
        var res = VmDSectorEffect.MoveCeiling(
            planes,
            8,
            205,
            10,
            1,
            false,
            (_, _, _, _, _, _) => true,
            (_, _, _, _, _) => false,
            _ => { });
        Assert.Equal(VmADoorTickSound.MoveResult.PastDest, res);
        Assert.Equal(205, ceilingD.Value);
    }

    /// <summary>Closing move (<c>direction -1</c>) destination clamp with stopMoving: pastdest after rollback (door close audit).</summary>
    [Fact]
    public void MoveCeiling_CloseDestinationClamp_PastDestAfterChangeSectorRollback()
    {
        var ceilingD = new PlaneD { Value = 200 };
        var planes = MakeCeilingPlanes(ceilingD);
        planes.CeilingSlope = false;
        planes.FloorSlope = false;
        planes.PortalLinkedFloor = false;
        planes.FloorD = -100;
        planes.CompatFlags2 = 0;
        var res = VmDSectorEffect.MoveCeiling(
            planes,
            24,
            180,
            -1,
            -1,
            false,
            (_, _, _, _, _, _) => true,
            (_, _, _, _, _) => true,
            _ => { });
        Assert.Equal(VmADoorTickSound.MoveResult.PastDest, res);
        Assert.Equal(200, ceilingD.Value);
    }

    /// <summary>Incremental ceiling down step with obstruction returns crushed.</summary>
    [Fact]
    public void MoveCeiling_IncrementalDown_ObstructionReturnsCrushed()
    {
        var ceilingD = new PlaneD { Value = 200 };
        var planes = MakeCeilingPlanes(ceilingD);
        planes.CeilingSlope = false;
        planes.FloorSlope = false;
        planes.PortalLinkedFloor = false;
        planes.FloorD = -100;
        planes.CompatFlags2 = 0;
        var res = VmDSectorEffect.MoveCeiling(
            planes,
            8,
            170,
            10,
            -1,
            false,
            (_, _, _, _, _, _) => true,
            (_, _, _, _, _) => true,
            _ => { });
        Assert.Equal(VmADoorTickSound.MoveResult.Crushed, res);
    }
}
