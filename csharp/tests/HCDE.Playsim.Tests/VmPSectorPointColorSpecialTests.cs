namespace HCDE.Playsim.Tests;

public class VmPSectorPointColorSpecialTests
{
    [Fact]
    public void FindLowestCeilingPoint_FlatCeilingUsesFd()
    {
        var sector = new VmPSectorPointColorSpecial.MapSector();
        sector.CeilingPlane.D = 128;
        Assert.Equal(128, VmPSectorPointColorSpecial.FindLowestCeilingPoint(sector));
    }

    [Fact]
    public void ClosestPoint_PicksNearestOnSegment()
    {
        var sector = new VmPSectorPointColorSpecial.MapSector();
        var line = new VmPSectorPointColorSpecial.MapLine();
        line.V1.X = 0;
        line.V1.Y = 0;
        line.V2.X = 10;
        line.V2.Y = 0;
        sector.Lines.Add(line);
        VmPSectorPointColorSpecial.ClosestPoint(sector, 5, 4, out var ox, out var oy);
        Assert.Equal(5, ox);
        Assert.Equal(0, oy);
    }

    [Fact]
    public void SetColor_WritesColormapAndInvokesRecalc()
    {
        var sector = new VmPSectorPointColorSpecial.MapSector();
        var hit = false;
        VmPSectorPointColorSpecial.SetColor(sector, 0xff, 2, _ => hit = true);
        Assert.Equal(0xff, sector.Colormap.LightColor);
        Assert.True(hit);
    }

    [Fact]
    public void PlaneMoving_DetectsFloorData()
    {
        var sector = new VmPSectorPointColorSpecial.MapSector { FloorData = new object() };
        Assert.True(VmPSectorPointColorSpecial.PlaneMoving(sector, VmPSectorPointColorSpecial.Floor));
    }

    [Fact]
    public void GetFloorLight_UsesAbsoluteWhenFlagged()
    {
        var sector = new VmPSectorPointColorSpecial.MapSector();
        sector.Planes[0].Flags = VmPSectorPointColorSpecial.PlanefAbslighting;
        sector.Planes[0].Light = 200;
        Assert.Equal(200, VmPSectorPointColorSpecial.GetFloorLight(sector));
    }

    [Fact]
    public void TransferSpecial_CopiesDamageFields()
    {
        var dst = new VmPSectorPointColorSpecial.MapSector();
        var src = new VmPSectorPointColorSpecial.MapSector
        {
            Special = 7,
            DamageAmount = 5,
            DamageType = 2,
            DamageInterval = 32,
            LeakyDamage = 1,
            Flags = 0x10000
        };
        VmPSectorPointColorSpecial.TransferSpecial(dst, src);
        Assert.Equal(7, dst.Special);
        Assert.Equal(5, dst.DamageAmount);
    }

    [Fact]
    public void ValidatePortal_RejectsMissingSkybox()
    {
        var sector = new VmPSectorPointColorSpecial.MapSector();
        var port = new VmPSectorPointColorSpecial.SectorPortal { Type = 1, Skybox = null };
        Assert.Null(VmPSectorPointColorSpecial.ValidatePortal(port, 0, 0));
    }
}
