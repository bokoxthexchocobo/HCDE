using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MaxPickupHealthTests
{
    [Theory]
    [InlineData(0, 150)]
    [InlineData(100, 150)]
    [InlineData(200, 200)]
    public void OverrideReplacesUpgradesButPreservesLargerItemCap(int itemCap, int expected)
    {
        var player = new PlayerPawn { Health = 80, MaxHealth = 300, Stamina = 10, BonusHealth = 20, MaxPickupHealth = 150 };
        Assert.True(player.GiveBody(500, itemCap)); Assert.Equal(expected, player.Health);
        Assert.Equal(330, player.GetMaxHealth(true));
    }

    [Fact]
    public void ArchiveRestoresOverrideAndHealingContinuation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.MaxPickupHealth = 150;
        var bytes = SimSavegame.Write(sim.CaptureState());
        Assert.Equal(45, BitConverter.ToUInt16(bytes, 4));
        player.MaxPickupHealth = 0; SimSavegame.Apply(sim, bytes);
        Assert.Equal(150, player.MaxPickupHealth);
        Assert.True(player.GiveBody(100)); Assert.Equal(150, player.Health);
        bytes[^4] = 13; Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Fact]
    public void LegacyRestoreClearsOverride()
    {
        var sim = Room(); var player = sim.Players.Single(); player.BonusHealth = 20;
        var bytes = SimSavegame.Write(sim.CaptureState()); player.MaxPickupHealth = 150;
        SimSavegame.Apply(sim, bytes); Assert.Equal(0, player.MaxPickupHealth);
        Assert.Equal(20, player.BonusHealth);
    }

    [Fact]
    public void OverrideChangesChecksum()
    {
        var baseline = Room(); var modified = Room(); modified.Players.Single().MaxPickupHealth = 150;
        baseline.Tick(); modified.Tick(); Assert.NotEqual(baseline.Checksum, modified.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
