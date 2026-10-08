using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ResurrectionPermissionTests
{
    private sealed class CheckedActor(string name, List<string> calls, bool allow) : Actor
    {
        public override bool CanResurrect(Actor? other, bool passive)
        {
            calls.Add($"{name}:{passive}");
            return allow;
        }
    }

    [Theory]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void ChecksActiveThenPassiveWithShortCircuit(bool active, bool passive, bool expected)
    {
        var calls = new List<string>();
        var raiser = new CheckedActor("raiser", calls, active);
        var corpse = new CheckedActor("corpse", calls, passive);
        Assert.Equal(expected, ActorRaise.CanResurrect(raiser, corpse));
        Assert.Equal(active ? new[] { "raiser:False", "corpse:True" } : new[] { "raiser:False" }, calls);
    }

    [Fact]
    public void SelfAndNullCorpseRunOnlyActiveCheck()
    {
        var calls = new List<string>(); var actor = new CheckedActor("self", calls, true);
        Assert.True(ActorRaise.CanResurrect(actor, actor));
        Assert.True(ActorRaise.CanResurrect(actor, null));
        Assert.Equal(new[] { "self:False", "self:False" }, calls);
        Assert.False(ActorRaise.CanResurrect(null, actor));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothRoutesRespectVetoBeforeRevival(bool archvile)
    {
        var sim = Room(); var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.VelocityX = Fixed.FromInt(3);
        var calls = new List<string>(); var raiser = new CheckedActor("raiser", calls, false);
        Assert.False(archvile ? ArchvileActions.TryRaise(sim, raiser, corpse)
            : ActorRaiseActions.RaiseActor(raiser, corpse, 2));
        Assert.Equal(new[] { "raiser:False" }, calls);
        Assert.Equal(0, corpse.Health); Assert.True(corpse.Corpse);
        Assert.Equal(3, corpse.States.Current); Assert.Equal(default, corpse.VelocityX);
    }

    [Fact]
    public void BlockedPositionSkipsPermissionButBypassDoesNotBypassVeto()
    {
        var sim = Room(); var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.X = sim.Players.Single().X;
        var calls = new List<string>(); var raiser = new CheckedActor("raiser", calls, false);
        Assert.False(ActorRaiseActions.RaiseActor(raiser, corpse)); Assert.Empty(calls);
        Assert.False(ActorRaiseActions.RaiseActor(raiser, corpse, 2));
        Assert.Single(calls); Assert.True(corpse.IsDead);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
