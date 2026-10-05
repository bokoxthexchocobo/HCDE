using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerHealthUpgradeTests
{
    [Theory]
    [InlineData(false, 150)]
    [InlineData(true, 180)]
    public void MaximumIncludesUpgradesOnlyWhenRequested(bool upgrades, int expected)
    {
        var player = new PlayerPawn { MaxHealth = 150, Stamina = 10, BonusHealth = 20, Health = 80 };
        Assert.Equal(expected, player.GetMaxHealth(upgrades));
        Assert.True(player.GiveBody(200)); Assert.Equal(180, player.Health);
    }

    [Fact]
    public void UpgradeArchiveRestoresFieldsAndHealingContinuation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Stamina = 10; player.BonusHealth = 20;
        var bytes = SimSavegame.Write(sim.CaptureState());
        player.Stamina = 0; player.BonusHealth = 0;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(10, player.Stamina); Assert.Equal(20, player.BonusHealth);
        Assert.True(player.GiveBody(100)); Assert.Equal(130, player.Health);
        bytes[^4] = 13;
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Fact]
    public void LegacyArchiveClearsUpgradeMutations()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim.CaptureState()); var player = sim.Players.Single();
        player.Stamina = 10; player.BonusHealth = 20;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(0, player.Stamina); Assert.Equal(0, player.BonusHealth);
    }

    [Fact]
    public void UpgradeFieldsAffectChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().BonusHealth = 20;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
