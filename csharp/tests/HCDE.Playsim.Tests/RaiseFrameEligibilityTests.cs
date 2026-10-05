using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseFrameEligibilityTests
{
    [Theory]
    [InlineData(ActorStateMachine.Corpse, -1, false, true)]
    [InlineData(ActorStateMachine.Corpse, 4, false, false)]
    [InlineData(ActorStateMachine.Corpse, 4, true, true)]
    [InlineData(ActorStateMachine.Death, 4, false, false)]
    [InlineData(ActorStateMachine.Death, -1, false, true)]
    [InlineData(ActorStateMachine.Death, 4, true, true)]
    public void QueryAndRaiseRespectCurrentFramePermission(int state, int tics, bool canRaise, bool expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        var frames = new ActorFrame[] { new(-1, 0), new(4, 0), new(6, 3), new(-1, 3) };
        frames[state] = new(tics, ActorStateMachine.Corpse, CanRaise: canRaise);
        corpse.States.Configure(corpse, frames, state);
        corpse.VelocityX = Fixed.FromInt(3);

        Assert.Equal(expected, ActorRaise.CanRaise(sim, corpse));
        Assert.Equal(expected, ThingRaise.Execute(sim, 17, corpse, 0, 2));
        Assert.Equal(!expected, corpse.IsDead);
        Assert.Equal(expected ? default : Fixed.FromInt(3), corpse.VelocityX);
    }

    [Fact]
    public void ArchvileWaitsUntilTimedCorpseFrameSettles()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Configure(corpse,
            [new(-1, 0), new(4, 0), new(6, 3), new(1, 4), new(-1, 4, CanRaise: true)], 3);
        var archvile = new Actor();

        Assert.False(ArchvileActions.TryRaise(sim, archvile, sim.Players.Single()));
        corpse.States.Tick(corpse);
        Assert.True(ArchvileActions.TryRaise(sim, archvile, sim.Players.Single()));
    }
}
