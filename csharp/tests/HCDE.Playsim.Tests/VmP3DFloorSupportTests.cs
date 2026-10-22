using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmP3DFloorSupportTests
{
    private static VmP3DFloorSupport.FFloor SolidFloor(double top, double bottom, int extraFlags = 0)
    {
        var f = new VmP3DFloorSupport.FFloor
        {
            Flags = VmP3DFloorSupport.FfExists | VmP3DFloorSupport.FfSolid | extraFlags,
            TopTexture = "top",
            BottomTexture = "bottom",
            Model = new VmP3DFloorSupport.SectorTerrainModel
            {
                GetTerrain = (_, _) => 7,
            },
        };
        f.Top.ZAtPoint = (_, _) => top;
        f.Bottom.ZAtPoint = (_, _) => bottom;
        return f;
    }

    [Fact]
    public void ClearDynamic3DFloorData_RemovesDynamicAndRestoresClipped()
    {
        var level = new VmP3DFloorSupport.LevelState();
        var sector = new VmP3DFloorSupport.SectorState();
        level.Sectors.Add(sector);
        var dynamic = SolidFloor(40, 30, VmP3DFloorSupport.FfDynamic);
        var clipped = SolidFloor(50, 45, VmP3DFloorSupport.FfClipped);
        sector.XFloor.FFloors.Add(dynamic);
        sector.XFloor.FFloors.Add(clipped);

        var destroyed = new List<VmP3DFloorSupport.FFloor>();
        VmP3DFloorSupport.ClearDynamic3DFloorData(level, destroyed.Add);

        Assert.Single(destroyed);
        Assert.Same(dynamic, destroyed[0]);
        Assert.Single(sector.XFloor.FFloors);
        Assert.Equal(VmP3DFloorSupport.FfExists, sector.XFloor.FFloors[0].Flags & (VmP3DFloorSupport.FfExists | VmP3DFloorSupport.FfClipped));
    }

    [Fact]
    public void RecalculateAttached3DFloors_VisitsAttachedThenSelf()
    {
        var sec = new VmP3DFloorSupport.SectorState();
        var attached = new VmP3DFloorSupport.SectorState();
        sec.XFloor.Attached.Add(attached);
        var order = new List<VmP3DFloorSupport.SectorState>();
        VmP3DFloorSupport.RecalculateAttached3DFloors(sec, order.Add);
        Assert.Equal(new[] { attached, sec }, order);
    }

    [Fact]
    public void RecalculateLights_RefreshesFromLightSource()
    {
        var sector = new VmP3DFloorSupport.SectorState { Colormap = "sector" };
        var source = new VmP3DFloorSupport.FFloor { Alpha = 90, Colormap = "src" };
        var entry = new VmP3DFloorSupport.LightListEntry { LightSource = source, ExtraColormap = "old" };
        sector.XFloor.LightList.Add(entry);

        VmP3DFloorSupport.RecalculateLights(sector);

        Assert.Equal("src", entry.ExtraColormap);
        Assert.Equal(90, entry.Blend);
    }

    [Fact]
    public void GetPlaneLight_PicksTierBelowPlaneHeight()
    {
        var sector = new VmP3DFloorSupport.SectorState();
        var low = new VmP3DFloorSupport.LightListEntry();
        low.Plane.ZAtPoint = (_, _) => 80;
        var high = new VmP3DFloorSupport.LightListEntry();
        high.Plane.ZAtPoint = (_, _) => 120;
        sector.XFloor.LightList.Add(low);
        sector.XFloor.LightList.Add(high);

        var plane = new VmP3DFloorSupport.SecPlane();
        plane.ZAtPoint = (_, _) => 130;

        var pick = VmP3DFloorSupport.GetPlaneLight(sector, plane, underside: false);
        Assert.Same(low, pick);
    }

    [Fact]
    public void LineOpeningXFloors_RaisesWalkableFloor()
    {
        var front = new VmP3DFloorSupport.SectorState();
        front.FloorPlane.ZAtPoint = (_, _) => 0;
        var back = new VmP3DFloorSupport.SectorState();
        back.FloorPlane.ZAtPoint = (_, _) => 0;
        var rover = SolidFloor(64, 48);
        front.XFloor.FFloors.Add(rover);

        var open = new VmP3DFloorSupport.LineOpeningState { Top = 200, Bottom = 0, LowFloor = 0 };
        var thing = new VmP3DFloorSupport.ThingActor { Z = 70, Height = 56 };
        var line = new VmP3DFloorSupport.LineDef { FrontSector = front, BackSector = back };

        VmP3DFloorSupport.LineOpeningXFloors(open, thing, line, 0, 0, restrict: false);

        Assert.Equal(64, open.Bottom);
        Assert.Same(rover, open.BottomFFloor);
        Assert.Equal("top", open.FloorPic);
    }

    [Fact]
    public void LineOpeningXFloors_MergesPortalLowFloor()
    {
        var front = new VmP3DFloorSupport.SectorState();
        front.FloorPlane.ZAtPoint = (_, _) => 0;
        var back = new VmP3DFloorSupport.SectorState();
        back.FloorPlane.ZAtPoint = (_, _) => 0;
        front.XFloor.FFloors.Add(SolidFloor(48, 32));
        back.XFloor.FFloors.Add(SolidFloor(40, 24));

        var open = new VmP3DFloorSupport.LineOpeningState { Top = 200, Bottom = 0, LowFloor = 0 };
        var thing = new VmP3DFloorSupport.ThingActor { Z = 50, Height = 40 };
        var line = new VmP3DFloorSupport.LineDef { FrontSector = front, BackSector = back };

        VmP3DFloorSupport.LineOpeningXFloors(open, thing, line, 0, 0, restrict: false);

        Assert.True(open.Bottom >= 32);
        Assert.NotNull(open.FloorPic);
    }

    [Fact]
    public void FindFloorPlane_UsesSolidTopWhenStandingOnIt()
    {
        var sector = new VmP3DFloorSupport.SectorState();
        sector.FloorPlane.ZAtPoint = (_, _) => 0;
        sector.FloorPlane.CoeffC = 1;
        var rover = SolidFloor(32, 16);
        rover.Top.CoeffC = -1;
        sector.XFloor.FFloors.Add(rover);

        var plane = VmP3DFloorSupport.FindFloorPlane(sector, new VmP3DFloorSupport.Vector3(0, 0, 32));

        Assert.Equal(1, plane.CoeffC);
    }

    [Fact]
    public void Find3DFloor_BelowModeReturnsIndexWhenUnderTop()
    {
        var sector = new VmP3DFloorSupport.SectorState();
        sector.CeilingPlane.ZAtPoint = (_, _) => 200;
        sector.FloorPlane.ZAtPoint = (_, _) => 0;
        sector.XFloor.FFloors.Add(SolidFloor(80, 60));

        var idx = VmP3DFloorSupport.Find3DFloor(
            sector,
            new VmP3DFloorSupport.Vector3(0, 0, 50),
            above: false,
            floor: false,
            out var cmpZ);

        Assert.Equal(0, idx);
        Assert.Equal(60, cmpZ);
    }
}
