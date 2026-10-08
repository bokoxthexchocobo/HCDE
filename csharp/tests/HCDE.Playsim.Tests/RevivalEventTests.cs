using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RevivalEventTests
{
    private sealed class RevivedActor : Actor
    {
        public Action? Callback { get; set; }
        public override void OnRevive() => Callback?.Invoke();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NotificationFollowsHookBeforeFriendshipAndState(bool explicitRaise)
    {
        var sim = Room(); var actor = Corpse(sim);
        if (!explicitRaise) { actor.RaiseState = -1; actor.RaiseDuration = 20; actor.Brain = new(MonsterAttack.Melee); }
        var calls = new List<string>();
        actor.Callback = () => { calls.Add("hook"); actor.VelocityZ = Fixed.FromInt(5); };
        sim.WorldThingRevived += revived =>
        {
            Assert.Same(actor, revived); calls.Add("event");
            Assert.Equal(100, revived.Health); Assert.False(revived.Corpse);
            Assert.Equal(3, revived.States.Current); Assert.False(revived.Friendly);
            Assert.Equal(Fixed.FromInt(5), revived.VelocityZ);
        };
        void Destination(Actor revived) { calls.Add("state"); Assert.True(revived.Friendly); }
        actor.States.Configure(actor, [new(-1, 0, Destination), new(4, 0), new(6, 3),
            new(-1, 3), new(7, 0, Destination)], 3);
        Assert.True(ActorRaiseActions.RaiseActor(new Actor { Friendly = true }, actor, 3));
        Assert.Equal(new[] { "hook", "event", "state" }, calls);
    }

    [Fact]
    public void HookFailureDoesNotEmitNotification()
    {
        var sim = Room(); var actor = Corpse(sim); var calls = 0;
        sim.WorldThingRevived += _ => calls++;
        actor.Callback = () => throw new InvalidOperationException("Hook failure.");
        Assert.Throws<InvalidOperationException>(() => ActorRaiseActions.RaiseSelf(actor, 2));
        Assert.Equal(0, calls);
    }

    [Fact]
    public void NotificationFailurePropagatesWithoutEnteringDestination()
    {
        var sim = Room(); var actor = Corpse(sim);
        sim.WorldThingRevived += _ => throw new InvalidOperationException("Event failure.");
        var error = Assert.Throws<InvalidOperationException>(() => ActorRaiseActions.RaiseSelf(actor, 2));
        Assert.Equal("Event failure.", error.Message); Assert.Equal(3, actor.States.Current);
    }

    [Fact]
    public void QueriesAndBlockedRaisesDoNotEmitNotification()
    {
        var sim = Room(); var actor = Corpse(sim); var calls = 0;
        sim.WorldThingRevived += _ => calls++;
        Assert.True(ActorRaise.CanRaise(sim, actor));
        actor.X = sim.Players.Single().X;
        Assert.False(ActorRaiseActions.RaiseSelf(actor));
        Assert.Equal(0, calls);
    }

    private static RevivedActor Corpse(AuthoritySimulation sim)
    {
        var actor = new RevivedActor { Id = 99, Simulation = sim, Level = sim.Level,
            ResurrectionHealth = 100, RaiseState = 4, Corpse = true };
        actor.RestoreHealth(0);
        actor.States.Configure(actor, [new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(7, 0)], 3);
        return actor;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 500 }],
    });
}
