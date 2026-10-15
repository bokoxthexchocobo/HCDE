using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorGodModePolicyTests
{
    [Theory]
    [InlineData(false, 10)]
    [InlineData(false, 1000)]
    [InlineData(false, 10000)]
    [InlineData(true, 10)]
    [InlineData(true, 1000)]
    [InlineData(true, 10000)]
    public void SectorDamageSkipsBothGodModesRegardlessOfAmount(bool ultimate, int amount)
    {
        var sim = Room(amount); var player = sim.Players.Single();
        player.GodMode = !ultimate; player.GodMode2 = ultimate;
        SectorDamage.Tick(sim, player);
        Assert.Equal(100, player.Health); Assert.False(player.IsDead);
        Assert.Equal(0, player.DamageCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EndGodModeClearsOnlyOrdinaryGodBeforeDamage(bool ultimate)
    {
        var sim = Room(10); var player = sim.Players.Single();
        sim.Level.Sectors[0].DamageEndsGodMode = true; player.GodMode = true; player.GodMode2 = ultimate;
        SectorDamage.Tick(sim, player);
        Assert.False(player.GodMode); Assert.Equal(ultimate, player.GodMode2);
        Assert.Equal(ultimate ? 100 : 90, player.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoclipDoesNotGrantSectorDamageImmunity(bool secondMode)
    {
        var sim = Room(10); var player = sim.Players.Single(); player.NoClip = true; player.NoClip2 = secondMode;
        SectorDamage.Tick(sim, player); Assert.Equal(90, player.Health);
    }

    [Fact]
    public void GodModeDoesNotBlockHealingOrDamageSectorExit()
    {
        var sim = Room(-10); var player = sim.Players.Single(); player.GodMode = true; player.Health = 50;
        SectorDamage.Tick(sim, player); Assert.Equal(60, player.Health);
        sim.Level.Sectors[0].DamageAmount = 1000; sim.Level.Sectors[0].DamageEndsLevel = true; player.Health = 5;
        SectorDamage.Tick(sim, player); Assert.Equal(5, player.Health); Assert.True(sim.Exited);
    }

    [Fact]
    public void SavedGodModeSectorInteractionReplaysExactly()
    {
        var sim = Room(1000); var player = sim.Players.Single(); player.GodMode = true;
        var saved = SimSavegame.Write(sim, true); SectorDamage.Tick(sim, player);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); SectorDamage.Tick(sim, player);
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(100, player.Health);
    }

    private static AuthoritySimulation Room(int amount) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, DamageAmount = amount, DamageInterval = 1 }],
        Things = [new LevelThing { Type = 1 }] });
}
