using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerMaximumHealthTests
{
    [Theory]
    [InlineData(250, 250)]
    [InlineData(50, 50)]
    [InlineData(0, 100)]
    [InlineData(-7, 100)]
    public void SpawnHealthPropertyUsesIndependentPlayerMaximum(int maximum, int effective)
    {
        var sim = Room(); var player = sim.Players.Single();
        var spawn = player.ResurrectionHealth;
        AcsActorProperties.Set(sim, player, 0, AcsActorProperties.SpawnHealth, maximum);
        Assert.Equal(maximum, player.MaxHealth);
        Assert.Equal(spawn, player.ResurrectionHealth);
        Assert.Equal(effective, AcsActorProperties.Get(sim, player, 0, AcsActorProperties.SpawnHealth));
        Assert.True(AcsActorProperties.Check(sim, player, 0, AcsActorProperties.SpawnHealth, effective));
        Assert.Equal(effective, AcsPlayerInventory.Count(player, "Health", true));
    }

    [Theory]
    [InlineData("Health")]
    [InlineData("Stimpack")]
    [InlineData("Medikit")]
    public void OrdinaryHealthGrantUsesCustomMaximum(string type)
    {
        var player = new PlayerPawn { Health = 145, MaxHealth = 150 };
        AcsPlayerInventory.Give(player, type, 25);
        Assert.Equal(150, player.Health);
        AcsPlayerInventory.Give(player, type, 25);
        Assert.Equal(150, player.Health);
    }

    [Fact]
    public void BonusPickupRetainsExplicitLimit()
    {
        var player = new PlayerPawn { Health = 199, MaxHealth = 150 };
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.HealthBonus));
        Assert.Equal(200, player.Health);
    }

    [Theory]
    [InlineData(PickupCatalog.Stimpack)]
    [InlineData(PickupCatalog.Medikit)]
    public void WorldPickupUsesCustomLimitWithoutReducingOverCapHealth(int type)
    {
        var player = new PlayerPawn { Health = 145, MaxHealth = 150 };
        Assert.True(PickupCatalog.TryGive(player, type));
        Assert.Equal(150, player.Health);
        player.MaxHealth = 50;
        Assert.False(PickupCatalog.TryGive(player, type));
        Assert.Equal(150, player.Health);
    }

    [Fact]
    public void MaximumGrantCannotOverflowHealth()
    {
        var player = new PlayerPawn { Health = int.MaxValue - 5, MaxHealth = int.MaxValue };
        AcsPlayerInventory.Give(player, "Health", int.MaxValue);
        Assert.Equal(int.MaxValue, player.Health);
    }

    [Theory]
    [InlineData(250, false)]
    [InlineData(250, true)]
    [InlineData(-7, true)]
    public void MaximumRoundTripsAndChangesChecksum(int maximum, bool serialized)
    {
        var sim = Room(); var player = sim.Players.Single();
        var baseline = sim.Checksum;
        player.MaxHealth = maximum;
        sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(baseline, sim.Checksum);
        var expected = sim.Checksum;
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = SimSavegame.Write(state);
            Assert.Equal(37, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        player.MaxHealth = 0;
        sim.RestoreState(state);
        Assert.Equal(maximum, player.MaxHealth);
        Assert.Equal(expected, sim.Checksum);
    }

    [Fact]
    public void LegacyArchiveRestoresDefaultMaximum()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        Assert.Equal(36, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.Players.Single().MaxHealth = 250;
        sim.RestoreState(state);
        Assert.Equal(100, sim.Players.Single().EffectiveMaxHealth);
    }

    [Fact]
    public void InvalidExtensionSizeIsRejected()
    {
        var sim = Room(); sim.Players.Single().MaxHealth = 250;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-player-health-size", error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(37)]
    public void InvalidPriorVersionIsRejected(int prior)
    {
        var sim = Room(); sim.Players.Single().MaxHealth = 250;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), prior);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-player-health-header", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
