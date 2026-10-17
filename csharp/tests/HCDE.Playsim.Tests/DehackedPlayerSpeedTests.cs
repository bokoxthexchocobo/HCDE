using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedPlayerSpeedTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("2", 2)]
    [InlineData("98304 trailing", 1.5)]
    [InlineData("-1", -1)]
    [InlineData("-65536", -1)]
    [InlineData("-256", -256.0 / 65536)]
    public void PlayerSpeedControlsActualThrustAndResumedMovement(string number, double expected)
    {
        var patch = DehackedPatch.Apply($"Thing 1\nSpeed = {number}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var player = sim.Players.Single(); Assert.Equal(expected, player.MovementSpeed.ToDouble());
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(expected, player.X.ToDouble());
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        fresh.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); fresh.Tick();
        Assert.Equal(player.X, fresh.Players.Single().X); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EveryPlayerStartUsesThingOneClassDefaults(ushort start)
    {
        var patch = DehackedPatch.Apply("Thing 1\nSpeed = 2\nFast speed = 3\n");
        var level = new PlayLevel { Things = [new LevelThing { Type = start }] };
        var normal = AuthoritySimulation.Start(level, dehacked: patch);
        var fast = AuthoritySimulation.Start(level, dehacked: patch, spawnOptions: new SpawnOptions(FastMonsters: true));
        Assert.Equal(2, normal.Players.Single().MovementSpeed.ToDouble());
        Assert.Equal(3, fast.Players.Single().MovementSpeed.ToDouble());
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 3)]
    public void FastModeOverridesNormalSpeedWhileNegativeFastDefaultRetainsIt(bool fast, double expected)
    {
        var original = DehackedPatch.Apply("Thing 1\nSpeed = 2\nFast speed = 3\n");
        var inherited = DehackedPatch.Apply("Thing 1\nMass = 200\n", original);
        Assert.Equal(expected, Room(inherited, fast).Players.Single().MovementSpeed.ToDouble());
        var disabled = DehackedPatch.Apply("Thing 1\nFast speed = -1\n", inherited);
        Assert.Equal(2, Room(disabled, fast).Players.Single().MovementSpeed.ToDouble());
        Assert.Equal(1, Room(DehackedPatch.Apply("Thing 1\nMass = 200\n"), fast).Players.Single().MovementSpeed.ToDouble());
    }

    [Fact]
    public void SavedRuntimeSpeedOverrideRemainsIndependentOfPatchedSpawnDefault()
    {
        var patch = DehackedPatch.Apply("Thing 1\nSpeed = 2\n");
        var sim = Room(patch); sim.Players.Single().MovementSpeed = Fixed.FromInt(5);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(5, fresh.Players.Single().MovementSpeed.ToDouble());
        Assert.Equal(2, Room(patch).Players.Single().MovementSpeed.ToDouble());
    }

    [Theory]
    [InlineData("2147483648")]
    [InlineData("4294967295")]
    public void UnrepresentablePlayerSpeedReportsErrorWithoutReplacingInheritedDefault(string number)
    {
        var original = DehackedPatch.Apply("Thing 1\nSpeed = 2\n");
        var patch = DehackedPatch.Apply($"Thing 1\nSpeed = {number}\n", original);
        Assert.Contains(patch.Errors, error => error.Contains("fixed-point range", StringComparison.Ordinal));
        Assert.Equal(2, patch.Actors.Single(a => a.Index == 1).Speed);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch, bool fast = false) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: patch, spawnOptions: new SpawnOptions(FastMonsters: fast));
}
