using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorReviveHookTests
{
    private sealed class RevivedActor : Actor
    {
        public Action<Actor>? Callback { get; set; }
        public bool Allow { get; set; } = true;
        public override bool CanResurrect(Actor? other, bool passive) => Allow;
        public override void OnRevive() => Callback?.Invoke(this);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HookRunsAfterRestorationBeforeFriendshipAndDestination(bool explicitRaise)
    {
        var actor = Corpse(); actor.RaiseState = explicitRaise ? 4 : -1;
        actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.RaiseDuration = 20;
        actor.Killed = true; actor.Dormant = true; actor.Shootable = false;
        actor.LastDamageSourceId = 12; actor.VelocityZ = Fixed.FromInt(2);
        var calls = new List<string>();
        actor.Callback = self =>
        {
            calls.Add("revive"); Assert.Equal(100, self.Health);
            Assert.False(self.Corpse); Assert.False(self.Killed); Assert.False(self.Dormant);
            Assert.True(self.Shootable); Assert.Null(self.LastDamageSourceId);
            Assert.Equal(3, self.States.Current); Assert.False(self.Friendly);
            Assert.Equal(Fixed.FromInt(2), self.VelocityZ);
            Assert.Equal(MonsterMode.Raise, self.Brain!.Mode);
            self.Health = 42; self.VelocityZ = Fixed.FromInt(5);
        };
        void Destination(Actor self)
        {
            calls.Add("state"); Assert.True(self.Friendly);
            Assert.Equal(42, self.Health); Assert.Equal(Fixed.FromInt(5), self.VelocityZ);
        }
        actor.States.Configure(actor, [new(-1, 0, Destination), new(4, 0),
            new(6, 3), new(-1, 3), new(7, 0, Destination)], 3);
        Assert.True(ActorRaiseActions.RaiseActor(new Actor { Friendly = true }, actor, 3));
        Assert.Equal(new[] { "revive", "state" }, calls);
    }

    [Fact]
    public void PermissionVetoDoesNotCallReviveHook()
    {
        var actor = Corpse(); actor.Allow = false;
        actor.Callback = _ => throw new InvalidOperationException("Veto must skip revival.");
        Assert.False(ActorRaiseActions.RaiseActor(new Actor(), actor, 2));
        Assert.Equal(0, actor.Health); Assert.True(actor.Corpse);
    }

    [Fact]
    public void EligibilityQueryDoesNotCallReviveHook()
    {
        var actor = Corpse();
        actor.Callback = _ => throw new InvalidOperationException("Query must skip revival.");
        Assert.True(ActorRaise.CanRaise(actor.Simulation!, actor));
        Assert.Equal(0, actor.Health);
    }

    [Fact]
    public void HookFailurePropagatesWithoutDestinationEntry()
    {
        var actor = Corpse(); var entered = 0;
        actor.States.Configure(actor, [new(-1, 0), new(4, 0), new(6, 3),
            new(-1, 3), new(7, 0, _ => entered++)], 3);
        actor.Callback = _ => throw new InvalidOperationException("Revive failure.");
        var error = Assert.Throws<InvalidOperationException>(() => ActorRaiseActions.RaiseSelf(actor, 2));
        Assert.Equal("Revive failure.", error.Message);
        Assert.Equal(0, entered); Assert.Equal(3, actor.States.Current);
        Assert.Equal(100, actor.Health);
    }

    private static RevivedActor Corpse()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 500 }],
        });
        var actor = new RevivedActor { Id = 99, Simulation = sim, Level = sim.Level,
            ResurrectionHealth = 100, RaiseState = 4, Corpse = true };
        actor.RestoreHealth(0);
        actor.States.Configure(actor, [new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(7, 0)], 3);
        return actor;
    }
}
