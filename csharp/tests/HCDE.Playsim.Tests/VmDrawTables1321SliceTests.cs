namespace HCDE.Playsim.Tests;

public class VmDrawTables1321SliceTests
{
    [Fact]
    public void Init_BuildsShadeFuzzAndParticleTables()
    {
        var host = new VmDrawTables1321();
        host.InitShadeMaps();
        Assert.Equal(0, host.ShadeTables[0]);
        Assert.Equal(64, host.ShadeTables[255]);
        Assert.Equal(~0u, host.ShadeColor[15]);
        Assert.Equal(255, host.Identity[255]);

        host.InitFuzzTable();
        Assert.Equal(6, host.FuzzOffset[0]);
        Assert.InRange(host.FuzzOffset[49], 0, 31);
        Assert.Equal(0, host.FuzzOffset[50]);

        host.InitParticles();
        Assert.Equal(128u, host.Particles[0]);
        int mid = 32 + 32 * VmDrawTables1321.ParticleSize;
        Assert.True(host.Particles[VmDrawTables1321.ParticleSize * VmDrawTables1321.ParticleSize + mid] > 100);
        int corner = VmDrawTables1321.ParticleSize * VmDrawTables1321.ParticleSize;
        Assert.Equal(0u, host.Particles[corner]);

        host.SetDynLights(false);
        Assert.False(host.DynLights);
        Assert.Equal(1, host.SyncCount);
    }

    [Fact]
    public void Fuzz_AdvancesAndSetLightsKeepsTheNearColumn()
    {
        var host = new VmDrawTables1321();
        host.UpdateFuzzFrame();
        Assert.Equal(37, host.FuzzPos);
        host.UpdateFuzzFrame();
        Assert.Equal(25, host.FuzzPos);
        host.FuzzScale = false;
        int held = host.FuzzPos;
        host.UpdateFuzzFrame();
        Assert.Equal(held, host.FuzzPos);
        host.UpdateFuzzColumn(0, 10);
        Assert.Equal((held + 10) % VmDrawTables1321.FuzzTable, host.FuzzPos);
        host.UpdateFuzzColumn(8, 2);
        Assert.Equal((held + 10) % VmDrawTables1321.FuzzTable, host.FuzzPos);

        var wall = new VmDrawTables1321.Wall
        {
            Sx1 = 0,
            Sx2 = 320,
            Sz1 = 2,
            Sz2 = 2,
            Lights = new List<VmDrawTables1321.DynLight?>
            {
                new() { Radius = 100, Red = 255, Attenuated = false },
                new() { Active = false, Radius = 100, Blue = 9 },
                null
            }
        };
        host.SetLights(wall, 160, 100);
        VmDrawTables1321.DrawerLight light = Assert.Single(host.Lights);
        Assert.Equal(255 << 16, light.Color);
        Assert.Equal(2.56f, light.Radius);
        Assert.True(wall.ViewY > 0);

        wall.Lights = new List<VmDrawTables1321.DynLight?>();
        for (int i = 0; i < 20; i++)
            wall.Lights.Add(new VmDrawTables1321.DynLight { Radius = 1_000_000, Green = i });
        host.SetLights(wall, 160, 100);
        Assert.Equal(VmDrawTables1321.MaxLights, host.Lights.Count);

        wall.Lights = null;
        host.SetLights(wall, 160, 100);
        Assert.Empty(host.Lights);
    }
}
