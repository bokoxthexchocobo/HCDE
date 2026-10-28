namespace HCDE.Playsim.Tests;

public class VmP3dfloorsRecalculateTests
{
    private static VmP3dfloorsRecalculate.FFloorModel Floor(double top, double bottom, int flags = VmP3dfloorsRecalculate.FfExists | VmP3dfloorsRecalculate.FfRenderall)
    {
        var f = new VmP3dfloorsRecalculate.FFloorModel { Flags = flags, TopLightLevel = 128 };
        f.Top.ZAtPoint = (_, _) => top;
        f.Bottom.ZAtPoint = (_, _) => bottom;
        return f;
    }

    [Fact]
    public void Recalculate_BuildsCeilingLightHead()
    {
        var sector = new VmP3dfloorsRecalculate.SectorRecalcState
        {
            CenterX = 0,
            CenterY = 0,
            SectorLightLevel = 160,
        };
        sector.CeilingPlane.ZAtPoint = (_, _) => 200;
        sector.FloorPlane.ZAtPoint = (_, _) => 0;
        sector.FFloors.Add(Floor(120, 100));

        VmP3dfloorsRecalculate.Recalculate3DFloors(sector);

        Assert.Equal(2, sector.LightList.Count);
        Assert.Equal(160, sector.LightList[0].LightLevel);
        Assert.Equal(128, sector.LightList[1].LightLevel);
    }

    [Fact]
    public void Recalculate_SkipsNoshadeTier()
    {
        var sector = new VmP3dfloorsRecalculate.SectorRecalcState();
        sector.CeilingPlane.ZAtPoint = (_, _) => 200;
        sector.FloorPlane.ZAtPoint = (_, _) => 0;
        sector.FFloors.Add(Floor(120, 100, VmP3dfloorsRecalculate.FfExists | VmP3dfloorsRecalculate.FfNoshade));

        VmP3dfloorsRecalculate.Recalculate3DFloors(sector);

        Assert.Single(sector.LightList);
    }

    [Fact]
    public void Recalculate_ClipsTranslucentUnderSolid()
    {
        var sector = new VmP3dfloorsRecalculate.SectorRecalcState();
        sector.CeilingPlane.ZAtPoint = (_, _) => 200;
        sector.FloorPlane.ZAtPoint = (_, _) => 0;
        var solid = Floor(150, 130);
        var translucent = Floor(140, 110, VmP3dfloorsRecalculate.FfExists | VmP3dfloorsRecalculate.FfTranslucent);
        sector.FFloors.Add(translucent);
        sector.FFloors.Add(solid);

        VmP3dfloorsRecalculate.Recalculate3DFloors(sector);

        Assert.Contains(sector.FFloors, f => (f.Flags & VmP3dfloorsRecalculate.FfDynamic) != 0);
        Assert.Contains(sector.FFloors, f => (f.Flags & VmP3dfloorsRecalculate.FfClipped) != 0);
    }

    [Fact]
    public void Recalculate_ThisInsideBypassesClipSort()
    {
        var sector = new VmP3dfloorsRecalculate.SectorRecalcState();
        sector.CeilingPlane.ZAtPoint = (_, _) => 200;
        sector.FloorPlane.ZAtPoint = (_, _) => 0;
        var inside = Floor(50, 150, VmP3dfloorsRecalculate.FfExists | VmP3dfloorsRecalculate.FfThisinside);
        sector.FFloors.Add(inside);
        sector.FFloors.Add(Floor(120, 100));

        VmP3dfloorsRecalculate.Recalculate3DFloors(sector);

        Assert.Contains(sector.FFloors, f => (f.Flags & VmP3dfloorsRecalculate.FfThisinside) != 0);
    }
}
