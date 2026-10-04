using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthSkillFactorTests
{
    [Theory]
    [InlineData(2, 20)]
    [InlineData(0.55, 5)]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    public void HealthPathsApplySkillFactorAndMinimum(double factor, int expected)
    {
        var sim = Room(factor); var player = sim.Players.Single();
        player.Health = 10;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Stimpack));
        Assert.Equal(10 + expected, player.Health);
        player.Health = 10;
        AcsPlayerInventory.Give(player, "Health", 10);
        Assert.Equal(10 + expected, player.Health);
        player.Health = 10; player.DrainStrength = 0.5;
        ActorDamage.Apply(new Actor { Health = 100 }, 20, player);
        Assert.Equal(10 + expected, player.Health);
    }

    [Fact]
    public void AmountClampPrecedesSkillScaling()
    {
        var sim = Room(2); var player = sim.Players.Single();
        player.Health = 1; player.MaxHealth = 200000;
        AcsPlayerInventory.Give(player, "Health", int.MaxValue);
        Assert.Equal(131073, player.Health);
    }

    [Fact]
    public void BonusLimitStillCapsScaledGrant()
    {
        var sim = Room(2); var player = sim.Players.Single();
        player.Health = 199;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.HealthBonus));
        Assert.Equal(200, player.Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(150)]
    public void ScalingDoesNotReviveOrReduceOverCapHealth(int health)
    {
        var sim = Room(2); var player = sim.Players.Single(); player.Health = health;
        AcsPlayerInventory.Give(player, "Health", 10);
        Assert.Equal(health, player.Health);
    }

    [Fact]
    public void SkillFactorChangesChecksumAndSurvivesRestore()
    {
        var normal = Room(1); var scaled = Room(2);
        Assert.NotEqual(normal.Checksum, scaled.Checksum);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(scaled), out var state, out var error), error);
        scaled.RestoreState(state);
        Assert.Equal(2, scaled.HealthFactor);
        scaled.Players.Single().Health = 10;
        AcsPlayerInventory.Give(scaled.Players.Single(), "Health", 10);
        Assert.Equal(30, scaled.Players.Single().Health);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonfiniteFactorsAreRejected(double factor) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Room(factor));

    private static AuthoritySimulation Room(double factor) => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(HealthFactor: factor));
}
