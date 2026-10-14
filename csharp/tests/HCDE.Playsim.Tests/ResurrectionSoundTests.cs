using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ResurrectionSoundTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void SuccessfulRaiseEmitsNativeBodySound(int route)
    {
        var sim = Room(); var corpse = Ready(sim); sim.DrainSoundRequests();
        var raised = route switch
        {
            0 => ActorRaiseActions.RaiseSelf(corpse),
            1 => ThingRaise.Execute(sim, 17, corpse, 0, 0) == true,
            _ => ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
        };
        Assert.True(raised); var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal(corpse.Id, sound.ActorId); Assert.Equal("vile/raise", sound.Sound);
        Assert.Equal(4, sound.Channel); Assert.Equal(1.001f, sound.Attenuation); Assert.Equal(1, sound.Volume);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlockedRaisesAreSilent(bool archvile)
    {
        var sim = Room(); var corpse = Ready(sim); sim.Ceilings[0] = 10; sim.DrainSoundRequests();
        Assert.False(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ActorRaiseActions.RaiseSelf(corpse));
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void ReviveHookSeesSoundAlreadyQueued()
    {
        var sim = Room(); var actor = new ObservingActor
        { Simulation = sim, ResurrectionHealth = 100, Corpse = true, RaiseState = ActorStateMachine.Spawn };
        Assert.True(ActorRaiseActions.RaiseSelf(actor, 2)); Assert.True(actor.Observed);
    }

    private sealed class ObservingActor : Actor
    {
        public bool Observed { get; private set; }
        public override void OnRevive() { Observed = Simulation!.PendingSoundRequests.Any(s => s.Sound == "vile/raise"); }
    }

    private static Actor Ready(AuthoritySimulation sim)
    {
        var actor = sim.Actors.Single(a => a.IsMonster); actor.Health = 0;
        actor.States.Enter(actor, ActorStateMachine.Corpse); return actor;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }]
    });
}
