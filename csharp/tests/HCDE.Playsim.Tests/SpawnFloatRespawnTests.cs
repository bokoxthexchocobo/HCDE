using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnFloatRespawnTests
{
    [Theory]
    [InlineData("SPAWNFLOAT")]
    [InlineData("Actor.spawnfloat")]
    public void ScriptFlagChangesRuntimeWithoutMovingActorOrChangingClassDefault(string name)
    {
        var sim = Room("NOGRAVITY+6"); var player = sim.Players.Single(); var z = player.Z;
        Assert.True(AcsActorFlags.TrySet(player, name, true)); Assert.True(AcsActorFlags.TryGet(player, name, out var value));
        Assert.True(value); Assert.False(player.DefaultSpawnFloat); Assert.Equal(z, player.Z);
        Assert.Null(sim.CaptureState().FloatSpawnRandomState);
        var saved = SimSavegame.Write(sim, true); player.SpawnFloat = false;
        SimSavegame.Apply(sim, saved); Assert.True(player.SpawnFloat); Assert.False(player.DefaultSpawnFloat);
        Assert.True(AcsActorFlags.TrySet(player, name, false)); Assert.False(player.SpawnFloat);
    }

    [Theory]
    [InlineData("SPAWNFLOAT+NOGRAVITY+6", true, false)]
    [InlineData("SPAWNCEILING+SPAWNFLOAT+NOGRAVITY+6", true, true)]
    [InlineData("NOGRAVITY+6", false, false)]
    public void RespawnUsesClassPlacementAndReplaysFromSave(string bits, bool floating, bool ceiling)
    {
        var sim = Room(bits); var player = sim.Players.Single(); var height = player.Height;
        ActorDamage.Apply(player, 10000); Assert.True(player.IsDead);
        player.SpawnFloat = !floating; player.SpawnCeiling = !ceiling;
        player.RespawnArmed = true; sim.Thinkers.Clock.Restore(player.RespawnEarliestTic);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(bits); SimSavegame.Apply(fresh, saved);
        var restored = fresh.Players.Single(); var randomBefore = sim.CaptureState().FloatSpawnRandomState;
        sim.TryPlayerRespawn(player); fresh.TryPlayerRespawn(restored);
        Assert.False(player.IsDead); Assert.Equal(height, player.Height);
        Assert.Equal(floating, player.SpawnFloat); Assert.Equal(ceiling, player.SpawnCeiling);
        if (ceiling) Assert.Equal(256 - height.ToDouble(), player.Z.ToDouble());
        else if (floating) Assert.InRange(player.Z.ToDouble(), 40, 256 - height.ToDouble());
        else Assert.Equal(0, player.Z.ToDouble());
        Assert.Equal(!floating && !ceiling, player.OnGround);
        Assert.Equal(player.Z, restored.Z);
        Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
        if (ceiling || !floating) Assert.Equal(randomBefore, sim.CaptureState().FloatSpawnRandomState);
        else Assert.NotEqual(randomBefore, sim.CaptureState().FloatSpawnRandomState);
    }

    [Fact]
    public void BlockedRespawnDoesNotChangePlacementFlagsOrConsumeRandom()
    {
        var sim = Room("SPAWNFLOAT+NOGRAVITY+6"); var player = sim.Players.Single();
        ActorDamage.Apply(player, 10000); player.SpawnFloat = false; player.RespawnArmed = true;
        sim.Thinkers.Clock.Restore(player.RespawnEarliestTic); sim.NoRespawn = true;
        var saved = SimSavegame.Write(sim, true); sim.TryPlayerRespawn(player);
        Assert.True(player.IsDead); Assert.False(player.SpawnFloat); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ShortRoomRespawnUsesFloorWithoutRandomDraw()
    {
        var sim = Room("SPAWNFLOAT+NOGRAVITY+6", 104); var player = sim.Players.Single();
        ActorDamage.Apply(player, 10000); player.RespawnArmed = true;
        sim.Thinkers.Clock.Restore(player.RespawnEarliestTic); sim.TryPlayerRespawn(player);
        Assert.False(player.IsDead); Assert.Equal(0, player.Z.ToDouble()); Assert.True(player.OnGround);
        Assert.Null(sim.CaptureState().FloatSpawnRandomState);
    }

    private static AuthoritySimulation Room(string bits, short ceiling = 256)
    {
        var patch = DehackedPatch.Apply($"Thing 1\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = ceiling }],
            Things = [new LevelThing { Type = 1 }] }, rngSeed: 23, dehacked: patch,
            spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
    }
}
