using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullSlamStateTests
{
    [Theory]
    [InlineData(false, true, 3)]
    [InlineData(false, false, 4)]
    [InlineData(true, true, 4)]
    [InlineData(true, false, 4)]
    public void CustomSlamAndIdleLabelsFollowNativePriorityAndSave(bool dormant, bool hasSlam, int expected)
    {
        var sim = Room(); var soul = Soul(sim); var entries = new int[5];
        soul.States.Configure(soul, Enumerable.Range(0, 5)
            .Select(i => new ActorFrame(11, i, _ => entries[i]++)), 2);
        Array.Clear(entries);
        soul.SpawnState = 0; soul.SeeState = hasSlam ? 1 : -1;
        soul.SlamState = hasSlam ? 3 : -1; soul.IdleState = 4; soul.PainState = -1;
        soul.Brain!.StartCharge(soul, sim.Players.Single()); soul.Dormant = dormant;
        var before = SimSavegame.Write(sim);
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.Equal(expected, soul.States.Current);
        Assert.Equal(dormant ? -1 : 11, soul.States.RemainingTics);
        Assert.Equal(1, entries[expected]); Assert.Equal(1, entries.Sum());
        var health = sim.Players.Single().Health;
        SimSavegame.Apply(sim, before); Array.Clear(entries);
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.Equal(expected, soul.States.Current); Assert.Equal(health, sim.Players.Single().Health);
        Assert.Equal(1, entries[expected]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SetIdleUsesCustomLabelAndHonorsNoFunction(bool noFunction)
    {
        var actor = new Actor(); var entries = 0;
        actor.States.Configure(actor, [new ActorFrame(-1, 0), new ActorFrame(7, 1, _ => entries++)], 0);
        actor.IdleState = 1; actor.SetIdle(noFunction);
        Assert.Equal(1, actor.States.Current); Assert.Equal(7, actor.States.RemainingTics);
        Assert.Equal(noFunction ? 0 : 1, entries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CollisionEntersSeeOrSpawnAndRunsEntryAction(bool hasSee)
    {
        var sim = Room(); var soul = Soul(sim);
        var spawnEntries = 0; var seeEntries = 0;
        soul.States.Configure(soul,
            [new ActorFrame(9, 0, _ => spawnEntries++), new ActorFrame(7, 1, _ => seeEntries++), new ActorFrame(-1, 2)], 2);
        soul.SpawnState = 0; soul.SeeState = hasSee ? 1 : -1; soul.PainState = -1;
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.False(soul.Brain.Charging);
        Assert.Equal(hasSee ? 1 : 0, soul.States.Current);
        Assert.Equal(hasSee ? 7 : 9, soul.States.RemainingTics);
        Assert.Equal(hasSee ? 0 : 1, spawnEntries); Assert.Equal(hasSee ? 1 : 0, seeEntries);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DormantCollisionEntersSpawnAndHoldsAcrossSave(bool hasSee)
    {
        var sim = Room(); var soul = Soul(sim); var entries = 0;
        soul.States.Configure(soul,
            [new ActorFrame(9, 0, _ => entries++), new ActorFrame(7, 1), new ActorFrame(-1, 2)], 2);
        soul.SpawnState = 0; soul.SeeState = hasSee ? 1 : -1; soul.PainState = -1;
        soul.Brain!.StartCharge(soul, sim.Players.Single()); soul.Dormant = true;
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.False(soul.Brain.Charging); Assert.Equal(0, soul.States.Current);
        Assert.Equal(-1, soul.States.RemainingTics); Assert.Equal(1, entries);
        Assert.Equal(100, sim.Players.Single().Health);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
        var bytes = SimSavegame.Write(sim);
        soul.States.Enter(soul, 2); SimSavegame.Apply(sim, bytes);
        var afterRestore = entries;
        for (var i = 0; i < 12; i++) soul.Tick();
        Assert.Equal(0, soul.States.Current); Assert.Equal(-1, soul.States.RemainingTics);
        Assert.Equal(afterRestore, entries);
    }

    private static Actor Soul(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3006);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3006 }] });
}
