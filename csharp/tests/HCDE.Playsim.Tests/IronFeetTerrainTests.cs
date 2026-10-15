using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IronFeetTerrainTests
{
    [Theory]
    [InlineData(false, false, 95)]
    [InlineData(false, true, 95)]
    [InlineData(true, false, 95)]
    [InlineData(true, true, 100)]
    public void PeriodicDamageRequiresBothTerrainOptInAndActivePower(bool protection, bool power, int health)
    {
        var sim = Room(protection); var player = sim.Players.Single();
        if (power) player.GivePowerIronFeet();
        TerrainDamage.Tick(sim, player);
        Assert.Equal(health, player.Health);
    }

    [Fact]
    public void LandingDamageDoesNotHonorPeriodicProtection()
    {
        var sim = Room(true); var player = sim.Players.Single(); player.GivePowerIronFeet();
        sim.Level.TerrainDefinitions = [sim.Level.TerrainDefinitions[0] with { DamageTimeMask = 1 }];
        sim.Tick(); TerrainDamage.Land(sim, player);
        Assert.Equal(95, player.Health);
    }

    [Fact]
    public void InventoryGrantCountRemovalAndDeathUseTimer()
    {
        var sim = Room(true); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "PowerIronFeet", 1);
        Assert.Equal(2100, player.PowerIronFeetTics);
        Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerIronFeet", false));
        AcsPlayerInventory.Take(player, "PowerIronFeet", 1);
        Assert.Equal(0, player.PowerIronFeetTics);
        player.GivePowerIronFeet(); player.Health = 0;
        Assert.Equal(0, player.PowerIronFeetTics);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    public void PlayerPropertyGrantsOrRemovesSuit(int value)
    {
        var sim = Room(true); var player = sim.Players.Single(); player.GivePowerIronFeet();
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, player, 0, value, 8));
        Assert.Equal(value != 0, player.PowerIronFeetTics > 0);
    }

    [Fact]
    public void ExpirationRemovesTerrainProtection()
    {
        var sim = Room(true); var player = sim.Players.Single(); player.GivePowerIronFeet(1);
        sim.Tick(); Assert.Equal(0, player.PowerIronFeetTics);
        Assert.Equal(95, player.Health);
    }

    [Fact]
    public void SaveRestoresTimerChecksumAndLegacyClearsPower()
    {
        var sim = Room(true); var player = sim.Players.Single(); var legacy = SimSavegame.Write(sim, true);
        player.GivePowerup(ManagedPowerupKind.IronFeet, -60);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.PowerIronFeetTics);
        SimSavegame.Apply(sim, saved); Assert.Equal(2100, player.PowerIronFeetTics); Assert.Equal(checksum, sim.Checksum);
        TerrainDamage.Tick(sim, player); Assert.Equal(100, player.Health);
    }

    [Fact]
    public void InvalidTimerRejectsBeforeRestore()
    {
        var sim = Room(true); var state = sim.CaptureState(); state.Actors[0].PowerIronFeetTics = -1;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    private static AuthoritySimulation Room(bool protection) => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new LevelTerrainDefinition("Lava", DamageAmount: 5, DamageTimeMask: 0,
            DamageOnLand: true, Splash: "LavaSplash", AllowProtection: protection)],
        TerrainSplashes = ["LavaSplash"], DefaultTerrain = "Lava",
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
