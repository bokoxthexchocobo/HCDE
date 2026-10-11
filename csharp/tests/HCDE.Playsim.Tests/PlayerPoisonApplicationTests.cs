using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerPoisonApplicationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ProtectionRejectsPoisonWithoutChangingExistingState(int protection)
    {
        var player = new PlayerPawn(); Assert.True(player.ApplyPoison(7, new Actor { DamageType = "Fire" }));
        player.GodMode = protection == 0; player.GodMode2 = protection == 1;
        player.Invulnerable = protection == 2; player.NoDamage = protection == 3;
        Assert.False(player.ApplyPoison(20, new Actor { DamageType = "Ice" }));
        Assert.Equal(7, player.PoisonCount); Assert.Equal("Fire", player.PoisonDamageType);
    }

    [Fact]
    public void AccumulationCapsAndSnapshotsPoisonerTypesWithoutDealingImmediateDamage()
    {
        var player = new PlayerPawn { Health = 100 }; var poisoner = new Actor { DamageType = "Fire", PainType = "Ice" };
        Assert.True(player.ApplyPoison(60, poisoner)); Assert.True(player.ApplyPoison(int.MaxValue, poisoner));
        poisoner.DamageType = "Poison"; poisoner.PainType = "None";
        Assert.Equal(100, player.PoisonCount); Assert.Equal(100, player.Health);
        Assert.Equal("Fire", player.PoisonDamageType); Assert.Equal("Ice", player.PoisonPainType);
        Assert.True(player.ApplyPoison(1, poisoner)); Assert.Equal("Poison", player.PoisonPainType);
        Assert.True(player.ApplyPoison(1)); Assert.Null(player.PoisonDamageType); Assert.Null(player.PoisonPainType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void NonpositivePoisonSucceedsWithoutReplacingExistingTypes(int amount)
    {
        var player = new PlayerPawn(); player.ApplyPoison(10, new Actor { DamageType = "Fire" });
        Assert.True(player.ApplyPoison(amount)); Assert.Equal(10, player.PoisonCount);
        Assert.Equal("Fire", player.PoisonDamageType);
    }

    [Fact]
    public void SaveRestoresSourceAndTypesWhileLegacyClearsThem()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); var source = sim.AddBot(100, 0); var legacy = SimSavegame.Write(sim);
        player.ApplyPoison(20, new Actor { DamageType = "Fire", PainType = "Ice" }, source);
        var saved = SimSavegame.Write(sim); player.ApplyPoison(1);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(source.Id, player.PoisonSourceId); Assert.Equal("Fire", player.PoisonDamageType);
        Assert.Equal("Ice", player.PoisonPainType); Assert.Equal(20, player.PoisonCount);
        Assert.Equal(saved, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, legacy); Assert.Null(player.PoisonSourceId);
        Assert.Null(player.PoisonDamageType); Assert.Null(player.PoisonPainType);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        saved[saved.Length - size + 17] = 0xff;
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-player-poison-invalid", error);
    }
}
