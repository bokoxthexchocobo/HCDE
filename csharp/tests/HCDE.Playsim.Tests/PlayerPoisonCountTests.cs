using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerPoisonCountTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1, Coop = true }] },
        spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));

    [Theory]
    [InlineData(0, 19)]
    [InlineData(9, 19)]
    [InlineData(10.01, 20)]
    [InlineData(90, 20)]
    public void DeathFadeUsesKillerAngleBeforeTurning(double angle, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); var attacker = sim.AddBot(100, 0);
        player.PoisonCount = 20; ActorDamage.Apply(player, 200, attacker, DamageFlags.Forced);
        player.Angle = BamAngle.FromDegrees(angle); player.Tick();
        Assert.Equal(expected, player.PoisonCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrSelfAttackerFadesToZero(bool self)
    {
        var sim = Room(); var player = sim.Players.Single(); player.PoisonCount = 1;
        ActorDamage.Apply(player, 200, self ? player : null, DamageFlags.Forced);
        player.Tick(); player.Tick(); Assert.Equal(0, player.PoisonCount);
    }

    [Fact]
    public void SaveRoundTripAndLegacyClearCounterAndRejectCorruption()
    {
        var sim = Room(); var player = sim.Players.Single(); var legacy = SimSavegame.Write(sim);
        player.PoisonCount = 27; var saved = SimSavegame.Write(sim);
        Assert.Equal(160, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        player.PoisonCount = 0; SimSavegame.Apply(sim, saved); Assert.Equal(27, player.PoisonCount);
        Assert.Equal(saved, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.PoisonCount);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 101);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-player-poison-count-invalid", error);
    }

    [Fact]
    public void InvalidCounterIsRejectedWithoutChangingIt()
    {
        var player = new PlayerPawn { PoisonCount = 10 };
        Assert.Throws<ArgumentOutOfRangeException>(() => player.PoisonCount = -1);
        Assert.Throws<ArgumentOutOfRangeException>(() => player.PoisonCount = 101);
        Assert.Equal(10, player.PoisonCount);
    }

    [Fact]
    public void RebirthClearsRemainingPoison()
    {
        var sim = Room(); var player = sim.Players.Single(); player.PoisonCount = 100;
        ActorDamage.Apply(player, 200, flags: DamageFlags.Forced);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        Assert.True(player.PoisonCount > 0);
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.False(player.IsDead); Assert.Equal(0, player.PoisonCount);
    }
}
