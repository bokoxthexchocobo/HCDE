using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPostBeginPlayTests
{
    private sealed class HookActor : Actor
    {
        public Action? Hook { get; set; }
        public override void PostBeginPlay() => Hook?.Invoke();
    }

    [Fact]
    public void FreshActorArmsNoDelayBeforeSpawnEventAndFirstTick()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Brain = null;
        var calls = new List<string>();
        actor.States.Configure(actor, [new(-1, 0, _ => calls.Add("action"), NoDelay: true)], 0);
        calls.Clear();
        sim.WorldThingSpawned += spawned =>
        {
            Assert.Same(actor, spawned); Assert.True(spawned.HandleNoDelay);
            Assert.False(spawned.JustSpawned); Assert.Equal(0, spawned.TickCount);
            calls.Add("event");
        };
        sim.Thinkers.Run(); sim.Thinkers.Run();
        Assert.Equal(new[] { "event", "action" }, calls);
        Assert.Equal(1, actor.PostBeginCount); Assert.False(actor.HandleNoDelay);
    }

    [Fact]
    public void OverrideWithoutBaseStillEmitsEventAfterHook()
    {
        var sim = Room(); var actor = new HookActor { Simulation = sim };
        var calls = new List<string>(); actor.Hook = () => calls.Add("hook");
        sim.WorldThingSpawned += spawned => { Assert.Same(actor, spawned); calls.Add("event"); };
        actor.CallPostBeginPlay();
        Assert.Equal(new[] { "hook", "event" }, calls);
        Assert.False(actor.HandleNoDelay); Assert.Equal(0, actor.PostBeginCount);
    }

    [Fact]
    public void HookFailurePreventsEventAndFirstTick()
    {
        var sim = Room(); var actor = new HookActor { Simulation = sim };
        actor.Hook = () => throw new InvalidOperationException("Hook failure.");
        var events = 0; sim.WorldThingSpawned += _ => events++;
        var thinkers = new ThinkerCollection(); thinkers.Add(actor);
        Assert.Throws<InvalidOperationException>(() => thinkers.Run());
        Assert.Equal(0, events); Assert.Equal(0, actor.TickCount);
    }

    [Fact]
    public void EventDestructionSkipsFirstTick()
    {
        var sim = Room(); var actor = sim.Actors[0];
        var events = 0; sim.WorldThingSpawned += spawned => { events++; spawned.Destroy(); };
        sim.Thinkers.Run(); sim.Thinkers.Run();
        Assert.Equal(1, events); Assert.Equal(0, actor.TickCount);
        Assert.Empty(sim.Thinkers.ThinkersIn(ThinkerStat.Default));
    }

    [Fact]
    public void EventFailurePropagatesBeforeFirstTick()
    {
        var sim = Room(); var actor = sim.Actors[0];
        sim.WorldThingSpawned += _ => throw new InvalidOperationException("Event failure.");
        var error = Assert.Throws<InvalidOperationException>(() => sim.Thinkers.Run());
        Assert.Equal("Event failure.", error.Message); Assert.Equal(0, actor.TickCount);
        Assert.True(actor.HandleNoDelay);
    }

    [Fact]
    public void DormantFreshActorKeepsPendingNoDelayUntilAwakened()
    {
        var actor = new Actor { Dormant = true }; var calls = 0;
        actor.States.Configure(actor, [new(-1, 0, _ => calls++, NoDelay: true)], 0);
        calls = 0;
        var thinkers = new ThinkerCollection(); thinkers.Add(actor);
        thinkers.Run(); Assert.True(actor.HandleNoDelay); Assert.Equal(0, calls);
        actor.Dormant = false; thinkers.Run(); thinkers.Run();
        Assert.Equal(1, calls); Assert.False(actor.HandleNoDelay);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] });
}
