using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PowerMaskTests
{
    [Theory]
    [InlineData("Fire", DamageFlags.None, 100)]
    [InlineData("fire", DamageFlags.None, 100)]
    [InlineData("Drowning", DamageFlags.None, 100)]
    [InlineData("Slime", DamageFlags.None, 90)]
    [InlineData("Fire", DamageFlags.Forced, 90)]
    [InlineData("Fire", DamageFlags.BypassArmor, 90)]
    public void DamageAbsorptionMatchesTypeAndFlags(string type, DamageFlags flags, int health)
    {
        var player = Room().Players.Single(); player.GivePowerMask();
        ActorDamage.Apply(player, 10, flags: flags, damageType: type); Assert.Equal(health, player.Health);
    }

    [Fact]
    public void MaskInheritsTerrainAndNormalSectorProtection()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerMask();
        TerrainDamage.Tick(sim, player); SectorDamage.Tick(sim, player);
        Assert.Equal(100, player.Health); Assert.NotNull(sim.SectorDamageRandomState);
        sim.Level.Sectors[0].Leakiness = 256; SectorDamage.Tick(sim, player);
        Assert.Equal(95, player.Health);
    }

    [Theory]
    [InlineData(IronFeetMode.Full, 100, false)]
    [InlineData(IronFeetMode.None, 95, true)]
    public void MultiplePowerTypesUseStrongestActiveSectorProtection(IronFeetMode mode, int health, bool draw)
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerMask(); player.GivePowerIronFeet();
        player.PowerIronFeetMode = mode; sim.Level.Sectors[0].Leakiness = 256;
        SectorDamage.Tick(sim, player); Assert.Equal(health, player.Health);
        Assert.Equal(draw, sim.SectorDamageRandomState.HasValue);
    }

    [Fact]
    public void InventoryUsesIndependentEightySecondTimerAndRemoval()
    {
        var player = Room().Players.Single(); AcsPlayerInventory.Give(player, "PowerMask", 1);
        Assert.Equal(2800, player.PowerMaskTics); Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerMask", false));
        Assert.Equal(0, player.PowerIronFeetTics);
        AcsPlayerInventory.Take(player, "PowerMask", 1); Assert.Equal(0, player.PowerMaskTics);
        ActorDamage.Apply(player, 10, damageType: "Fire"); Assert.Equal(90, player.Health);
    }

    [Fact]
    public void ExpirationAndDeathRemoveMask()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerMask(1);
        sim.Tick(); Assert.Equal(0, player.PowerMaskTics);
        player.GivePowerMask(); player.Health = 0; Assert.Equal(0, player.PowerMaskTics);
    }

    [Fact]
    public void SaveRestoresTimerChecksumAndOlderSaveClearsMask()
    {
        var sim = Room(); var player = sim.Players.Single(); var old = SimSavegame.Write(sim, true);
        player.GivePowerup(ManagedPowerupKind.Mask, -80); sim.RestoreState(sim.CaptureState());
        var checksum = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, old); Assert.Equal(0, player.PowerMaskTics);
        SimSavegame.Apply(sim, saved); Assert.Equal(2800, player.PowerMaskTics); Assert.Equal(checksum, sim.Checksum);
        ActorDamage.Apply(player, 10, damageType: "Fire"); Assert.Equal(100, player.Health);
    }

    [Fact]
    public void NegativeTimerRejectsBeforeMutation()
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].PowerMaskTics = -1;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new LevelTerrainDefinition("Lava", DamageAmount: 5, DamageTimeMask: 0, AllowProtection: true)],
        DefaultTerrain = "Lava", Sectors = [new LevelSector { CeilingHeight = 128, DamageAmount = 5,
            DamageInterval = 1, Leakiness = 0 }], Things = [new LevelThing { Type = 1 }]
    });
}
