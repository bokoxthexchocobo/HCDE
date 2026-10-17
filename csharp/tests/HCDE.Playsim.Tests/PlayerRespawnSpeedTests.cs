using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerRespawnSpeedTests
{
    [Theory]
    [InlineData("", false, 1)]
    [InlineData("Speed = 2", false, 2)]
    [InlineData("Speed = 98304", false, 1.5)]
    [InlineData("Speed = 0", false, 0)]
    [InlineData("Speed = -1", false, -1)]
    [InlineData("Speed = 2\nFast speed = 3", true, 3)]
    [InlineData("Speed = 2\nFast speed = -1", true, 2)]
    public void RespawnRestoresClassSpeedAndReplaysFromDeadSave(string fields, bool fast, double expected)
    {
        var sim = Room(fields, fast); var player = sim.Players.Single();
        player.MovementSpeed = Fixed.FromInt(7); player.FloatSpeed = 9; player.PainThreshold = 17;
        player.HasTuningOverride = true; KillAndArm(sim, player);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(fields, fast); SimSavegame.Apply(fresh, saved);
        var loaded = fresh.Players.Single(); Assert.Equal(7, loaded.MovementSpeed.ToDouble());
        sim.TryPlayerRespawn(player); fresh.TryPlayerRespawn(loaded);
        Assert.False(player.IsDead); Assert.Equal(expected, player.MovementSpeed.ToDouble());
        Assert.Equal(expected, loaded.MovementSpeed.ToDouble());
        Assert.Equal(4, player.FloatSpeed); Assert.Equal(0, player.PainThreshold); Assert.False(player.HasTuningOverride);
        Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        fresh.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); fresh.Tick();
        Assert.Equal(expected, player.X.ToDouble()); Assert.Equal(player.X, loaded.X); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Theory]
    [InlineData("blocked")]
    [InlineData("early")]
    [InlineData("unarmed")]
    public void UnsuccessfulRespawnPreservesSavedRuntimeSpeed(string reason)
    {
        var sim = Room("Speed = 2", false); var player = sim.Players.Single();
        player.MovementSpeed = Fixed.FromInt(7); KillAndArm(sim, player);
        if (reason == "blocked") sim.NoRespawn = true;
        if (reason == "early") sim.Thinkers.Clock.Restore(player.RespawnEarliestTic - 1);
        if (reason == "unarmed") player.RespawnArmed = false;
        var saved = SimSavegame.Write(sim, true); sim.TryPlayerRespawn(player);
        Assert.True(player.IsDead); Assert.Equal(7, player.MovementSpeed.ToDouble());
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void DeathmatchRespawnRestoresFastClassSpeed()
    {
        var sim = Room("Speed = 2\nFast speed = 3", true, SpawnGameMode.Deathmatch);
        var player = sim.Players.Single(); player.MovementSpeed = Fixed.FromInt(7); KillAndArm(sim, player);
        sim.TryPlayerRespawn(player); Assert.False(player.IsDead); Assert.Equal(3, player.MovementSpeed.ToDouble());
    }

    private static void KillAndArm(AuthoritySimulation sim, PlayerPawn player)
    {
        ActorDamage.Apply(player, 10000); Assert.True(player.IsDead);
        player.RespawnArmed = true; sim.Thinkers.Clock.Restore(player.RespawnEarliestTic);
    }

    private static AuthoritySimulation Room(string fields, bool fast, SpawnGameMode mode = SpawnGameMode.Cooperative)
    {
        var patch = DehackedPatch.Apply($"Thing 1\n{fields}\n"); Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch,
            spawnOptions: new SpawnOptions(Mode: mode, FastMonsters: fast));
    }
}
