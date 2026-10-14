using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ResurrectionFriendshipHealthTests
{
    [Theory]
    [InlineData(0, 7)]
    [InlineData(1, 20)]
    [InlineData(2, 20)]
    public void FriendshipTransferResetsHealthAfterRevivalEvent(int route, int expected)
    {
        var sim = Room(); var corpse = sim.Actors.Single(a => a.IsMonster);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        int? eventHealth = null;
        sim.WorldThingRevived += actor =>
        {
            if (actor != corpse) return;
            eventHealth = actor.Health; actor.RestoreHealth(7);
        };
        var raiser = new Actor { Friendly = true };
        Assert.True(route == 2 ? ArchvileActions.TryRaise(sim, raiser, sim.Players.Single())
            : ActorRaiseActions.RaiseActor(raiser, corpse, route));
        Assert.Equal(20, eventHealth); Assert.Equal(expected, corpse.Health);
        Assert.Equal(route != 0, corpse.Friendly);
    }

    [Fact]
    public void StandaloneCopyActionStillPreservesHealth()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.IsMonster);
        actor.MasterId = sim.Players.Single().Id; actor.Health = 7;
        ActorPropertyActions.CopyFriendliness(actor); Assert.Equal(7, actor.Health);
    }

    [Fact]
    public void TransferHealthResetDoesNotExecuteAnotherStateAction()
    {
        var sim = Room(); var corpse = sim.Actors.Single(a => a.IsMonster); var entries = 0;
        corpse.States.Configure(corpse, [new(-1, 0, _ => entries++), new(-1, 1), new(-1, 2)], 0);
        corpse.DeathState = 2; corpse.RaiseState = 1; corpse.Health = 0;
        sim.WorldThingRevived += actor => { if (actor == corpse) actor.RestoreHealth(7); };
        Assert.True(ActorRaiseActions.RaiseActor(new Actor(), corpse, 1));
        Assert.Equal(20, corpse.Health); Assert.Equal(1, entries); Assert.Equal(1, corpse.States.Current);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }]
    });
}
