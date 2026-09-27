using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamageExitTests
{
    [Theory]
    [InlineData(SpawnGameMode.Single, false, true)]
    [InlineData(SpawnGameMode.Single, true, true)]
    [InlineData(SpawnGameMode.Cooperative, false, true)]
    [InlineData(SpawnGameMode.Cooperative, true, true)]
    [InlineData(SpawnGameMode.Deathmatch, false, true)]
    [InlineData(SpawnGameMode.Deathmatch, true, false)]
    public void NoExitOnlySuppressesDeathmatchExitAndKeepsDamage(SpawnGameMode mode, bool noExit, bool exits)
    {
        var sim = AuthoritySimulation.Start(Level(MapDataFormat.HexenBinary, 75),
            spawnOptions: new SpawnOptions(Mode: mode), noExit: noExit);
        var player = sim.Players.Single(); player.Health = 30; player.GodMode = true;
        sim.Tick();
        Assert.Equal(10, player.Health); Assert.False(player.GodMode); Assert.Equal(exits, sim.Exited);
        if (!exits)
        {
            for (var i = 0; i < 32; i++) sim.Tick();
            Assert.True(player.IsDead); Assert.False(sim.Exited);
        }
    }

    [Fact]
    public void ExitRuleParticipatesInChecksumBeforeGameplay()
    {
        var level = Level(MapDataFormat.HexenBinary, 75);
        var options = new SpawnOptions(Mode: SpawnGameMode.Deathmatch);
        Assert.NotEqual(AuthoritySimulation.Start(level, spawnOptions: options).Checksum,
            AuthoritySimulation.Start(level, spawnOptions: options, noExit: true).Checksum);
    }

    [Theory]
    [InlineData(MapDataFormat.DoomBinary, 11)]
    [InlineData(MapDataFormat.HexenBinary, 75)]
    public void ExitFloorRemovesGodModeAndExitsAtThreshold(MapDataFormat format, short special)
    {
        var sim = Room(format, special); var player = sim.Players.Single();
        player.Health = 50; player.GodMode = true;
        sim.Tick(); Assert.False(player.GodMode); Assert.Equal(30, player.Health); Assert.False(sim.Exited);
        for (var i = 0; i < 31; i++) sim.Tick();
        Assert.False(sim.Exited);
        sim.Tick(); Assert.Equal(10, player.Health); Assert.True(sim.Exited); Assert.False(sim.SecretExit);
    }

    [Fact]
    public void ContactRemovesGodModeBetweenDamageTicksButKeepsInvulnerability()
    {
        var sim = Room(MapDataFormat.HexenBinary, 75); var player = sim.Players.Single();
        player.Invulnerable = true; sim.Tick();
        player.GodMode = true; sim.Tick();
        Assert.False(player.GodMode); Assert.True(player.Invulnerable);
        Assert.Equal(100, player.Health); Assert.False(sim.Exited);
    }

    [Fact]
    public void ExplicitDamageDoesNotAcquireExitFlags()
    {
        var level = Level(MapDataFormat.HexenBinary, 75); level.Sectors[0].DamageAmount = 5;
        var sim = AuthoritySimulation.Start(level); var player = sim.Players.Single();
        player.Health = 10; player.GodMode = true;
        sim.Tick(); Assert.True(player.GodMode); Assert.Equal(10, player.Health); Assert.False(sim.Exited);
        Assert.False(sim.Level.Sectors[0].DamageEndsLevel);
    }

    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 90)]
    public void OrdinaryDamageRespectsGodModeUnlessExplicitlyBypassed(bool bypass, int expected)
    {
        var player = new PlayerPawn { GodMode = true };
        ActorDamage.Apply(player, 10, flags: bypass ? DamageFlags.BypassInvulnerability : DamageFlags.None);
        Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void AirbornePlayerKeepsGodMode()
    {
        var sim = Room(MapDataFormat.HexenBinary, 75); var player = sim.Players.Single();
        player.GodMode = true; player.NoGravity = true; player.Z = Fixed.FromInt(16);
        sim.Tick(); Assert.True(player.GodMode); Assert.Equal(100, player.Health);
    }

    private static AuthoritySimulation Room(MapDataFormat format, short special) => AuthoritySimulation.Start(Level(format, special));
    private static PlayLevel Level(MapDataFormat format, short special) => new()
    {
        Format = format, Sectors = [new LevelSector { CeilingHeight = 128, Special = special }],
        Things = [new LevelThing { Type = 1 }],
    };
}
