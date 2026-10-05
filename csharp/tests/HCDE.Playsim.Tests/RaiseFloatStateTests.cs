using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseFloatStateTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalClearsFloatStateAndBlockedRaisePreservesIt(bool archvile, bool blocked)
    {
        var sim = Room();
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.InFloat = corpse.VerticalFriction = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.InFloat);
        Assert.Equal(blocked, corpse.VerticalFriction);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RevivedActorDoesNotRetainTemporaryVerticalDrag(bool archvile)
    {
        var sim = Room();
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.InFloat = corpse.VerticalFriction = true;
        corpse.VelocityZ = Fixed.FromInt(8);
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        corpse.NoGravity = true;
        corpse.Brain!.Enabled = false;
        ActorPhysics.Step(sim, corpse);
        Assert.Equal(Fixed.FromInt(8), corpse.VelocityZ);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
