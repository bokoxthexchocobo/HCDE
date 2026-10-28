namespace HCDE.Playsim.Tests;

public class VmPSectorGlowActionsTests
{
    [Fact]
    public void RemoveForceField_ClearsBlockingForceFieldLines()
    {
        var sector = new VmPSectorGlowActions.MapSector();
        var line = new VmPSectorGlowActions.MapLine
        {
            BackSector = new VmPSectorGlowActions.MapSector(),
            Special = VmPSectorGlowActions.ForceFieldSpecial,
            Flags = VmPSectorGlowActions.MlBlocking,
        };
        sector.Lines.Add(line);
        VmPSectorGlowActions.RemoveForceField(sector);
        Assert.Equal(0u, line.Flags);
        Assert.Equal(0, line.Special);
    }

    [Fact]
    public void CheckSpriteGlow_BrightensNearFloorGlow()
    {
        var sector = new VmPSectorGlowActions.MapSector();
        sector.Planes[0].GlowColor = 0x00ff0000;
        sector.Planes[0].GlowHeight = 32;
        sector.FloorPlane.ZAtPoint = (_, _) => 0;
        var lit = VmPSectorGlowActions.CheckSpriteGlow(sector, 100, 0, 0, 8);
        Assert.True(lit > 100);
    }

    [Fact]
    public void CheckOverlap_SetsFlagWhenFloorAboveCeiling()
    {
        var sector = new VmPSectorGlowActions.MapSector();
        sector.Planes[0].TexZ = 128;
        sector.Planes[1].TexZ = 64;
        VmPSectorGlowActions.CheckOverlap(sector);
        Assert.Equal(VmPSectorGlowActions.SecmfOverlapping, sector.MoreFlags);
    }

    [Fact]
    public void TriggerSectorActions_OrCombinesResults()
    {
        var sector = new VmPSectorGlowActions.MapSector
        {
            SecActTarget = new VmPSectorGlowActions.SectorActionActor
            {
                Tracer = new VmPSectorGlowActions.SectorActionActor(),
            },
        };
        var hits = 0;
        var ok = VmPSectorGlowActions.TriggerSectorActions(sector, null, 1, (_, _, _) =>
        {
            hits++;
            return hits == 1;
        });
        Assert.True(ok);
        Assert.Equal(2, hits);
    }
}
