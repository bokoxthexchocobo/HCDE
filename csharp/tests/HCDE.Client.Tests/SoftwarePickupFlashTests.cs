using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class SoftwarePickupFlashTests
{
    [Theory]
    [InlineData(false, 32, 80, 64)]
    [InlineData(true, 0, 48, 0)]
    public void ConfiguredPickupColorReachesBothViews(bool hardware, byte red, byte green, byte blue)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            PickupColor = 0x00ff00,
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.HealthBonus }]
        });
        using var host = new ClientHost(sim); host.UseHardware = hardware; host.Tick(default);
        host.Frame.Get(0, 0, out var r, out var g, out var b); Assert.Equal(new[] { red, green, blue }, new[] { r, g, b });
    }

    [Theory]
    [InlineData(false, 72, 67, 77)]
    [InlineData(true, 40, 35, 12)]
    public void ContactRendersDefaultDoomPickupTintInBothViews(bool hardware, byte red, byte green, byte blue)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.HealthBonus }]
        });
        using var host = new ClientHost(sim); host.UseHardware = hardware; host.Tick(default);
        Assert.Equal(6, sim.Players.Single().BonusCount);
        host.Frame.Get(0, 0, out var r, out var g, out var b);
        Assert.Equal(new[] { red, green, blue }, new[] { r, g, b });
        host.GamePaused = true; var saved = SimSavegame.Write(sim, true); host.Tick(default);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }
}
