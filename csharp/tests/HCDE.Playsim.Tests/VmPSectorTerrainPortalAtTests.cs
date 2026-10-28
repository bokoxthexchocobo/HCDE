namespace HCDE.Playsim.Tests;

public class VmPSectorTerrainPortalAtTests
{
    [Fact]
    public void GetTerrain_UsesExplicitTerrainNum()
    {
        var sec = new VmPSectorTerrainPortalAt.MapSector();
        sec.TerrainNum[0] = 2;
        VmPSectorTerrainPortalAt.TerrainTypes = new[] { 0, 1, 2 };
        Assert.Equal(2, VmPSectorTerrainPortalAt.GetTerrain(sec, 0));
    }

    [Fact]
    public void CheckPortalPlane_SetsObstructedWhenFloorAbovePortal()
    {
        var sec = new VmPSectorTerrainPortalAt.MapSector();
        sec.Planes[0].PortalType = VmPSectorTerrainPortalAt.PortsLinkedPortal;
        sec.Planes[0].PortalPlaneZ = 64;
        sec.Planes[0].TexZ = 128;
        VmPSectorTerrainPortalAt.CheckPortalPlane(sec, 0);
        Assert.Equal(VmPSectorTerrainPortalAt.PlanefObstructed, sec.Planes[0].Flags);
    }

    [Fact]
    public void NextLowestFloorAt_PicksSolid3dFloor()
    {
        var sec = new VmPSectorTerrainPortalAt.MapSector();
        sec.FloorPlane.ZAtPoint = (_, _) => 0;
        var ff = new VmPSectorTerrainPortalAt.FFloor
        {
            Flags = VmPSectorTerrainPortalAt.FfExists | VmPSectorTerrainPortalAt.FfSolid,
        };
        ff.Top.ZAtPoint = (_, _) => 32;
        ff.Bottom.ZAtPoint = (_, _) => 16;
        sec.XFloor.FFloors.Add(ff);
        var z = VmPSectorTerrainPortalAt.NextLowestFloorAt(sec, 0, 0, 40, 0, 24, out _, out var hit);
        Assert.Equal(32, z);
        Assert.Same(ff, hit);
    }

    [Fact]
    public void GetFriction_UsesSectorFrictionWhenFlagged()
    {
        var sec = new VmPSectorTerrainPortalAt.MapSector { Flags = VmPSectorTerrainPortalAt.SecfFriction, Friction = 0.5, MoveFactor = 0.1 };
        var f = VmPSectorTerrainPortalAt.GetFriction(sec, 0, out var mf);
        Assert.Equal(0.5, f);
        Assert.Equal(0.1, mf);
    }

    [Fact]
    public void HighestCeilingAt_StopsAtBlockingPortal()
    {
        var inner = new VmPSectorTerrainPortalAt.MapSector();
        inner.CeilingPlane.ZAtPoint = (_, _) => 200;
        inner.Planes[1].PortalBlocksMovement = true;
        var outer = new VmPSectorTerrainPortalAt.MapSector { Level = new VmPSectorTerrainPortalAt.MapLevel() };
        outer.Level.PointInSector = (_, _) => inner;
        outer.Planes[1].PortalBlocksMovement = false;
        outer.Planes[1].PortalPlaneZ = 100;
        outer.Planes[1].PortalDisplacement = (0, 0);
        var h = VmPSectorTerrainPortalAt.HighestCeilingAt(outer, 0, 0, out var res);
        Assert.Same(inner, res);
        Assert.Equal(200, h);
    }
}
