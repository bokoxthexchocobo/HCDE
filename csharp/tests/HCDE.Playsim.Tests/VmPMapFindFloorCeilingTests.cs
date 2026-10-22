namespace HCDE.Playsim.Tests;

public class VmPMapFindFloorCeilingTests
{
    static VmPMapFindFloorCeiling.MapSector? s_portalFloor;

    static double NextCeilingFlat(
        VmPMapFindFloorCeiling.MapSector s,
        double x,
        double y,
        double _,
        double __,
        int ___,
        out VmPMapFindFloorCeiling.MapSector? cs,
        out VmPMapFindFloorCeiling.FFloorRef? ff)
    {
        cs = s;
        ff = null;
        return s.CeilingPlane.ZAtPoint(x, y);
    }

    [Fact]
    public void FindRefPoint_ProjectsOntoSegmentWhenSlopesPresent()
    {
        var line = new VmPMapFindFloorCeiling.MapLine
        {
            FrontSector = new VmPMapFindFloorCeiling.MapSector { FloorPlane = { IsSlope = true } },
            BackSector = new VmPMapFindFloorCeiling.MapSector(),
        };
        line.V1.X = 0;
        line.V1.Y = 0;
        line.V2.X = 10;
        line.V2.Y = 0;
        var p = VmPMapFindFloorCeiling.FindRefPoint(line, 5, 4);
        Assert.Equal(5, p.X);
        Assert.Equal(0, p.Y);
    }

    [Fact]
    public void PitFindFloorCeiling_LowersCeilingFromOpening()
    {
        var tmf = new VmPMapFindFloorCeiling.CheckPosition
        {
            Thing = new VmPMapFindFloorCeiling.MapActor { MaxDropOffHeight = 24 },
            CeilingZ = 256,
            FloorZ = 0,
            DropoffZ = 0,
        };
        var line = new VmPMapFindFloorCeiling.MapLine
        {
            FrontSector = new VmPMapFindFloorCeiling.MapSector(),
            BackSector = new VmPMapFindFloorCeiling.MapSector(),
        };
        var cres = new VmPMapFindFloorCeiling.LineCheckResult { Line = line };
        var mit = new VmPMapFindFloorCeiling.LineIterator();
        VmPMapFindFloorCeiling.PitFindFloorCeiling(
            mit,
            cres,
            mit.Box,
            tmf,
            0,
            (_, _) => true,
            (_, _) => -1,
            (open, _, _, _, _, _) => open.Top = 128);
        Assert.Equal(128, tmf.CeilingZ);
        Assert.True(mit.StopUp);
    }

    [Fact]
    public void PitFindFloorCeiling_RaisesFloorFromOpening()
    {
        var tmf = new VmPMapFindFloorCeiling.CheckPosition
        {
            Thing = new VmPMapFindFloorCeiling.MapActor { MaxDropOffHeight = 24 },
            CeilingZ = 256,
            FloorZ = 0,
            DropoffZ = 0,
        };
        var line = new VmPMapFindFloorCeiling.MapLine
        {
            FrontSector = new VmPMapFindFloorCeiling.MapSector(),
            BackSector = new VmPMapFindFloorCeiling.MapSector(),
        };
        VmPMapFindFloorCeiling.PitFindFloorCeiling(
            new VmPMapFindFloorCeiling.LineIterator(),
            new VmPMapFindFloorCeiling.LineCheckResult { Line = line },
            new VmPMapFindFloorCeiling.BoundingBox(),
            tmf,
            0,
            (_, _) => true,
            (_, _) => -1,
            (open, _, _, _, _, _) => open.Bottom = 64);
        Assert.Equal(64, tmf.FloorZ);
    }

    [Fact]
    public void FindFloorCeiling_UpdatesWhenFloorPortalGroupDiffers()
    {
        var home = new VmPMapFindFloorCeiling.MapSector { PortalGroup = 1 };
        var portalFloor = new VmPMapFindFloorCeiling.MapSector { PortalGroup = 2 };
        portalFloor.FloorPlane.ZAtPoint = (_, _) => 48;
        var actor = new VmPMapFindFloorCeiling.MapActor
        {
            Sector = home,
            Level = new VmPMapFindFloorCeiling.MapLevel { PointInSector = (_, _) => home },
        };
        VmPMapFindFloorCeiling.FindFloorCeiling(
            actor,
            0,
            nextHighestCeilingAt: NextCeilingFlat,
            nextLowestFloorAt: (_, x, y, _, _, _, out var fs, out _, _) =>
            {
                fs = portalFloor;
                return portalFloor.FloorPlane.ZAtPoint(x, y);
            });
        Assert.Equal(48, actor.FloorZ);
        Assert.Same(portalFloor, actor.FloorSector);
    }

    [Fact]
    public void FindFloorCeiling_CopiesHeightsToActor()
    {
        var sector = new VmPMapFindFloorCeiling.MapSector();
        sector.FloorPlane.ZAtPoint = (_, _) => 0;
        sector.CeilingPlane.ZAtPoint = (_, _) => 128;
        var actor = new VmPMapFindFloorCeiling.MapActor
        {
            Sector = sector,
            Level = new VmPMapFindFloorCeiling.MapLevel { PointInSector = (_, _) => sector },
        };
        VmPMapFindFloorCeiling.FindFloorCeiling(actor, 0);
        Assert.Equal(0, actor.FloorZ);
        Assert.Equal(128, actor.CeilingZ);
    }
}
