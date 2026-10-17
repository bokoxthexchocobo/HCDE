using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFastSpeedTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("12", 12)]
    [InlineData("255", 255)]
    [InlineData("256", 256.0 / 65536)]
    [InlineData("98304 trailing", 1.5)]
    [InlineData("-1", -1)]
    [InlineData("-65536", -65536)]
    public void NativeSpeedNumberConventionUsesFastValueOnlyWhenEnabled(string value, double fast)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nSpeed = 4\nFast speed = {value}\n"); Assert.Empty(patch.Errors);
        Assert.Equal(fast, patch.Actors.Single(a => a.Index == 2).FastSpeed);
        Assert.Equal(4, Room(patch, false).AddBot(0, 0).MovementSpeed.ToDouble());
        Assert.Equal(fast >= 0 ? fast : 4, Room(patch, true).AddBot(0, 0).MovementSpeed.ToDouble());
    }

    [Theory]
    [InlineData(2, null, 4)]
    [InlineData(4, null, 12)]
    [InlineData(4, false, 4)]
    [InlineData(2, true, 12)]
    public void MapAndDynamicSpawnsFollowGlobalFastSetting(int skill, bool? fast, double expected)
    {
        var patch = DehackedPatch.Apply("Thing 1\nSpeed = 4\nFast speed = 12\n\nThing 2\nSpeed = 4\nFast speed = 12\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 200 }] },
            dehacked: patch, spawnOptions: new SpawnOptions(Skill: skill, FastMonsters: fast));
        Assert.All(sim.Actors, actor => Assert.Equal(expected, actor.MovementSpeed.ToDouble()));
        var actor = sim.AddBot(400, 0); Assert.Equal(expected, actor.MovementSpeed.ToDouble());
        actor.AlwaysFast = true; actor.NeverFast = true; Assert.Equal(expected, actor.MovementSpeed.ToDouble());
    }

    [Theory]
    [InlineData(false, 8)]
    [InlineData(true, 12)]
    public void ProjectileVelocityUsesSelectedSpeedAndFreshSaveKeepsRuntimeOverride(bool fast, double expected)
    {
        var patch = DehackedPatch.Apply("Thing 34\nSpeed = 8\nFast speed = 12\n");
        var sim = Room(patch, fast); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal(expected, missile.Speed); Assert.Equal(expected, missile.VelocityX.ToDouble());
        var x = missile.X.ToDouble(); missile.Tick(); Assert.Equal(expected, missile.X.ToDouble() - x);
        missile.MovementSpeed = Fixed.FromInt(6);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch, fast); SimSavegame.Apply(fresh, saved);
        var loaded = Assert.IsType<ProjectileActor>(fresh.Actors.Single(a => a.Id == missile.Id));
        Assert.Equal(6, loaded.Speed); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(expected, fresh.SpawnProjectile(fresh.Players.Single(), ProjectileKind.Rocket).Speed);
    }

    [Fact]
    public void SpawnedLostSoulReceivesFastDefaultAndSavedRuntimeSpeed()
    {
        var patch = DehackedPatch.Apply("Thing 19\nFast speed = 12\n");
        var sim = Room(patch, true); var parent = sim.AddBot(200, 0, 71); parent.Brain = null;
        var soul = sim.SpawnLostSoul(parent, null, 0); Assert.NotNull(soul);
        Assert.Equal(12, soul.MovementSpeed.ToDouble()); soul.MovementSpeed = Fixed.FromInt(7);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch, true); SimSavegame.Apply(fresh, saved);
        Assert.Equal(7, fresh.Actors.Single(a => a.Id == soul.Id).MovementSpeed.ToDouble());
    }

    [Fact]
    public void LayeringRetainsFastSpeedAndNegativeReplacementDisablesIt()
    {
        var original = DehackedPatch.Apply("Thing 2\nSpeed = 4\nFast speed = 12\n");
        var inherited = DehackedPatch.Apply("Thing 2\nMass = 200\n", original);
        Assert.Equal(12, Room(inherited, true).AddBot(200, 0).MovementSpeed.ToDouble());
        var disabled = DehackedPatch.Apply("Thing 2\nFast speed = -1\n", inherited);
        Assert.Equal(4, Room(disabled, true).AddBot(200, 0).MovementSpeed.ToDouble());
        Assert.Equal(12, original.Actors.Single(a => a.Index == 2).FastSpeed);
        Assert.Equal(Room(DehackedPatch.Apply(""), false).AddBot(200, 0).MovementSpeed,
            Room(DehackedPatch.Apply(""), true).AddBot(200, 0).MovementSpeed);
    }

    [Theory]
    [InlineData("2147483648")]
    [InlineData("4294967295")]
    public void UnrepresentableFastSpeedReportsErrorWithoutOverflow(string value)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nFast speed = {value}\n");
        Assert.Contains(patch.Errors, error => error.Contains("fixed-point range", StringComparison.Ordinal));
        Assert.Null(patch.Actors.Single(a => a.Index == 2).FastSpeed);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch, bool fast) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: patch, spawnOptions: new SpawnOptions(FastMonsters: fast));
}
