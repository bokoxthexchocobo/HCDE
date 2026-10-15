using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class DroppedRadSuitTests
{
    [Theory]
    [InlineData(PickupCatalog.RadSuit, true)]
    [InlineData(PickupCatalog.Clip, false)]
    public void DroppedPickupUsesClassSpawnBrightness(int type, bool bright)
    {
        var sim = Room(); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), type));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        Assert.Equal(bright, drop.FullBright); drop.States.Tick(drop); Assert.Equal(bright, drop.FullBright);
        Assert.Equal(type == PickupCatalog.RadSuit ? 46 : 16, drop.Height.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReconstructedSuitPreservesSpawnStateAndBrightnessOverride(bool dimmed)
    {
        var sim = Room(); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.RadSuit));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup); drop.FullBright = !dimmed;
        var saved = SimSavegame.Write(sim, true); var restored = Room(); SimSavegame.Apply(restored, saved);
        var loaded = restored.Actors.Single(a => a.SpawnedDroppedPickup);
        Assert.Equal(!dimmed, loaded.FullBright); Assert.True(loaded.States.CurrentFullBright);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
    }

    [Fact]
    public void DroppedSuitStillGrantsFullPowerWhenTouched()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(sim.SpawnDroppedPickup(player, PickupCatalog.RadSuit));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        drop.Z = player.Z; drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        sim.Tick(); Assert.True(drop.Destroyed); Assert.Equal(2100, player.PowerIronFeetTics);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
